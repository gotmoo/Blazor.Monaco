using System.ComponentModel;

namespace Blazor.Monaco;

public enum WrappingIndent
{
    [Description("none")]
    None,
    [Description("same")]
    Same,
    [Description("indent")]
    Indent,
    [Description("deepIndent")]
    DeepIndent
}