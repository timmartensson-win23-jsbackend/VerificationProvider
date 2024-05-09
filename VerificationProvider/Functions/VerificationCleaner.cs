using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VerificationProvider.Data.Contexts;
using VerificationProvider.Services;

namespace VerificationProvider.Functions;

public class VerificationCleaner(ILogger<VerificationCleaner> logger, CleanerService cleanerService)
{
    private readonly ILogger<VerificationCleaner> _logger = logger;
    private readonly CleanerService _cleanerService = cleanerService;

    [Function("VerificationCleaner")]
    public async Task Run([TimerTrigger("0 */1 * * * *")] TimerInfo myTimer)
    {
        try
        {
            await _cleanerService.RemoveExpiredRequestsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : VerificationCleaner.Run() :: {ex.Message}");
        }
    }
}
