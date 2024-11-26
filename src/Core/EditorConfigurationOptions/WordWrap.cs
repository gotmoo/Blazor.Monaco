using System.ComponentModel;

namespace Blazor.Monaco;

public enum WordWrap
{
    [Description("off")]
    Off,
    [Description("on")]
    On,
    [Description("wordWrapColumn")]
    WordWrapColumn,
    [Description("bounded")]
    Bounded
}