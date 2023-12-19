using AuctionService;
using AuctionService.Consumers;
using AuctionService.Data;
using AuctionService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionServiceDbContext>(opts => 
{
    opts.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnString"));
});
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMemoryCache();

builder.Services.AddMassTransit(x => 
{
    x.AddEntityFrameworkOutbox<AuctionServiceDbContext>(opts => 
    {
        opts.QueryDelay = TimeSpan.FromSeconds(10);

        opts.UsePostgres();
        opts.UseBusOutbox();
    });

    x.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

    x.UsingRabbitMq((context, cfg) => 
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts => 
    {
        opts.Authority = builder.Configuration["IdentityServiceUrl"];
        opts.RequireHttpsMetadata = false;
        opts.TokenValidationParameters.ValidateAudience = false;
        opts.TokenValidationParameters.NameClaimType = "username";
    });
builder.Services.AddGrpc();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddScoped<IAuctionRespository, AuctionRespository>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<GrpcAuctionService>();

try
{
    await DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}

app.Run();

public partial class Program {}
