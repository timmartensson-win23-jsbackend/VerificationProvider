using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VerificationProvider.Data.Contexts;

namespace VerificationProvider.Services;

public class CleanerService(DataContext context, ILogger<CleanerService> logger)
{
    private readonly DataContext _context = context;
    private readonly ILogger<CleanerService> _logger = logger;

    public async Task RemoveExpiredRequestsAsync()
    {
        try
        {
            var expired = await _context.VerificationRequests.Where(x => x.ExpiryDate <= DateTime.Now).ToListAsync();
            _context.RemoveRange(expired);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : CleanerService.RemoveExpiredRequestsAsync :: {ex.Message}");
        }
    }
}
