using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VerificationProvider.Data.Contexts;
using VerificationProvider.Data.Entities;
using VerificationProvider.Models;

namespace VerificationProvider.Services;

public class VerificationService(ILogger<VerificationService> logger, IServiceProvider serviceProvider)
{
    private readonly ILogger<VerificationService> _logger = logger;
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public VerificationRequest UnpackVerificationRequest(ServiceBusReceivedMessage message)
    {
        try
        {
            var verificationRequest = JsonConvert.DeserializeObject<VerificationRequest>(message.Body.ToString());
            if (verificationRequest != null && !string.IsNullOrEmpty(verificationRequest.Email))
            {
                return verificationRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateVerificationCode.UnpackVerificationRequest() :: {ex.Message}");
        }
        return null!;
    }

    public string GenerateCode()
    {
        try
        {
            var rnd = new Random();
            var code = rnd.Next(100000, 999999);

            return code.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateVerificationCode.GenerateCode() :: {ex.Message}");
        }
        return null!;
    }

    public async Task<bool> SaveVerificationRequest(VerificationRequest verificationRequest, string code)
    {
        try
        {
            using var context = _serviceProvider.GetRequiredService<DataContext>();

            var existingRequest = await context.VerificationRequests.FirstOrDefaultAsync(x => x.Email == verificationRequest.Email);
            if (existingRequest != null)
            {
                existingRequest.Code = code;
                existingRequest.ExpiryDate = DateTime.UtcNow.AddMinutes(10);
                context.Entry(existingRequest).State = EntityState.Modified;
            }
            else
            {
                context.VerificationRequests.Add(new VerificationRequestEntity()
                {
                    Email = verificationRequest.Email,
                    Code = code
                });
            }
            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateVerificationCode.SaveVerificationRequest() :: {ex.Message}");
        }
        return false;
    }

    public EmailRequest GenerateEmailRequest(VerificationRequest verificationRequest, string code)
    {
        try
        {
            if (!string.IsNullOrEmpty(verificationRequest.Email) && !string.IsNullOrEmpty(code))
            {
                var emailRequest = new EmailRequest()
                {
                    Recipient = verificationRequest.Email,
                    Subject = $"Verification Code : {code}",
                    HtmlContent = $@"
                        <!DOCTYPE html>
                        <html lang='en'>
                            <head>
                                <meta charset='UTF-8'>
                                <meta name='viewport' content='idth=device-width, initial-scale=1.0'>
                                <title>Verification Code</title>
                            </head>
                            <body>
                                <div style='color: #191919; max-width: 500px;''>
                                    <div style='background-color: #4f85f6; color: white; text-align: center; padding: 20px 0;'>
                                        <h1 style='font-weight: 400;'>Verification Code</h1>
                                    </div>
                                    <div style='background-color: #f4f4f4; padding: 1rem 2rem;'>
                                        <p>Dear user,</p>
                                        <p>We have recieved a request to verify your account using the email {verificationRequest.Email}. Please verify your account using the code:</p>
                                        <p style='font-weight: 700; text-align: center; font-size: 48px; letter-spacing: 8px;'>
                                            {code}
                                        </p>
                                        <div style='color: #191919; font-size: 11px;'>
                                            <p>This email was sent from a notification-only address that cannot accept incoming email.</p>
                                            <p>If you did not request this code, you have been super hacked</p>
                                        </div>
                                    </div>
                                    <div style='color: #191919; text-align: center; font-size: 11px;'>
                                        <p>© Silicon Sweden. All rights reserved.</p>
                                    </div>
                                </div>
                            </body>
                        </html>
                    ",
                    PlainTextContent = $"Please verify your account using the code: {code}. This email was sent from a notification-only address that cannot accept incoming email. If you did not request this code, you have been super hacked"
                };

                return emailRequest;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateVerificationCode.GenerateEmailRequest() :: {ex.Message}");
        }
        return null!;
    }

    public string GenerateServiceBusEmailRequest(EmailRequest emailRequest)
    {
        try
        {
            var payload = JsonConvert.SerializeObject(emailRequest);
            if (!string.IsNullOrEmpty(payload))
            {
                return payload;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"ERROR : GenerateVerificationCode.GenerateServiceBusEmailRequest() :: {ex.Message}");
        }
        return null!;
    }
}
