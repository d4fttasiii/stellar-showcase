using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarShowcase.DataAccess.Blockchain;
using StellarShowcase.DataAccess.Database;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("StellarShowcase.Repositories")]
namespace StellarShowcase.DataAccess
{
    public static class Registar
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services)
        {
            services.AddScoped<IStellarClient, StellarClient>();
            services.AddDatabase();

            return services;
        }

        private static void AddDatabase(this IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("ShowcaseDb"));
            services.AddScoped<IAppDbContext, AppDbContext>();
        }
    }
}
