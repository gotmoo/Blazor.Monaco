using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazor.Monaco;

/// <summary>
/// A Blazor component for integrating the Monaco editor.
/// </summary>
public partial class MonacoEditor : ComponentBase
{
    private DotNetObjectReference<MonacoEditor> _dotNetHelper;
    private readonly InteropService _service;
    public MonacoEditor(InteropService service)
    {
        _service = service;
        _dotNetHelper = DotNetObjectReference.Create(this);
    }
    
    /// <summary>
    /// The ID of the HTML element to host the editor.  A GUID is generated if not provided.
    /// </summary>
    [Parameter] public string ElementId { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// The initial code to display in the editor.
    /// </summary>
    [Parameter] public string? ScriptContent { get; set; }
    
    /// <summary>
    /// The language of the editor (e.g., JavaScript, PowerShell). This overrides the language set in <see cref="EditorOptions"/>.
    /// </summary>
    [Parameter] public Language? Language { get; set; }
    
    /// <summary>
    /// The editor options. See <see cref="EditorOptions"/> for available settings.
    /// </summary>
    [Parameter] public EditorOptions EditorOptions { get; set; } = new();
    
    /// <summary>
    /// CSS styles to apply to the editor container.
    /// </summary>
    [Parameter] public string? Style { get; set; } 
    
    /// <summary>
    /// CSS classes to apply to the editor container.
    /// </summary>
    [Parameter] public string? Class { get; set; }
    
    /// <summary>
    /// Callback invoked when the editor content changes.  The argument indicates whether the content has changed from the initial value.
    /// </summary>
    [Parameter] public EventCallback<bool> OnContentChanged { get; set; }
    
    /// <summary>
    /// Callback invoked when a save request (e.g., Ctrl+S) is triggered in the editor.
    /// </summary>
    [Parameter] public EventCallback OnSaveRequested { get; set; }
    
    private bool ContentHasChanged { get;  set; }

    private string _initialCode = string.Empty;
    private string? FallbackStyle => Style is null && Class is null ? "min-height: 10em;" : Style;
    protected override void OnInitialized()
    {
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_service.LoaderRegistered)
        {
            await _service.EnsureScriptsLoadedAsync();
        }

        if (firstRender)
        {
            EditorOptions.Language = Language ?? EditorOptions.Language;
            _initialCode = !string.IsNullOrWhiteSpace(ScriptContent)
                ? ScriptContent
                : MonacoDefaultScript.Get[EditorOptions.Language];
            await _service.InitializeMonacoEditor(ElementId, _initialCode, EditorOptions, _dotNetHelper);
        }
    }

    [JSInvokable]
    public async Task OnEditorContentChanged(bool contentChanged)
    {
        ContentHasChanged = contentChanged;
        await NotifyParentOfContentChange();
    }

    [JSInvokable]
    public async Task OnEditorSaveRequest()
    {
        if (OnSaveRequested.HasDelegate)
        {
            await OnSaveRequested.InvokeAsync();
        }
    }

    private async Task NotifyParentOfContentChange()
    {
        if (OnContentChanged.HasDelegate)
        {
            await OnContentChanged.InvokeAsync(ContentHasChanged);
        }
    }

    /// <summary>
    /// Sets the content of the editor.
    /// </summary>
    /// <param name="newContent">The new text content to set in the editor.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SetEditorContent(string newContent)
    {
        await _service.SetEditorContent(ElementId, newContent);
        await ResetChangeTracker(newContent);
    }

    /// <summary>
    /// Gets the current content of the editor.
    /// </summary>
    /// <param name="resetChangedOnRead">If <c>true</c>, resets the change tracker after reading the content, so subsequent calls to <see cref="OnContentChanged"/> will only fire if the content is modified again. Defaults to <c>false</c>.</param>
    /// <returns>A <see cref="Task{string}"/> containing the editor's content.</returns>
    public async Task<string> GetEditorContent(bool resetChangedOnRead = false)
    {
        var newContent = await _service.GetEditorContent(ElementId, resetChangedOnRead);
        if (resetChangedOnRead)
        {
            await ResetChangeTracker(newContent);
        }

        return newContent;
    }

    private async Task ResetChangeTracker(string newContent)
    {
        ContentHasChanged = false;
        _initialCode = newContent;
        ScriptContent = newContent;
        await NotifyParentOfContentChange();
    }

    /// <summary>
    /// Updates the editor's configuration with the provided options.
    /// </summary>
    /// <param name="newConfig">The new editor options to apply.  See <see cref="EditorOptions"/> for available settings.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task UpdateEditorConfiguration(EditorOptions newConfig)
    {
        await _service.UpdateEditorConfiguration(ElementId, newConfig);
    }

    /// <summary>
    /// Re-initializes the Monaco editor. This can be useful for applying configuration changes that require a full re-initialization.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task ReInitializeEditor()
    {
        _initialCode = await GetEditorContent();
        await _service.InitializeMonacoEditor(ElementId, _initialCode, EditorOptions, _dotNetHelper);
    }
}