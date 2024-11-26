using System.ComponentModel;

namespace Blazor.Monaco;

public enum Language
{
    [Description("plaintext")]
    PlainText,
    [Description("csharp")]
    CSharp,
    [Description("javascript")]
    JavaScript,
    [Description("powershell")]
    PowerShell,
    [Description("typescript")]
    TypeScript,
    [Description("html")]
    Html,
    [Description("css")]
    Css,
    [Description("json")]
    Json,
    [Description("xml")]
    Xml,
    
}