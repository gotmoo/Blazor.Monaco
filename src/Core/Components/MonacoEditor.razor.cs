﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazor.Monaco;

public partial class MonacoEditor : ComponentBase
{
    private readonly InteropService _service;

    public MonacoEditor(InteropService service)
    {
        _service = service;
    }
    [Parameter] public required string ElementId { get; set; } 
    [Parameter] public string? ScriptContent { get; set; }
    [Parameter] public MonacoLanguage Language { get; set; } = MonacoLanguage.PowerShell;
    [Parameter] public EventCallback<bool> ContentChanged { get; set; }
    public bool ContentHasChanged { get; private set; }

    private string InitialCode => ScriptContent ?? MonacoDefaultScript.Get[Language];
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {

        if (!_service.LoaderRegistered)
        {
            await _service.EnsureScriptsLoadedAsync();
        }
        if (firstRender)
        {
            var dotNetRef = DotNetObjectReference.Create(this);
            await _service.InitializeMonacoEditor(ElementId, InitialCode, Language, dotNetRef);
        }
    }
    [JSInvokable]
    public async Task OnEditorContentChanged(bool contentChanged)
    {
        Console.WriteLine(
            $"OnEditorContentChanged: {contentChanged}");
        ContentHasChanged = contentChanged;
        await NotifyParentOfContentChange();
    }
    private async Task NotifyParentOfContentChange()
    {
        Console.WriteLine($"NotifyParentOfContentChange: {ContentHasChanged}");
        await ContentChanged.InvokeAsync(ContentHasChanged);
    }

}