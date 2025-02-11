using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace Blazor.Monaco;

public class InteropService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly GlobalState _globalState;
    private readonly LibraryConfiguration _libraryConfiguration;
    private readonly ILogger<InteropService> _logger;
    private bool _isInitialized;
    private bool _isInitializing;

    public InteropService(IJSRuntime jsRuntime, GlobalState globalState, LibraryConfiguration libraryConfiguration,
        ILogger<InteropService> logger)
    {
        _jsRuntime = jsRuntime;
        _globalState = globalState;
        _libraryConfiguration = libraryConfiguration;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        var loaderUrl = _libraryConfiguration.MonacoLoaderUrl;
        if (_isInitialized)
        {
            _logger.LogDebug
                ("MonacoInit: Monaco Editor already initialized. Skipping initialization");
            return;
        }

        if (_isInitializing)
        {
            _logger.LogDebug
                ("MonacoInit: Monaco Editor initialization in progress. Waiting for completion...");
            while (_isInitializing)
            {
                await Task.Delay(100);
            }

            if (_isInitialized)
            {
                _logger.LogDebug
                    ("MonacoInit: Monaco Editor successfully initialized by another caller");
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
            _logger.LogError("Monaco Editor initialization failed: {Message}",ex.Message);
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
            _logger.LogDebug
                ("CreateEditor: Created for '{Element}' with options {Options}", elementId, editorOptions);
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
        _logger.LogDebug
            ("SetEditorContent: Set for '{Element}'", elementId);
    }

    public async Task<string> GetEditorContent(string elementId, bool resetChangedOnRead = false)
    {
        var content = await _jsRuntime.InvokeAsync<string>("monacoInterop.getEditorContent", elementId, resetChangedOnRead);
        _logger.LogDebug
            ("GetEditorContent: Get for '{Element}', resetChangedOnRead: {Reset}", elementId, resetChangedOnRead);
        return content;
    }

    public async Task UpdateEditorConfiguration(string elementId, EditorOptions editorOptions)
    {
        await _jsRuntime.InvokeVoidAsync("monacoInterop.updateEditorConfiguration", elementId, editorOptions.ToJson());
        _logger.LogDebug
            ("UpdateEditorConfiguration: Updated `{Element}` with options {Options}", elementId, editorOptions.ToJson());
    }
}