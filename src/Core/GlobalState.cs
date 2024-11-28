using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazor.Monaco;

/// <summary>
/// This class is used to store the global values of the Blazor.Monaco components.
/// </summary>
// TODO: #vNext: Rename this class to 'GlobalDesign' in the next major version.
public class GlobalState
{
    public ElementReference Container { get; set; } = default!;
    public StandardLuminance Luminance { get; set; } = StandardLuminance.LightMode;
    public bool InteropInDom { get; set; }
    public bool LoaderInDom { get; set; }
    public IJSObjectReference JsObjectReference { get; set; } = default!;
    public DateTime? FirstLoadTime { get; set; }
    public string? Color { get; set; }

    public event Action? OnChange;

    public void SetLuminance(StandardLuminance luminance)
    {
        Luminance = luminance;
        NotifyStateHasChanged();
    }

    public void SetContainer(ElementReference container)
    {
        Container = container;
        NotifyStateHasChanged();
    }

    public void SetColor(string? color)
    {
        Color = color;
        NotifyStateHasChanged();
    }

    private void NotifyStateHasChanged()
    {
        OnChange?.Invoke();
    }
}