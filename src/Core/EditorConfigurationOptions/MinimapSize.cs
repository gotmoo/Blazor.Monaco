using System.ComponentModel;

namespace Blazor.Monaco.EditorConfigurationOptions;

public enum MinimapSize
{
    [Description("actual")]
    Actual,
    [Description("proportional")]
    Proportional,
    [Description("fill")]
    Fill,
    [Description("fit")]
    Fit
}