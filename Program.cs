using DiscordBot;
using DiscordBot.Models;
using DiscordBot.Utilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NetCord;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ComponentInteractions;

var modRoles = RaidFilesLoader.LoadSettings();
var adminIds = SecretsLoader.GetAdminIds();
BotApplicationSettings bas = new()
{
    AdminIds = adminIds,
    ModerationRoles = modRoles
};

string token = string.Empty;
try
{
    token = SecretsLoader.GetToken();
}
catch
{
    Console.WriteLine("secrets.json is failed to load");
    Console.ReadLine();
    return;
}

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddDiscordGateway(config =>
    {
        config.Token = token;
        config.Intents = NetCord.Gateway.GatewayIntents.All;
    })
    .AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>()
    .AddComponentInteractions<ModalInteraction, ModalInteractionContext>()
    .AddApplicationCommands()
    .AddGatewayHandlers(typeof(Program).Assembly);

builder.Services
    .AddSingleton(bas)
    .AddSingleton(new PollContainer());

var host = builder.Build();

host.AddModules(typeof(Program).Assembly);

host.UseGatewayHandlers();

await host.RunAsync();