using SB.Tickets.Api.Config;
using SB.Tickets.Api.Hubs;
using SB.Tickets.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ImportConfig>(builder.Configuration.GetSection("Import"));

builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<ILocationService, LocationService>();
builder.Services.AddSingleton<ITipTypeService, TipTypeService>();
builder.Services.AddSingleton<IMatchService, MatchService>();
builder.Services.AddSingleton<ITicketService, TicketService>();

builder.Services.AddControllers();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        b =>
        {
            string[] corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>();
            b.WithOrigins(corsOrigins)
                .AllowAnyHeader()
                .WithMethods("GET", "POST")
                .AllowCredentials();
        });
});

builder.WebHost.UseUrls("http://*:5066");

var app = builder.Build();
Console.WriteLine("SB.Tickets");

ITicketService ticketService = app.Services.GetRequiredService<ITicketService>();

app.UseAuthorization();

// UseCors must be called before MapHub.
app.UseCors();

app.MapControllers();
app.MapHub<TicketHub>("/hubs/tickets");

app.Run();
