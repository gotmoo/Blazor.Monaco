using Microsoft.AspNetCore.Components;

namespace Blazor.Monaco;

// If needed, additional services configuration objects can be added here

/// <summary>
/// Defines the global Fluent UI Blazor component library services configuration
/// </summary>
public class LibraryConfiguration
{
    /// <summary>
    /// Gets the assembly version formatted as a string.
    /// </summary>
    internal static readonly string? AssemblyVersion = typeof(LibraryConfiguration).Assembly.GetName().Version?.ToString();

    /// <summary>
    /// Gets or sets a the location of the Monaco Editor loader javascript file. The components will 
    /// use this URL to initialize the editor.
    /// </summary>
    public string MonacoLoaderUrl = "https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.52.0/min/vs/loader.js";

    /// <summary>
    /// Gets or sets the function that formats the URL of the collocated JavaScript file,
    /// adding the return value as a query string parameter.
    /// By default, the function adds a query string parameter with the version of the assembly: `v=[AssemblyVersion]`.
    /// </summary>
    public Func<string, string>? CollocatedJavaScriptQueryString { get; set; } = (url)
        => string.IsNullOrEmpty(AssemblyVersion) ? string.Empty : $"v={AssemblyVersion}";

    public LibraryConfiguration()
    {
    }


    internal static LibraryConfiguration ForUnitTests => new()
    {
        CollocatedJavaScriptQueryString = null,
    };
}
