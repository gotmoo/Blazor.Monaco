using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor.Monaco;

public static class BlazorMonacoServiceCollectionExtensions
{
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

    public static IServiceCollection AddBlazorMonacoComponents(this IServiceCollection services,
        Action<LibraryConfiguration> configuration)
    {
        LibraryConfiguration options = new();
        configuration.Invoke(options);

        return AddBlazorMonacoComponents(services, options);
    }
}