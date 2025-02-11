using Microsoft.JSInterop;

namespace Blazor.Monaco;

public class InteropService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly GlobalState _globalState;
    private readonly LibraryConfiguration _libraryConfiguration;
    private bool _isInitialized;
    private bool _isInitializing;

    public InteropService(IJSRuntime jsRuntime, GlobalState globalState, LibraryConfiguration libraryConfiguration)
    {
        _jsRuntime = jsRuntime;
        _globalState = globalState;
        _libraryConfiguration = libraryConfiguration;
    }

    public async Task InitializeAsync()
    {
        var loaderUrl = _libraryConfiguration.MonacoLoaderUrl;
        if (_isInitialized)
        {
            Console.WriteLine("MonacoInit: Monaco Editor already initialized. Skipping initialization.");
            return;
        }

        if (_isInitializing)
        {
            Console.WriteLine("MonacoInit: Monaco Editor initialization in progress. Waiting for completion...");
            while (_isInitializing)
            {
                await Task.Delay(100); 
            }

            if (_isInitialized)
            {
                Console.WriteLine("MonacoInit: Monaco Editor successfully initialized by another caller.");
                return;
            }
        }

        try
        {
            _isInitializing = true;

            var scriptPath = $"./_content/Blazor.Monaco/monacoInterop.js";
            
            await _jsRuntime.InvokeVoidAsync("import", scriptPath);

            await _jsRuntime.InvokeVoidAsync("monacoInterop.initialize", loaderUrl);
            _isInitialized = true;
        }
        catch (JSException ex)
        {
            Console.Error.WriteLine($"Monaco Editor initialization failed: {ex.Message}");
        }
        finally
        {
            _isInitializing = false;
        }
    }
    public async Task CreateMonacoEditor(string elementId, string initialCode, EditorOptions options,
        DotNetObjectReference<MonacoEditor> dotnetRef)
    {
        try
        {
            var editorOptions = options.ToJson();
            await _jsRuntime.InvokeVoidAsync("monacoInterop.createEditor", elementId, initialCode,
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
        await _jsRuntime.InvokeVoidAsync("monacoInterop.updateEditorConfiguration", elementId, editorOptions.ToJson());
    }
}