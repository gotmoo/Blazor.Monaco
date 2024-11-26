using System.ComponentModel;

namespace Blazor.Monaco;

public enum AutoClosingBrackets
{
    [Description("always")]
    Always,
    [Description("languageDefined")]
    LanguageDefined,
    [Description("beforeWitespace")]
    BeforeWhitespace,
    [Description("never")]
    Never
}