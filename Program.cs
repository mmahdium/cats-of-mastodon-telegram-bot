using CatsOfMastodonBot.Models;
using CatsOfMastodonBot.Repositories;
using CatsOfMastodonBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


var builder = Host.CreateApplicationBuilder(args);

var config = AppConfigLoader.LoadConfigFromEnv();

builder.Services.AddSingleton(config);

// Database
builder.Services.AddSingleton<Database>();

builder.Services.AddSingleton<MigrationService>();

builder.Services.AddSingleton<MastodonService>();

builder.Services.AddScoped<PostRepository>();
builder.Services.AddScoped<MediaAttachmentRepository>();
/*
// Telegram Bot
builder.Services.AddSingleton<ITelegramBotClient>(sp =>
    new TelegramBotClient(builder.Configuration["Telegram:BotToken"]));

// Application Services
builder.Services.AddScoped<RepositoryService>();
builder.Services.AddSingleton<TelegramBotService>();
builder.Services.AddHostedService<PeriodicFetchService>();
builder.Services.AddHostedService<TelegramBotService>(sp => sp.GetRequiredService<TelegramBotService>());*/

// Build and run
var app = builder.Build();

using var scope = app.Services.CreateScope();

await scope.ServiceProvider
    .GetRequiredService<MigrationService>()
    .RunAsync();

await app.RunAsync();