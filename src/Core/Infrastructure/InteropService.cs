using Microsoft.JSInterop;

namespace Blazor.Monaco;

public class InteropService
{
    private readonly GlobalState _globalState;
    private readonly IJSRuntime _jsRuntime;
    private bool _interopRegistered = false;
    public bool InteropRegistered => _globalState.InteropInDom;
    public bool LoaderRegistered => _globalState.LoaderInDom;
    public DateTime? FirstLoadTime => _globalState.FirstLoadTime;

    public InteropService(GlobalState globalState, IJSRuntime jsRuntime)
    {
        _globalState = globalState;
        _jsRuntime = jsRuntime;
    }

    private Task<bool> InjectScriptAsync(string scriptSrc, string testVar)
    {
        var script =
            $$$"""
               (() => {
                   return new Promise((resolve, reject) => {
                       if (!document.querySelector('script[src="{{{scriptSrc}}}"]')) {
                           const scriptTag = document.createElement('script');
                           scriptTag.src = '{{{scriptSrc}}}';
                           scriptTag.onload = () => {
                               if ({{{testVar}}}!==undefined) {
                                   {{{testVar}}}=true;
                               }
                               if ({{{(scriptSrc == "https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.52.0/min/vs/loader.js").ToString().ToLower()}}}) {
                                   require.config({ paths: { 'vs': '{{{scriptSrc.Replace(@"/loader.js", "")}}}' } });
                                   require(['vs/editor/editor.main'], function () {
                                       monacoInterop.monacoEditorScriptLoaded = true;
                                       console.log("Monaco Editor Scripts Loaded Successfully.");
                                       resolve(true);
                                   });
                               }
                               resolve(true);
                           };
                           scriptTag.onerror = () => reject(new Error('Failed to load script: {{{scriptSrc}}}'));
                           document.head.appendChild(scriptTag);
                           } else {
                           resolve(true);
                           }
                   });
               })();
               """;
        // Console.WriteLine(script);
        return _jsRuntime.InvokeAsync<bool>("eval", script).AsTask();
    }

    private async Task<bool> LoadAndVerifyScriptAsync(string scriptSrc, string verifyFunction, TimeSpan timeout,
        TimeSpan pollInterval)
    {
        var scriptLoadTask = InjectScriptAsync(scriptSrc, verifyFunction);
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

    private async Task WaitForMonacoInteropAsync()
    {
        var isInteropScriptLoaded = await LoadAndVerifyScriptAsync(
            "_content/Blazor.Monaco/monacoInterop.js",
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