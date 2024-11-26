using System.Text.Json.Serialization;

namespace Blazor.Monaco;

public class MinimapOptions
{
    public bool Autohide { get; set; } = false;
    public bool Enabled { get; set; } = true;
    public int MaxColumn { get; set; } = 120;
    public bool RenderCharacters { get; set; } = true;
    public int Scale { get; set; } = 1;
    public int SectionHeaderFontSize { get; set; } = 9;
    public int SectionHeaderLetterSpacing { get; set; } = 1;
    public bool ShowMarkSectionHeaders { get; set; } = true;
    public bool ShowRegionSectionHeaders { get; set; } = true;
    [JsonConverter(typeof(EnumDescriptionConverter))]
    public MinimapShowSlider ShowSlider { get; set; } = MinimapShowSlider.MouseOver;
    [JsonConverter(typeof(EnumDescriptionConverter))]
    public MinimapSide Side { get; set; } = MinimapSide.Right;
    [JsonConverter(typeof(EnumDescriptionConverter))]
    public MinimapSize Size { get; set; } = MinimapSize.Actual;
    
}