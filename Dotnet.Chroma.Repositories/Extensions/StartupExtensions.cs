using Dotnet.Chroma.Repositories.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.Connectors.Chroma;

namespace Dotnet.Chroma.Repositories.Extensions
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddChromaConfiguration(this IServiceCollection services, IConfiguration configuration)
            => services.Configure<ChromaSettings>(configuration.GetSection(nameof(ChromaSettings)));


#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        /// <summary>
        /// Helper method to register the SK IChromaClient configuring it from the app ChromaSettings
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <param name="lifetime"></param>
        /// <returns></returns>
        public static IServiceCollection AddChromaClient(this IServiceCollection services, IConfiguration configuration, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            => lifetime switch
            {
                ServiceLifetime.Transient => services.AddTransient<IChromaClient, ChromaClient>(sp =>
                {
                    var settings = sp.GetRequiredService<IOptions<ChromaSettings>>().Value;
                    return new ChromaClient(settings.ServerUrl);
                }),
                ServiceLifetime.Scoped => services.AddScoped<IChromaClient, ChromaClient>(sp =>
                {
                    var settings = sp.GetRequiredService<IOptions<ChromaSettings>>().Value;
                    return new ChromaClient(settings.ServerUrl);
                }),
                _ => services
            };
    }
}
