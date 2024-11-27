using Microsoft.JSInterop;

namespace Blazor.Monaco;

public class InteropService
{
    private const string JAVASCRIPT_FILE = "./_content/Blazor.Monaco.Test/monacoInterop.js";
    private IJSObjectReference _jsModule = default!;

    private readonly GlobalState _globalState;
    private readonly IJSRuntime _jsRuntime;
    private readonly LibraryConfiguration _libraryConfiguration;
    public bool InteropRegistered => _globalState.InteropInDom;
    public bool LoaderRegistered => _globalState.LoaderInDom;
    public DateTime? FirstLoadTime => _globalState.FirstLoadTime;

    public InteropService(GlobalState globalState, IJSRuntime jsRuntime, LibraryConfiguration libraryConfiguration)
    {
        _globalState = globalState;
        _jsRuntime = jsRuntime;
        _libraryConfiguration = libraryConfiguration;
    }

    private Task<bool> InjectScriptAsync(string scriptName, string scriptSrc = "local")
    {
        var script =
            $$$"""
               ((scriptName, cdnPath = "local") => {
                   return new Promise((resolve, reject) => {
                       let script = document.querySelector(`script[src*="{scriptName}"]`)
                       const inDom = (script);
                       if (!inDom) {
                           script = document.createElement("script");
                           let resolvedPath = cdnPath;
               
                           if (cdnPath.substring(0,4) !== "http") {
                               const importMap = JSON.parse(document.querySelector('script[type="importmap"]').textContent).imports;
                               const partialMatchKey = Object.keys(importMap).find(key => key.includes(scriptName));
               
                               if (!partialMatchKey) {
                                   script.onerror = () => reject(new Error('Failed to load script: ' + scriptName));
                               }
                               resolvedPath = importMap[partialMatchKey].replace("./", "/");
                           }
                           script.src = resolvedPath;
                           script.type = "module";
                           console.log("About to load" + scriptName);
                           script.onload = () => () => {
                            console.log("loading" + scriptName);
                           
                               if (scriptName === "loader") {
                                   const regex = /\/loader\.js$/i;
                                   const configPath = resolvedPath.replace(regex, '');
                                    console.log("About to configPath" + configPath);
                                   require.config({paths: {'vs': configPath}});
                                   require(['vs/editor/editor.main'], function () {
                                       monacoInterop.monacoEditorScriptLoaded = true;
                                       console.log("Monaco Editor Scripts Loaded Successfully.");
                                   });
                               }
                               resolve(true);
                           };
                           script.onerror = () => reject(new Error('Failed to load script: ' + scriptName));
                           document.head.appendChild(script);
                       } else {
                           if (scriptName === "loader") {
                               const regex = /\/loader\.js$/i;
                               const configPath = resolvedPath.replace(regex, '');
                               console.log(configPath);
                               require.config({paths: {'vs': configPath}});
                               require(['vs/editor/editor.main'], function () {
                                   monacoInterop.monacoEditorScriptLoaded = true;
                                   console.log("Monaco Editor Scripts Loaded Successfully.");
                               });
                           }
                       }
                       resolve(true);
                   });
               
               })("{{{scriptName}}}","{{{scriptSrc}}}");
               """;
         Console.WriteLine(script);
        return _jsRuntime.InvokeAsync<bool>("eval", script).AsTask();
    }

    private async Task<bool> LoadAndVerifyScriptAsync(string scriptName ,string scriptSrc, string verifyFunction, TimeSpan timeout,
        TimeSpan pollInterval)
    {
        var scriptLoadTask = InjectScriptAsync(scriptName, scriptSrc);
        var startTime = DateTime.UtcNow;

        while (DateTime.UtcNow - startTime < timeout)
        {
            if (await scriptLoadTask || await _jsRuntime.InvokeAsync<bool>(verifyFunction))
            {
                return true;
            }

            await Task.Delay(pollInterval);
        }

        return false;
    }

    private async Task WaitForMonacoLoaderAsync()
    {
        var isInteropScriptLoaded = await LoadAndVerifyScriptAsync(
            "loader",
            "https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.52.0/min/vs/loader.js",
            "monacoInterop.isMonacoLoaderScriptLoaded",
            TimeSpan.FromSeconds(10),
            TimeSpan.FromMilliseconds(100));

        if (!isInteropScriptLoaded)
        {
            throw new InvalidOperationException("Failed to load Monaco Interop script within the timeout period.");
        }

        _globalState.LoaderInDom = true;
    }

