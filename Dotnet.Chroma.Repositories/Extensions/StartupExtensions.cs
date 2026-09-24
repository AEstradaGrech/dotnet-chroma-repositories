using Dotnet.Chroma.Repositories.Interfaces;
using Dotnet.Chroma.Repositories.Models.Interfaces;
using Dotnet.Chroma.Repositories.Models.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;

namespace Dotnet.Chroma.Repositories.Extensions
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddChromaConfiguration(this IServiceCollection services, IConfiguration configuration)
            => services.Configure<ChromaSettings>(configuration.GetSection(nameof(ChromaSettings)));

        public static IServiceCollection AddDefaultChromaRepository(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            => lifetime switch {
                ServiceLifetime.Scoped => services.AddScoped<IChromaChunksRepository, ChromaChunksRepository>(),
                ServiceLifetime.Transient => services.AddTransient<IChromaChunksRepository, ChromaChunksRepository>(),
                ServiceLifetime.Singleton => services.AddSingleton<IChromaChunksRepository, ChromaChunksRepository>(),
                _ => services.AddScoped<IChromaChunksRepository, ChromaChunksRepository>()
            };

        public static IServiceCollection AddChromaDbClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<IChromaDbClient, ChromaDbClient>(client =>
            {
                var cfg = configuration.GetSection(nameof(ChromaSettings)).Get<ChromaSettings>();
                client.BaseAddress = new Uri(cfg.ServerUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            });

            return services;
        }
    }
}
