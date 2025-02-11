namespace Blazor.Monaco;

/// <summary>
/// This class is used to store the global values of the Blazor.Monaco components.
/// </summary>
// TODO: #vNext: Rename this class to 'GlobalDesign' in the next major version.
public class GlobalState
{
    public bool InteropInDom { get; set; }
    public bool LoaderInDom { get; set; }
    public DateTime? FirstLoadTime { get; set; }
}