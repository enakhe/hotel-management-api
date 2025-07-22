var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithRedisInsight()
    .WithRedisCommander();

var sqlPassword = builder.AddParameter("sql-password", secret: true);

var sql = builder.AddSqlServer("sql", password: sqlPassword)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

var database = sql.AddDatabase("HotelManagementDb");

builder.AddProject<Projects.Web>("web")
    .WithReference(database)
    .WithReference(cache)
    .WaitFor(database);

builder.Build().Run();
