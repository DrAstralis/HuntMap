using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApiClient
    {
        public static IServiceCollection AddApiClient(this IServiceCollection services)
        {
            services.AddHttpClient("api", (sp, client) =>
            {
                var nav = sp.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
                client.BaseAddress = new System.Uri(nav.BaseUri);
            });
            return services;
        }
    }
}