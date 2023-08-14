using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Configuration.AddJsonFile("appsettings.Development.json");
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () => 
{
    try
    {
        await DbInitializer.InitDB(app);
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
    }
});
   
app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy() 
 => HttpPolicyExtensions
    .HandleTransientHttpError() 
    .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));