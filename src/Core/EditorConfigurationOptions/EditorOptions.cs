using System.Text.Json.Serialization;

namespace Blazor.Monaco;

public class EditorOptions
{
    [JsonConverter(typeof(EnumDescriptionConverter))]
    public Language Language { get; set; } = Language.PowerShell;
    [JsonConverter(typeof(EnumDescriptionConverter))]
    public LineNumbers LineNumbers { get; set; } = LineNumbers.On;
    public int LineNumbersMinChars { get; set; } = 5;
    public bool RoundedSelection { get; set; } = true;
    public bool ScrollBeyondLastLine { get; set; } = true;
    public int ScrollBeyondLastColumn { get; set; } = 5;
    [JsonConverter(typeof(EnumDescriptionConverter))]
    public WordWrap WordWrap { get; set; } = WordWrap.Off; 
    [JsonConverter(typeof(EnumDescriptionConverter))]
    public WrappingIndent WrappingIndent { get; set; } = WrappingIndent.None; 
    public bool ReadOnly { get; set; } = false;
    [JsonConverter(typeof(EnumDescriptionConverter))]
    public Theme Theme { get; set; } = Theme.VsDark;
    [JsonConverter(typeof(EnumDescriptionConverter))]
    public AutoClosingBrackets AutoClosingBrackets { get; set; } = AutoClosingBrackets.LanguageDefined;
    public bool MatchBrackets { get; set; } = true;
    public MinimapOptions Minimap { get; set; } = new MinimapOptions();
    internal string ToJson(bool prettyPrint = false)
    {
        return this.ToJsonWithEnumDescription(prettyPrint);
    }
}