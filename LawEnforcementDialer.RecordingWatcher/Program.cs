using Azure.Communication.CallAutomation;
using JasonShave.AzureStorage.QueueService.Extensions;
using JasonShave.AzureStorage.QueueService.Models;
using LawEnforcementDialer.RecordingWatcher;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton(new CallAutomationClient(hostContext.Configuration["ACS:ConnectionString"]));

        services.AddHostedService<RecordingWorker>();

        // get configuration from IConfiguration binder
        services.AddAzureStorageQueueServices(options => hostContext.Configuration.Bind(nameof(QueueClientSettings), options));

        // optionally customize JsonSerializerOptions
        services.AddAzureStorageQueueServices(
            options => hostContext.Configuration.Bind(nameof(QueueClientSettings), options),
            serializationOptions => serializationOptions.AllowTrailingCommas = true);
    })
    .Build();

host.Run();
