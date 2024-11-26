using System.ComponentModel;

namespace Blazor.Monaco;

public enum LineNumbers
{
    [Description("off")]
    Off,
    [Description("on")]
    On,
    [Description("relative")]
    Relative,
    [Description("interval")]
    Interval
}