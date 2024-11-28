using Microsoft.JSInterop;

namespace Blazor.Monaco;

public class InteropService
{
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

    private Task<bool> InjectScriptAsync(string monacoLoaderSource = "local")
    {
        var script =
            $$$"""
               ((loaderSource) => {
                   return new Promise((resolve, reject) => {
                       let scriptName = "monacoInterop";
                       let interopScriptTag = document.querySelector(`script[src*="{scriptName}"]`)
                       const inDom = (interopScriptTag !== null);
                       if (!inDom) {
                           interopScriptTag = document.createElement("script");
                           const importMap = JSON.parse(document.querySelector('script[type="importmap"]').textContent).imports;
                           const partialMatchKey = Object.keys(importMap).find(key => key.includes(scriptName));
               
                           if (!partialMatchKey) {
                               interopScriptTag.onerror = () => reject(new Error('Failed to load script: ' + scriptName));
                           }
                           resolvedPath = importMap[partialMatchKey].replace("./", "/");
                           interopScriptTag.src = resolvedPath;
                           interopScriptTag.type = "module";
                           console.log("About to load " + scriptName); 1
                           interopScriptTag.onload = () => {
                               window.monacoInterop.setMonacoLoaderSource(loaderSource); 
                               console.table(monacoInterop)
                               resolve(true);
                           };
                           interopScriptTag.onerror = () => reject(new Error('Failed to load script: ' + scriptName));
                           document.head.appendChild(interopScriptTag);
                       } 
                   });
               })("{{{monacoLoaderSource}}}");
               """;
        return _jsRuntime.InvokeAsync<bool>("eval", script).AsTask();
    }

    private async Task<bool> LoadAndVerifyScriptAsync(string scriptSrc, string verifyFunction, TimeSpan timeout,
        TimeSpan pollInterval)
    {
        var scriptLoadTask = InjectScriptAsync(scriptSrc);
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


    private async Task WaitForMonacoInteropAsync()
    {
        var isInteropScriptLoaded = await LoadAndVerifyScriptAsync(
            "https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.52.0/min/vs/loader.js",
            "monacoInterop.testIsMonacoLoaded()",
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

    public async Task<string> GetEditorContent(string elementId, bool resetChangedOnRead = false)
    {
        return await _jsRuntime.InvokeAsync<string>("monacoInterop.getEditorContent", elementId, resetChangedOnRead);
    }

    public async Task UpdateEditorConfiguration(string elementId, EditorOptions editorOptions)
    {
        await _jsRuntime.InvokeVoidAsync("monacoInterop.updateEditorConfiguration", elementId, editorOptions);
    }
}