using System.ComponentModel;

namespace Blazor.Monaco;

public enum Theme
{
    [Description("vs")]
    Vs,
    [Description("vs-dark")]
    VsDark,
    [Description("hc-black")]
    HcBlack,
    [Description("hc-light")]
    HcLight
}