var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
    .WithRedisInsight()
    .WithRedisCommander();


builder.AddProject<Projects.Web>("web")
    .WithReference(cache);

builder.Build().Run();
