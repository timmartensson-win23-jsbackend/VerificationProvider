using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VerificationProvider.Services;

namespace VerificationProvider.Functions;

public class ValidateCode(ILogger<ValidateCode> logger, ValidateCodeService validateCodeService)
{
    private readonly ILogger<ValidateCode> _logger = logger;
    private readonly ValidateCodeService _validateCodeService = validateCodeService;

    [Function("ValidateCode")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "validate")] HttpRequest req)
    {
        try
        {
            var validateRequest = await _validateCodeService.UnpackValidateRequestAsync(req);
            if (validateRequest != null)
            {
                var validateResult = await _validateCodeService.ValidateVerificationCodeAsync(validateRequest);
                if (validateResult == true)
                {
                    return new OkResult();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : ValidateCode.Run() :: {ex.Message}");
        }
        return new UnauthorizedResult();
    }


}
