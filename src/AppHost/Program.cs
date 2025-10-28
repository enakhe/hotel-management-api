using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;

var builder = DistributedApplication.CreateBuilder(args);

// Configure SSL certificate validation bypass for development
if (builder.Environment.IsDevelopment())
{
    builder.Services.Configure<HttpClientFactoryOptions>(options =>
    {
        options.HttpMessageHandlerBuilderActions.Add(builder =>
        {
            builder.PrimaryHandler = new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
        });
    });
}

var cache = builder.AddRedis("cache")
    .WithRedisInsight()
    .WithRedisCommander();


builder.AddProject<Projects.Web>("web")
    .WithReference(cache);

builder.Build().Run();
