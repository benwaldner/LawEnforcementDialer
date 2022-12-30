using Azure.Communication.CallAutomation;
using Azure.Messaging;
using CallAutomation.Contracts;
using CallAutomation.Extensions;
using CallAutomation.Extensions.Interfaces;
using CallNotificationService.Client;
using Serilog;
using System.Reflection;
using LawEnforcementDialer.Api;
using LawEnforcementDialer.Api.Handlers;
using LawEnforcementDialer.Api.Interfaces;
using LawEnforcementDialer.CallManager;
using LawEnforcementDialer.Persistence;
using LawEnforcementDialer.PinManager;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ICallManagerService, CallManagerService>();
builder.Services.AddSingleton<IActiveCallRepository, InMemoryActiveCallRepository>();
builder.Services.AddSingleton<ICallTargetRepository, InMemoryCallTargetRepository>();
builder.Services.AddSingleton<IDialogController, DialogController>();

// add pin management services and worker
builder.Services.AddPinManager(builder.Configuration);

// add Call Automation client and handlers
builder.Services
    .AddCallAutomationClient(builder.Configuration["ACS:ConnectionString"])
    .AddCallAutomationEventHandling()
    .AddAutomaticHandlerDiscovery(Assembly.GetExecutingAssembly());

// CNS
builder.Services
    .AddCallNotificationServiceClient(builder.Configuration, builder.Configuration["VS_TUNNEL_URL"])
    .AddRegistrationWorker();

// add Serilog
builder.Host.UseSerilog((ctx, lc) => lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));

var midCallEventsUri = builder.Configuration["VS_TUNNEL_URL"] + "api/callbacks";

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/callNotification", async (CallNotification callNotification, CallAutomationClient client, ICallManagerService callManagerService) =>
{
    var response = await client
        .Answer(callNotification)
        .WithCallbackUri(midCallEventsUri)
        .OnCallConnected<PinHandler>()
        .OnCallDisconnected(async () => await callManagerService.RemoveActiveCallAsync(callNotification.CorrelationId))
        .ExecuteAsync();

    await callManagerService.SaveActiveCallAsync(new ActiveCall()
    {
        Id = response.Value.CallConnection.CallConnectionId,
        Source = response.Value.CallConnectionProperties.CallSource.Identifier.RawId,
        Target = response.Value.CallConnectionProperties.Targets.FirstOrDefault().RawId
    });
});

app.MapPost("/api/callbacks", async (CloudEvent[] events, ICallAutomationEventPublisher publisher) => await publisher.PublishAsync(events));

app.MapPost("/api/callRecording", async (CloudEvent[] events, ILogger<Program> logger) =>
{
    foreach (var cloudEvent in events)
    {
        var @event = CallAutomationEventParser.Parse(cloudEvent);
        if (@event is RecordingStateChanged recordingStateChanged)
        {
            logger.LogInformation($"Recording message: {@event.ResultInformation?.Message}");
        }
    }
});

app.Run();