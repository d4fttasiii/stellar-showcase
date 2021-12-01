using Microsoft.Extensions.DependencyInjection;
using StellarShowcase.Repositories.Implementations;
using StellarShowcase.Repositories.Interfaces;

namespace StellarShowcase.Repositories
{
    public static class Registar
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IIssuerRepository, IssuerRepository>();
            services.AddScoped<IUserAccountRepository, UserAccountRepository>();
            services.AddScoped<IExchangeRepository, ExchangeRepository>();

            return services;
        }
    }
}
