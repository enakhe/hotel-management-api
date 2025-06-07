using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Dotmim.Sync;
using Dotmim.Sync.SqlServer;
using Dotmim.Sync.Web.Client;
using Dotmim.Sync.Web.Server;

namespace HotelManagement.Web.Controllers.Sync;


[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly WebServerAgent _agent;

    public SyncController(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("HotelManagementDb");
        var provider = new SqlSyncProvider(connectionString);

        var tables = new string[]
        {
                "Guests",
                "Reservations",
                "Rooms",
                "Payments",
                "Invoices",
                "Bookings",
                "Transactions",
                "RoomTypes"
        };

        var setup = new SyncSetup(tables);

        _agent = new WebServerAgent(provider, setup);
    }

    [HttpPost]
    [Route("sync")]
    public async Task Sync()
    {
        await _agent.HandleRequestAsync(HttpContext);
    }
}
