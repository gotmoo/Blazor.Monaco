using Microsoft.AspNetCore.Components;

namespace Blazor.Monaco;

public partial class MonacoEditor : ComponentBase
{
    private readonly InteropService _service;

    public MonacoEditor(InteropService service)
    {
        _service = service;
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        Console.WriteLine($"{DateTime.UtcNow} InteropRegistered: {_service.InteropRegistered} LoaderRegistered: {_service.LoaderRegistered} FirstLoaded: {_service.FirstLoadTime}");

        if (!_service.LoaderRegistered)
        {
            await _service.EnsureScriptsLoadedAsync();
        }
        if (firstRender)
        {
        }
        Console.WriteLine($"{DateTime.UtcNow} InteropRegistered: {_service.InteropRegistered} LoaderRegistered: {_service.LoaderRegistered} FirstLoaded: {_service.FirstLoadTime}");
    }
}