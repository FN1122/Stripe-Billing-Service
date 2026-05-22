using Microsoft.Extensions.DependencyInjection;
using NetCore.AutoRegisterDi;

namespace Core.Utils
{
    public static class DiRegistrationExtensions
    {
        public static void RegisterServiceLayerDi(this IServiceCollection services)
        {
            services.RegisterAssemblyPublicNonGenericClasses(typeof(DiRegistrationExtensions).Assembly)
                .Where(c => c.Name.EndsWith("Service") || c.Name.EndsWith("Gateway") || c.Name.EndsWith("Handler"))
                .AsPublicImplementedInterfaces(ServiceLifetime.Scoped);
        }

        public static void RegisterRepositoryLayerDi(this IServiceCollection services)
        {
            services.RegisterAssemblyPublicNonGenericClasses(typeof(DiRegistrationExtensions).Assembly)
                .Where(c => c.Name.EndsWith("Repository"))
                .AsPublicImplementedInterfaces(ServiceLifetime.Scoped);
        }
    }
}
