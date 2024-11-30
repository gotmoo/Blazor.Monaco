
using Microsoft.Extensions.DependencyInjection;

//Namespace is deliberately the root namespace.
namespace Blazor.Monaco;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add common services required by the Monaco Editor for Blazor library
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Library configuration</param>
    public static IServiceCollection AddBlazorMonacoComponents(this IServiceCollection services,
        LibraryConfiguration? configuration = null)
    {
        services.AddScoped<GlobalState>();

        var options = configuration ?? new();

        
        services.AddScoped<InteropService>();
        services.AddSingleton(options);

        return services;
    }

    /// <summary>
    /// Add common services required by the Monaco Editor for Blazor library
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Library configuration</param>
    public static IServiceCollection AddBlazorMonacoComponents(this IServiceCollection services,
        Action<LibraryConfiguration> configuration)
    {
        LibraryConfiguration options = new();
        configuration.Invoke(options);

        return AddBlazorMonacoComponents(services, options);
    }
}