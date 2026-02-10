using Verendar.Notification.Application.Dtos.ESms;
using Verendar.Notification.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace Verendar.Notification.Infrastructure.ExternalServices.ESms
{
    public class ESmsService : IESmsService
    {
        private readonly HttpClient _httpClient;
        private readonly ESmsOptions _options;
        private readonly ILogger<ESmsService> _logger;
        private const string GetBalanceEndpoint = "GetBalance_json/";
        private const string SendOtpEndpoint = "SendMultipleMessage_V4_post_json/";
        private const string SendSmsEndpoint = "SendMultipleMessage_V4_post_json/";
        private const string SendZaloZnsEndpoint = "SendZalo";

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
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("eSMS API returned non-success status: {StatusCode}, Response: {Response}",
                        response.StatusCode, errorContent);

                    return new ESmsBalanceResponse
                    {
                        CodeResult = "999",
                        Balance = 0,
                        UserName = string.Empty
                    };
                }

                var result = await DeserializeResponseAsync<ESmsBalanceResponse>(response.Content, "GetBalance");

                return result ?? new ESmsBalanceResponse
                {
                    CodeResult = "999",
                    Balance = 0,
                    UserName = string.Empty
                };
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
                    RequestId = requestId ?? Guid.NewGuid().ToString(),
                    Sandbox = _options.Sandbox
                };

                _logger.LogInformation("Sending OTP to {Phone}, RequestId: {RequestId}, Sandbox: {Sandbox}",
                    normalizedPhone, request.RequestId, request.Sandbox);

                var requestUrl = new Uri(_httpClient.BaseAddress!, SendOtpEndpoint);
                _logger.LogDebug("eSMS Request URL: {Url}, Endpoint: {Endpoint}",
                    _httpClient.BaseAddress, SendOtpEndpoint);

                var response = await _httpClient.PostAsJsonAsync(SendOtpEndpoint, request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("eSMS API returned non-success status: {StatusCode}, Response: {Response}",
                        response.StatusCode, errorContent);

                    return new ESmsResponse
                    {
                        CodeResult = "999",
                        ErrorMessage = $"HTTP {response.StatusCode}: {errorContent}"
                    };
                }

                var result = await DeserializeResponseAsync<ESmsResponse>(response.Content, "SendOTP");

                return result ?? new ESmsResponse
                {
                    CodeResult = "999",
                    ErrorMessage = "Empty or invalid response from eSMS API"
                };
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
                    RequestId = requestId ?? Guid.NewGuid().ToString(),
                    Sandbox = _options.Sandbox
                };

                _logger.LogInformation("Sending SMS to {Phone}, RequestId: {RequestId}, Sandbox: {Sandbox}",
                                    normalizedPhone, request.RequestId, request.Sandbox);

                _logger.LogDebug("eSMS Request URL: {Url}, Endpoint: {Endpoint}",
                    _httpClient.BaseAddress, SendSmsEndpoint);

                var response = await _httpClient.PostAsJsonAsync(SendSmsEndpoint, request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("eSMS API returned non-success status: {StatusCode}, Response: {Response}",
                        response.StatusCode, errorContent);

                    return new ESmsResponse
                    {
                        CodeResult = "999",
                        ErrorMessage = $"HTTP {response.StatusCode}: {errorContent}"
                    };
                }

                var result = await DeserializeResponseAsync<ESmsResponse>(response.Content, "SendSMS");

                if (result == null)
                {
                    return new ESmsResponse
                    {
                        CodeResult = "999",
                        ErrorMessage = "Empty or invalid response from eSMS API"
                    };
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
                    RequestId = requestId ?? Guid.NewGuid().ToString(),
                    Sandbox = _options.Sandbox
                };

                _logger.LogInformation("Sending Zalo ZNS to {Phone}, Template: {TemplateId}, RequestId: {RequestId}, Sandbox: {Sandbox}",
                    normalizedPhone, templateId, request.RequestId, request.Sandbox);

                _logger.LogDebug("eSMS Request URL: {Url}, Endpoint: {Endpoint}",
                    _httpClient.BaseAddress, SendZaloZnsEndpoint);

                var response = await _httpClient.PostAsJsonAsync(SendZaloZnsEndpoint, request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("eSMS Zalo API returned non-success status: {StatusCode}, Response: {Response}",
                        response.StatusCode, errorContent);

                    return new ESmsResponse
                    {
                        CodeResult = "999",
                        ErrorMessage = $"HTTP {response.StatusCode}: {errorContent}"
                    };
                }

                var result = await DeserializeResponseAsync<ESmsResponse>(response.Content, "SendZaloZNS");

                if (result == null)
                {
                    return new ESmsResponse
                    {
                        CodeResult = "999",
                        ErrorMessage = "Empty or invalid response from eSMS Zalo API"
                    };
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

        private async Task<T?> DeserializeResponseAsync<T>(HttpContent content, string operationName)
        {
            string? responseBody = null;

            try
            {
                responseBody = await content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseBody))
                {
                    _logger.LogWarning("Empty response body received for {Operation}", operationName);
                    return default;
                }

                // Check if response is valid JSON (starts with { or [)
                var trimmedBody = responseBody.TrimStart();
                if (!trimmedBody.StartsWith('{') && !trimmedBody.StartsWith('['))
                {
                    _logger.LogWarning("Non-JSON response received for {Operation}. Response: {Response}",
                        operationName, responseBody);
                    return default;
                }

                return JsonSerializer.Deserialize<T>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize {Operation} response. Response body: {ResponseBody}",
                    operationName, responseBody ?? "Unable to read response body");
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deserializing {Operation} response. Response body: {ResponseBody}",
                    operationName, responseBody ?? "Unable to read response body");
                return default;
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
}
