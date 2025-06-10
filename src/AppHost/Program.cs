var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithRedisInsight()
    .WithRedisCommander();

var sql = builder.AddSqlServer("sql");

var database = sql.AddDatabase("HotelManagementDb");

builder.AddProject<Projects.Web>("web")
    .WithReference(database)
    .WithReference(cache)
    .WaitFor(database);

builder.Build().Run();
