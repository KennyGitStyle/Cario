using Contracts;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Configuration.AddJsonFile("appsettings.Development.json");
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddMassTransit(x => {

    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    x.AddConsumersFromNamespaceContaining<AuctionUpdatedConsumer>();
    x.AddConsumersFromNamespaceContaining<AuctionDeletedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));
    x.UsingRabbitMq((context, cfg) => 
    {
        cfg.ReceiveEndpoint("search-auction-created", e => 
        {
            e.UseMessageRetry(r => 
            {
               r.Interval(5, 5);
            });

            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });
        
        cfg.ConfigureEndpoints(context);
    });
});
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