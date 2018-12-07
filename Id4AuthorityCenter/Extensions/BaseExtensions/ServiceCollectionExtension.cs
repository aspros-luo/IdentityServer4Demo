using System.IO;
using System.Security.Cryptography.X509Certificates;
using Id4AuthorityCenter.Config;
using IdentityServer4.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Id4AuthorityCenter.Extensions.BaseExtensions
{
    public static class ServiceCollectionExtension
    {
        public static AppSettings GetConfig(this IServiceCollection services, IConfiguration configuration)
        {
            //Config
            services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetService<IOptions<AppSettings>>().Value;
        }
        //DI注入
        public static void Inject(this IServiceCollection services, IConfiguration configuration, AppSettings appsettings)
        {
            
            //Core self Log
            //            services.AddTransient<ILoggerFactory, LoggerFactory>();

            //            //Self Validator
            //            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();
            //            services.AddTransient<IExtensionGrantValidator, ExternalGrantValidator>();
            //            var databasestr = Encrypt.DecryptStr(appsettings.CSEEV2Str, appsettings.CSEEV2Key, appsettings.CSEEV2Iv);
            //            //Db
            //            services.AddScoped<IDatabase, Database>(db => new Database(databasestr, "System.Data.SqlClient"));
            //            services.AddSingleton(new MongoDBHelper(appsettings.MongodbStr));
            //            //idwork
            //            //从配置中获取
            //            var workId = appsettings.WorkId;
            //            services.AddSingleton(new IdWorker(workId));
            //            //Cache
            //            services.AddSingleton<ICacheProvider, MemCacheProvider>();
            //            services.AddEnyimMemcached(configuration.GetSection("AppSettings:EnyimMemcachedSettings"));
            //            services.AddSingleton(new RedisHelper(appsettings.RedisSettings.DbNum, appsettings.RedisSettings.ReadWriteHosts));
        }

        public static void IdentityServer(this IServiceCollection services, IConfiguration configuration, AppSettings appsettings)
        {
            Configs.AccessTokenLifetime = appsettings.AccessTokenLifetime;
            Configs.SlidingRefreshTokenLifetime = appsettings.SlidingRefreshTokenLifetime;
            // configure identity server with in-memory stores, keys, clients and scopes
            var pfxFilePath = Path.Combine(Directory.GetCurrentDirectory(), appsettings.CertsName);//idsrv4.pfx
            if (!File.Exists(pfxFilePath)) throw new FileNotFoundException("Signing Certificate is missing!");
            var cert = new X509Certificate2(pfxFilePath, appsettings.CertsPwd);
            
            var builder = services.AddIdentityServer(x => x.IssuerUri = configuration["AppSettings:IssuerUri"])
                .AddConfigurationStore(configuration.GetSection("AppSettings:MongoDB"))
                .AddOperationalStore(configuration.GetSection("AppSettings:MongoDB"));//Configuration.GetSection("AppSettings:MongoDB")

            builder.AddSigningCredential(cert)
                .AddInMemoryIdentityResources(Configs.GetIdentityResources())
                .AddInMemoryApiResources(Configs.GetApiResources())
                .AddInMemoryClients(Configs.GetClients());
            builder.Services.RemoveAll<ITokenService>();
            builder.Services.TryAddTransient<ITokenService, CustomTokenService>();
            builder.Services.AddTransient<IProfileService, ProfileService>();
//            builder.AddResourceOwnerValidator<ResourceOwnerPasswordValidator>();
//            builder.AddExtensionGrantValidator<AnonymousGrantValidator>();
//            builder.AddExtensionGrantValidator<ExternalGrantValidator>();
//            builder.AddExtensionGrantValidator<VerificationGrantValidator>();
//            builder.AddExtensionGrantValidator<VerifycodeGrantValidator>();
//            builder.AddExtensionGrantValidator<WechatGrantValidator>();

            builder.Services.AddTransient<ICustomClientSecretValidator, CustomClientSecretValidator>();
            builder.Services.AddTransient<ICustomSecretParser, CustomPostBodySecretParser>();
            builder.Services.AddTransient<CustomSecretParser>();
            builder.Services.AddTransient<ILocalTokenEndpointHandler, LocalTokenEndpoint>();

            //            if (appsettings.DefaultConnectionLimit > 0)
            //            {
            //                Common.Utility.HttpSender.SetDefaultConnectionLimit(appsettings.DefaultConnectionLimit);
            //            }
        }
    }
}
