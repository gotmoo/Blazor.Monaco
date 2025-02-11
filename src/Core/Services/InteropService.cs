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

    /// Initializes the shared Monaco Editor environment by loading necessary scripts and configurations.
    /// This method prevents redundant initialization by tracking the initialization state.
    /// <returns>A Task representing the asynchronous operation of initializing the Monaco Editor environment.</returns>
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

    /// Creates a Monaco Editor instance within the specified HTML element.
    /// This method initializes a Monaco Editor with the given element identifier, initial code, editor options, and
    /// a .NET object reference for interaction.
    /// <param name="elementId">The ID of the HTML element where the Monaco Editor will be created.</param>
    /// <param name="initialCode">The initial content to populate within the editor instance.</param>
    /// <param name="options">The configuration options for the editor, such as language, theme, and other settings.</param>
    /// <param name="dotnetRef">A .NET object reference to facilitate communication between .NET and JavaScript.</param>
    /// <returns>A Task representing the asynchronous operation of creating a Monaco Editor instance.</returns>
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

    /// Sets the content of the specified Monaco Editor instance to the provided value.
    /// This method interacts with the underlying JavaScript API to update the editor's content.
    /// <param name="elementId">The unique identifier of the Monaco Editor instance to modify.</param>
    /// <param name="newContent">The new content to set in the editor.</param>
    /// <returns>A Task that represents the asynchronous operation of setting the editor's content.</returns>
    public async Task SetEditorContent(string elementId, string newContent)
    {
        await _jsRuntime.InvokeVoidAsync("monacoInterop.setEditorContent", elementId, newContent);
        _logger.LogDebug
            ("SetEditorContent: Set for '{Element}'", elementId);
    }

    /// Retrieves the current content of the specified Monaco Editor instance by its element ID.
    /// This method can optionally reset the "changed" state tracker after reading the content.
    /// <param name="elementId">The unique identifier of the Monaco Editor instance from which to retrieve content.</param>
    /// <param name="resetChangedOnRead">A Boolean flag indicating whether the "changed" state tracker should be reset after retrieving content. Defaults to false.</param>
    /// <returns>A Task representing the asynchronous operation, with a string result containing the current editor content.</returns>
    public async Task<string> GetEditorContent(string elementId, bool resetChangedOnRead = false)
    {
        var content = await _jsRuntime.InvokeAsync<string>("monacoInterop.getEditorContent", elementId, resetChangedOnRead);
        _logger.LogDebug
            ("GetEditorContent: Get for '{Element}', resetChangedOnRead: {Reset}", elementId, resetChangedOnRead);
        return content;
    }

    /// Updates the configuration of a Monaco Editor instance by applying the specified editor options.
    /// This method invokes a JavaScript function to update the editor configuration and logs the operation.
    /// <param name="elementId">The unique identifier of the Monaco Editor instance to be updated.</param>
    /// <param name="editorOptions">The configuration options to be applied to the editor instance.</param>
    /// <returns>A Task representing the asynchronous operation of updating the editor configuration.</returns>
    public async Task UpdateEditorConfiguration(string elementId, EditorOptions editorOptions)
    {
        await _jsRuntime.InvokeVoidAsync("monacoInterop.updateEditorConfiguration", elementId, editorOptions.ToJson());
        _logger.LogDebug
            ("UpdateEditorConfiguration: Updated `{Element}` with options {Options}", elementId, editorOptions.ToJson());
    }
}