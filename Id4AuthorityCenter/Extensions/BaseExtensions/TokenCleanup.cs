using AQHG.WL.Authorization.Web.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AQHG.WL.Authorization.Web.Extensions
{
    public class TokenCleanupOptions
    {
        public int Interval { get; set; } = 60;
    }

    public class CustomTokenCleanup
    {
//        protected Logger Log => SelfLogManage.GetLog(typeof(CustomTokenCleanup));
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _interval;
        private CancellationTokenSource _source;

        public CustomTokenCleanup(IServiceProvider serviceProvider, TokenCleanupOptions options)
        {
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            if (options == null) throw new ArgumentNullException(nameof(options));
            if (options.Interval < 1) throw new ArgumentException("interval must be more than 1 second");

            _serviceProvider = serviceProvider;
            _interval = TimeSpan.FromSeconds(options.Interval);
        }

        public void Start()
        {
            if (_source != null) throw new InvalidOperationException("Already started. Call Stop first.");

//            Log.Main.Debug("Starting token cleanup");

            _source = new CancellationTokenSource();
            Task.Factory.StartNew(() => Start(_source.Token));
        }

        public void Stop()
        {
            if (_source == null) throw new InvalidOperationException("Not started. Call Start first.");

//            Log.Main.Debug("Stopping token cleanup");

            _source.Cancel();
            _source = null;
        }

        private async Task Start(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
//                    Log.Main.Debug("CancellationRequested");
                    break;
                }

                try
                {
                    await Task.Delay(_interval, cancellationToken);
                }
                catch
                {
//                    Log.Main.Debug("Task.Delay exception. exiting.");
                    break;
                }

                if (cancellationToken.IsCancellationRequested)
                {
//                    Log.Main.Debug("CancellationRequested");
                    break;
                }

                await ClearTokens();
            }
        }

        private async Task ClearTokens()
        {
            try
            {
//                Log.Main.Debug("Querying for tokens to clear");

                using (var serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    {
                        //clear tokenlist from redis
                        //clear tokeninfo from sqlserver
                    }
                }
            }
            catch (Exception ex)
            {
//                Log.Main.Error("Exception cleaning tokens {exception}", ex);
            }
        }
    }
}
namespace Microsoft.Extensions.DependencyInjection
{
    public static class IdentityServerMongoDbBuilderExtensions
    {
        public static IApplicationBuilder UseIdentityServerCustomTokenCleanup(this IApplicationBuilder app, IApplicationLifetime applicationLifetime)
        {
            var tokenCleanup = app.ApplicationServices.GetService<CustomTokenCleanup>();
            if (tokenCleanup == null)
            {
                throw new InvalidOperationException("AddOperationalStore must be called on the service collection.");
            }
            applicationLifetime.ApplicationStarted.Register(tokenCleanup.Start);
            applicationLifetime.ApplicationStopping.Register(tokenCleanup.Stop);

            return app;
        }
    }
}