using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StellarShowcase.DataAccess.Blockchain;
using StellarShowcase.DataAccess.Database;
using StellarShowcase.DataAccess.Security;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("StellarShowcase.Repositories")]
namespace StellarShowcase.DataAccess
{
    public static class Registar
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services)
        {
            services.AddScoped<IStellarClient, StellarClient>();
            services.AddScoped<IEncryptor, Encryptor>();
            services.AddDatabase();

            return services;
        }

        private static void AddDatabase(this IServiceCollection services)
        {
            services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Filename=C:/temp/db/showcase.db"));
            services.AddScoped<IAppDbContext, AppDbContext>();
        }
    }
}
