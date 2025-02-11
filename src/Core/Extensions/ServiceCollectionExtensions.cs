using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor.Monaco;

public static class BlazorMonacoServiceCollectionExtensions
{
    /// <summary>
    /// Configures and adds Blazor Monaco components to the service collection. Provides options for
    /// customization of the library configuration.
    /// </summary>
    /// <param name="services">The service collection to which the Blazor Monaco components will be added.</param>
    /// <param name="configuration">An optional configuration to customize the library setup. If not provided, default configuration is used.</param>
    /// <returns>The modified <see cref="IServiceCollection"/> with Blazor Monaco components registered.</returns>
    public static IServiceCollection AddBlazorMonacoComponents(this IServiceCollection services,
        LibraryConfiguration? configuration = null)
    {
        var options = configuration ?? new();
        services.AddSingleton(options);

        var isWasm = services.Any(service => service.ServiceType == typeof(IWebAssemblyHostEnvironment));

        if (isWasm)
        {
            services.AddSingleton<GlobalState>();
            services.AddSingleton<InteropService>();
            return services;
        }

        services.AddScoped<GlobalState>();
        services.AddScoped<InteropService>();
        return services;
    }

    /// <summary>
    /// Configures and adds Blazor Monaco components to the service collection, with the option to customize the library configuration.
    /// </summary>
    /// <param name="services">The service collection to which the Blazor Monaco components will be added.</param>
    /// <param name="configuration">An action used to configure the library's setup. This allows customization of options such as the Monaco Loader URL.</param>
    /// <returns>The modified <see cref="IServiceCollection"/> with Blazor Monaco components registered.</returns>
    public static IServiceCollection AddBlazorMonacoComponents(this IServiceCollection services,
        Action<LibraryConfiguration> configuration)
    {
        LibraryConfiguration options = new();
        configuration.Invoke(options);

        return AddBlazorMonacoComponents(services, options);
    }
}