    private async Task InitializeInterop()
    {
        var jsFile = "./_content/Blazor.Monaco.Test/monacoInterop.js";
        var script =
               """
               ((scriptName) => {
                   return new Promise((resolve, reject) => {
                   document.querySelector(`script[src*="{scriptName}"]`)
                       const importMap = JSON.parse(document.querySelector('script[type="importmap"]').textContent).imports;
                       const partialMatchKey = Object.keys(importMap).find(key => key.includes(scriptName));
                       
                       if (partialMatchKey) {
                           const resolvedPath = importMap[partialMatchKey].replace("./","/");
                           const script = document.createElement("script");
                           script.src = resolvedPath;
                           script.type = "module";
                           script.onload = () => () => {
                            if (scriptName === "loader.js"){
                                
                            }
                           //    if ({{testVar}}}!==undefined) {
                           //        {{testVar}}}=true;
                           //    }
                           //    if ({{(scriptSrc == "https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.52.0/min/vs/loader.js").ToString().ToLower()}}}) {
                           //        require.config({ paths: { 'vs': '{{scriptSrc.Replace(@"/loader.js", "")}}}' } });
                           //        require(['vs/editor/editor.main'], function () {
                           //            monacoInterop.monacoEditorScriptLoaded = true;
                           //            console.log("Monaco Editor Scripts Loaded Successfully.");
                           //            resolve(true);
                           //        });
                           //    }
                               resolve(true);
                           };
                           script.onerror = () => reject(new Error('Failed to load script: monacoInterop.js'));
                           document.head.appendChild(script);
                       } else {
                           script.onerror = () => reject(new Error('Failed to load script: monacoInterop.js'));
                       }
                   });
               })();
               """;
        Console.WriteLine(JAVASCRIPT_FILE.FormatCollocatedUrl(_libraryConfiguration));
        await _jsRuntime.InvokeVoidAsync("eval", $"console.log('{jsFile}');");
        var loadScriptResponse = await _jsRuntime.InvokeAsync<bool>("eval", script);
        Console.WriteLine( $"loadScriptResponse {loadScriptResponse}");
        _jsModule = await _jsRuntime.InvokeAsync<IJSObjectReference>("load",
            JAVASCRIPT_FILE.FormatCollocatedUrl(_libraryConfiguration));
        _globalState.InteropInDom = true;
        await _jsModule.InvokeVoidAsync("monacoInterop.printHello");
    }

    private async Task WaitForMonacoInteropAsync()
    {
        var isInteropScriptLoaded = await LoadAndVerifyScriptAsync(
            "monacoInterop",
            "local",
            "monacoInterop.isMonacoInteropScriptLoaded",
            TimeSpan.FromSeconds(10),
            TimeSpan.FromMilliseconds(100));

        if (!isInteropScriptLoaded)
        {
            throw new InvalidOperationException("Failed to load Monaco Interop script within the timeout period.");
        }

        _globalState.InteropInDom = true;
    }

    public async Task EnsureScriptsLoadedAsync()
    {
        if (!_globalState.InteropInDom)
        {
            // await InitializeInterop();
            await WaitForMonacoInteropAsync();
        }

        if (!_globalState.LoaderInDom)
        {
            await WaitForMonacoLoaderAsync();
        }
    }

    public async Task InitializeMonacoEditor(string elementId, string initialCode, EditorOptions options,
        DotNetObjectReference<MonacoEditor> dotnetRef)
    {
        try
        {
            var editorOptions = options.ToJson();
            await _jsRuntime.InvokeVoidAsync("monacoInterop.initializeMonacoEditorInstance", elementId, initialCode,
                editorOptions, dotnetRef);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            throw new InvalidOperationException("An error occurred while initializing the Monaco Editor.", ex);
        }
    }

    public async Task SetEditorContent(string elementId, string newContent)
    {
        await _jsRuntime.InvokeVoidAsync("monacoInterop.setEditorContent", elementId, newContent);
    }

    public async Task<string> GetEditorContent(string elementId)
    {
        return await _jsRuntime.InvokeAsync<string>("monacoInterop.getEditorContent", elementId);
    }

    public async Task UpdateEditorConfiguration(string elementId, EditorOptions editorOptions)
    {
        await _jsRuntime.InvokeVoidAsync("monacoInterop.updateEditorConfiguration", elementId, editorOptions);
    }
}