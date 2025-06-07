using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotmim.Sync;
using Dotmim.Sync.SqlServer;
using Dotmim.Sync.Web.Client;

namespace Sync.Hybrid;

public interface ISyncService
{
    Task<SyncResult> SynchronizeAsync();
}

public class SyncService : ISyncService
{
    private readonly SqlSyncProvider _localProvider;
    private readonly WebRemoteOrchestrator _remoteProvider;
    private readonly SyncAgent _agent;

    public SyncService(string localConnectionString, string remoteUrl)
    {
        _localProvider = new SqlSyncProvider(localConnectionString);
        _remoteProvider = new WebRemoteOrchestrator(remoteUrl)
        {
            _remoteProvider.SerializerFactory = SerializationFormat.Json
        };

        _agent = new SyncAgent(_localProvider, _remoteProvider);
    }

    public async Task<SyncResult> SynchronizeAsync()
    {
        try
        {
            var result = await _agent.SynchronizeAsync();

            Console.WriteLine($"Downloaded: " +
                $"{result.TotalChangesDownloadedFromServer}, " +
                $"Uploaded: {result.TotalChangesUploadedToServer}"
            );

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sync error: {ex.Message}");
            throw;
        }
    }
}
