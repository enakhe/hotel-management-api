using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sync.Hybrid;
/// <summary>
/// Configuration class for sync setup
/// </summary>
public static class SyncConfiguration
{
    public static string[] TablesToSync =>
    [
        "Guests", "Reservations", "Payments", "Rooms", "Transactions"
    ];

    public static string ScopeName => "HotelScope";

    public static string RemoteSyncUrl => "https://yourcloudapi.com/api/sync"; // Update with actual cloud endpoint URL
}
