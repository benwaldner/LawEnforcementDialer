using System.Text.Json;
using Azure;
using Azure.Communication.CallAutomation;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using JasonShave.AzureStorage.QueueService.Interfaces;

namespace LawEnforcementDialer.RecordingWatcher
{
    public class RecordingWorker : BackgroundService
    {
        private readonly IQueueService _queueService;
        private readonly CallAutomationClient _callAutomationClient;
        private readonly ILogger<RecordingWorker> _logger;

        public RecordingWorker(IQueueService queueService, CallAutomationClient callAutomationClient, ILogger<RecordingWorker> logger)
        {
            _queueService = queueService;
            _callAutomationClient = callAutomationClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _queueService.ReceiveMessagesAsync<EventGridEvent>(ProcessMessage, ProcessError, stoppingToken);
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task ProcessMessage(EventGridEvent? message)
        {
            if (message is null) return;

            var status = JsonSerializer.Deserialize<AcsRecordingFileStatusUpdatedEventData>(message.Data);

            await using FileStream fileStream = new($"C:\\{Guid.NewGuid()}-{DateTime.UtcNow.ToLocalTime()}.mp3", FileMode.CreateNew, FileAccess.ReadWrite);

            foreach (var recordingChunk in status.RecordingStorageInfo.RecordingChunks)
            {
                var recordingDownloadUri = new Uri(recordingChunk.ContentLocation);
                try
                {
                    var response = await _callAutomationClient.GetCallRecording().DownloadStreamingAsync(recordingDownloadUri);
                    await response.Value.CopyToAsync(fileStream);
                }
                catch (RequestFailedException e)
                {
                    return;
                }
            }
        }

        private async Task ProcessError(Exception ex)
        {
            _logger.LogError($"{ex.Message}", ex);
        }
    }
}