using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazor.Monaco;

public partial class MonacoEditor : ComponentBase
{
    private readonly InteropService _service;

    public MonacoEditor(InteropService service)
    {
        _service = service;
    }

    [Parameter] public string ElementId { get; set; }
    [Parameter] public string? ScriptContent { get; set; }
    [Parameter] public Language? Language { get; set; }
    [Parameter] public EventCallback<bool> ContentChanged { get; set; }
    [Parameter] public EditorOptions EditorOptions { get; set; } = new();
    public bool ContentHasChanged { get; private set; }

    private string InitialCode;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_service.LoaderRegistered)
        {
            await _service.EnsureScriptsLoadedAsync();
        }

        if (firstRender)
        {
            var dotNetRef = DotNetObjectReference.Create(this);
            EditorOptions.Language = Language ?? EditorOptions.Language;
            InitialCode = !string.IsNullOrWhiteSpace(ScriptContent)
                ? ScriptContent
                : MonacoDefaultScript.Get[EditorOptions.Language];
            await _service.InitializeMonacoEditor(ElementId, InitialCode, EditorOptions, dotNetRef);
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
        ContentHasChanged = false;
        InitialCode = newContent;
        ScriptContent = newContent;
        await NotifyParentOfContentChange();
    }

    public async Task<string> GetEditorContent(bool resetChangedOnRead = false)
    {
        var newContent = await _service.GetEditorContent(ElementId);
        if (resetChangedOnRead)
        {
            ContentHasChanged = false;
            InitialCode = newContent;
            ScriptContent = newContent;   
            await NotifyParentOfContentChange();
        }

        return newContent;
    }

    public async Task UpdateEditorConfiguration(EditorOptions newConfig)
    {
        await _service.UpdateEditorConfiguration(ElementId, newConfig);
    }
}