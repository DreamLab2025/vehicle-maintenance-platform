using Verendar.Notification.Application.Dtos.ESms;
using Verendar.Notification.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace Verendar.Notification.Infrastructure.ExternalServices.ESms;

public class ESmsService : IESmsService
{
    private readonly HttpClient _httpClient;
    private readonly ESmsOptions _options;
    private readonly ILogger<ESmsService> _logger;
    private const string GetBalanceEndpoint = "/GetBalance_json/";
    private const string SendOtpEndpoint = "/SendMultipleMessage_V4_post_json/";
    private const string SendSmsEndpoint = "/SendMultipleMessage_V4_post_json/";
    private const string SendZaloZnsEndpoint = "/SendZalo";

    public ESmsService(HttpClient httpClient, IOptions<ESmsOptions>
    options, ILogger<ESmsService> logger)
    {
        _options = options.Value;
        _logger = logger;

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.Timeout);
    }

    public async Task<ESmsBalanceResponse> GetBalanceAsync()
    {
        try
        {
            _logger.LogInformation("Getting balance from eSMS API");

            var response = await _httpClient.GetAsync(GetBalanceEndpoint);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to get balance from eSMS API");
            }

            var result = await response.Content.ReadFromJsonAsync<ESmsBalanceResponse>();

            if (result == null)
            {
                throw new Exception("Empty response from eSMS API");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get balance from eSMS API");
            return new ESmsBalanceResponse
            {
                CodeResult = "999",
                Balance = 0,
                UserName = string.Empty
            };
        }
    }

    public async Task<ESmsResponse> SendOtpAsync(string phoneNumber, string otpCode, string? requestId = null)
    {
        try
        {
            var normalizedPhone = NormalizePhoneNumber(phoneNumber);

            var request = new ESmsRequest
            {
                ApiKey = _options.ApiKey,
                SecretKey = _options.SecretKey,
                Phone = normalizedPhone,
                Content = $"Mã OTP của bạn là: {otpCode}",
                SmsType = 4,
                Brandname = _options.BrandName,
                RequestId = requestId ?? Guid.NewGuid().ToString()
            };

            _logger.LogInformation("Sending OTP to {Phone}, RequestId: {RequestId}",
                normalizedPhone, request.RequestId);

            var response = await _httpClient.PostAsJsonAsync(SendOtpEndpoint, request);
            var result = await response.Content.ReadFromJsonAsync<ESmsResponse>();

            if (result == null)
            {
                throw new Exception("Empty response from eSMS API");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OTP to {Phone}", phoneNumber);
            return new ESmsResponse
            {
                CodeResult = "999",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ESmsResponse> SendSmsAsync(string phoneNumber, string content, string? requestId = null)
    {
        try
        {
            var normalizedPhone = NormalizePhoneNumber(phoneNumber);
            var request = new ESmsRequest
            {
                ApiKey = _options.ApiKey,
                SecretKey = _options.SecretKey,
                Phone = normalizedPhone,
                Content = content,
                SmsType = _options.SmsType,
                Brandname = _options.BrandName,
                RequestId = requestId ?? Guid.NewGuid().ToString()
            };

            _logger.LogInformation("Sending SMS to {Phone}, RequestId: {RequestId}",
                                normalizedPhone, request.RequestId);

            var response = await _httpClient.PostAsJsonAsync(SendSmsEndpoint, request);
            var result = await response.Content.ReadFromJsonAsync<ESmsResponse>();

            if (result == null)
            {
                throw new Exception("Empty response from eSMS API");
            }

            if (!result.IsSuccess)
            {
                _logger.LogError("eSMS Error: Code={Code}, Message={Message}",
                    result.CodeResult, result.ErrorMessage);
            }
            else
            {
                _logger.LogInformation("SMS sent successfully. SMSID: {SmsId}, Balance: {Balance}",
                    result.SmsId, result.Balance);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {Phone}", phoneNumber);
            return new ESmsResponse
            {
                CodeResult = "999",
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<ESmsResponse> SendZaloZnsAsync(string phoneNumber, string templateId, Dictionary<string, string> parameters, string? requestId = null)
    {
        try
        {
            var normalizedPhone = NormalizePhoneNumber(phoneNumber);

            var paramsList = parameters.Select(p => $"{p.Key}:{p.Value}").ToList();

            var request = new ZaloZnsRequest
            {
                ApiKey = _options.ApiKey,
                SecretKey = _options.SecretKey,
                Phone = normalizedPhone,
                TemplateId = templateId,
                Params = paramsList,
                RequestId = requestId ?? Guid.NewGuid().ToString()
            };

            _logger.LogInformation("Sending Zalo ZNS to {Phone}, Template: {TemplateId}, RequestId: {RequestId}",
                normalizedPhone, templateId, request.RequestId);

            var response = await _httpClient.PostAsJsonAsync(SendZaloZnsEndpoint, request);
            var result = await response.Content.ReadFromJsonAsync<ESmsResponse>();

            if (result == null)
            {
                throw new Exception("Empty response from eSMS Zalo API");
            }

            if (!result.IsSuccess)
            {
                _logger.LogError("Zalo ZNS Error: Code={Code}, Message={Message}",
                    result.CodeResult, result.ErrorMessage);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send Zalo ZNS to {Phone}", phoneNumber);
            return new ESmsResponse
            {
                CodeResult = "999",
                ErrorMessage = ex.Message
            };
        }
    }

    private string NormalizePhoneNumber(string phoneNumber)
    {
        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("84") && digits.Length >= 10)
        {
            digits = "0" + digits.Substring(2);
        }

        return digits;
    }
}
