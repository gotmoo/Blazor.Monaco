using System.ComponentModel;

namespace Blazor.Monaco.EditorConfigurationOptions;

public enum MinimapShowSlider
{
    [Description("always")]
    Always,
    [Description("mouseover")]
    MouseOver
}