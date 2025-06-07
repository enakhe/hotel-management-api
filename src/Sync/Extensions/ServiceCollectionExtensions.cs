using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Sync.Hybrid;

namespace Sync.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHybridSync(this IServiceCollection services, string localConn, string remoteUrl)
    {
        services.AddSingleton<ISyncService>(_ => new SyncService(localConn, remoteUrl));
        return services;
    }
}
