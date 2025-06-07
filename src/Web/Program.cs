using HotelManagement.Application;
using HotelManagement.Infrastructure;
using HotelManagement.Infrastructure.Data;
using HotelManagement.ServiceDefaults;
using HotelManagement.Web;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();
builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.InitialiseDatabaseAsync();
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseExceptionHandler("/error");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel API V1");
    options.RoutePrefix = "api"; // Swagger UI at /api
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Map("/", () => Results.Redirect("/api"));

//app.MapDefaultEndpoints();
//app.MapEndpoints();

app.Run();

public partial class Program { }
