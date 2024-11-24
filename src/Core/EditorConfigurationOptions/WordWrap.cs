using System.ComponentModel;

namespace Blazor.Monaco.EditorConfigurationOptions;

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