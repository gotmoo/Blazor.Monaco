using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazor.Monaco;

public partial class MonacoEditor : ComponentBase
{
    private DotNetObjectReference<MonacoEditor>? _dotNetHelper = null;
    private readonly InteropService _service;

    public MonacoEditor(InteropService service)
    {
        _service = service;
    }

    [Parameter] public string ElementId { get; set; } = Guid.NewGuid().ToString();
    [Parameter] public string? ScriptContent { get; set; }
    [Parameter] public Language? Language { get; set; }
    [Parameter] public EventCallback<bool> ContentChanged { get; set; }
    [Parameter] public EditorOptions EditorOptions { get; set; } = new();
    public bool ContentHasChanged { get; private set; }

    private string _initialCode = string.Empty;

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
            _dotNetHelper = DotNetObjectReference.Create(this);

            var dotNetRef = DotNetObjectReference.Create(this);
            EditorOptions.Language = Language ?? EditorOptions.Language;
            _initialCode = !string.IsNullOrWhiteSpace(ScriptContent)
                ? ScriptContent
                : MonacoDefaultScript.Get[EditorOptions.Language];
            await _service.InitializeMonacoEditor(ElementId, _initialCode, EditorOptions, dotNetRef);
        }
    }

    [JSInvokable]
    public async Task OnEditorContentChanged(bool contentChanged)
    {
        // Console.WriteLine(
        //     $"OnEditorContentChanged: {contentChanged}");
        ContentHasChanged = contentChanged;
        await NotifyParentOfContentChange();
    }

    private async Task NotifyParentOfContentChange()
    {
        // Console.WriteLine($"NotifyParentOfContentChange: {ContentHasChanged}");
        await ContentChanged.InvokeAsync(ContentHasChanged);
    }

    public async Task SetEditorContent(string newContent)
    {
        await _service.SetEditorContent(ElementId, newContent);
        await ResetChangeTracker(newContent);
    }

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
    public async Task UpdateEditorConfiguration(EditorOptions newConfig)
    {
        await _service.UpdateEditorConfiguration(ElementId, newConfig);
    }
}