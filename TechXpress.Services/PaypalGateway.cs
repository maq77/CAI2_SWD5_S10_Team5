using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Stripe;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Web;
using TechXpress.Services.Base;
using TechXpress.Services.DTOs;

namespace TechXpress.Services
{
    public class PayPalGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly string _clientId;
        private readonly string _secret;
        private readonly bool _isSandbox;
        private readonly ILogger<PayPalGateway> _logger;

        public string Name => "PayPal";

        public PayPalGateway(
            IOptions<PayPalSettings> paypalSettings,
            IHttpClientFactory httpClientFactory,
            ILogger<PayPalGateway> logger)
        {
            _httpClient = httpClientFactory.CreateClient("PayPal");
            _clientId = paypalSettings.Value.ClientId;
            _secret = paypalSettings.Value.Secret;
            _isSandbox = paypalSettings.Value.UseSandbox;
            _logger = logger;

            // Configure the base address based on environment
            _httpClient.BaseAddress = new Uri(_isSandbox
                ? "https://api-m.sandbox.paypal.com/"
                : "https://api-m.paypal.com/");
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                // Get access token
                var accessToken = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("Failed to obtain PayPal access token.");
                }

                // Create order
                var paymentRequest = new HttpRequestMessage(HttpMethod.Post, "v2/checkout/orders");
                paymentRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var orderRequest = new
                {
                    intent = "CAPTURE",
                    payment_source = new
                    {
                        paypal = new
                        {
                            experience_context = new
                            {
                                return_url = request.ReturnUrl,
                                cancel_url = request.CancelUrl
                            }
                        }
                    },
                    purchase_units = new[]
                    {
                        new {
                            invoice_id = (request.OrderId.ToString()=="0") ? Guid.NewGuid().ToString() : request.OrderId.ToString(),
                            amount = new {
                                currency_code = request.Currency,
                                value = request.Amount.ToString("F2", CultureInfo.InvariantCulture)
                            },
                            //description = request.Description
                        }
                    }
                };

                paymentRequest.Content = new StringContent(
                    JsonSerializer.Serialize(orderRequest),
                    Encoding.UTF8,
                    "application/json");

                paymentRequest.Headers.Add("PayPal-Request-Id", Guid.NewGuid().ToString());

                var paymentResponse = await _httpClient.SendAsync(paymentRequest);

                if (!paymentResponse.IsSuccessStatusCode)
                {
                    return await HandleErrorResponseAsync(paymentResponse, "ProcessPayment");
                }

                var result = await paymentResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                var links = ((JsonElement)result["links"]).EnumerateArray().ToList();
                var approvalUrl = links.FirstOrDefault(x => x.GetProperty("rel").GetString() == "payer-action").GetProperty("href").GetString();

                if (string.IsNullOrEmpty(approvalUrl))
                {
                    _logger.LogError("PayPal didn't return an approval URL");
                    return new PaymentResponse
                    {
                        Success = false,
                        ErrorMessage = "Payment gateway didn't return a redirect URL"
                    };
                }

                return new PaymentResponse
                {
                    Success = true,
                    TransactionId = result["id"].ToString(),
                    RedirectUrl = approvalUrl
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in PayPal payment processing");
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = "An unexpected error occurred while processing your payment"
                };
            }
        }

        public async Task<PaymentResponse> VerifyPaymentAsync(HttpRequest request)
        {
            try
            {
                var queryString = request.QueryString.Value;
                var query = HttpUtility.ParseQueryString(queryString);
                var orderId = query["token"];

                if (string.IsNullOrEmpty(orderId))
                {
                    _logger.LogWarning("PayPal verification called without order ID");
                    return new PaymentResponse
                    {
                        Success = false,
                        ErrorMessage = "Order ID not found in the return URL"
                    };
                }

                // Get access token
                var accessToken = await GetAccessTokenAsync();
                _logger.LogInformation("Sending PayPal capture request for access token: {accessToken}", accessToken);

                // Capture payment
                var captureRequest = new HttpRequestMessage(HttpMethod.Post, $"v2/checkout/orders/{orderId}/capture");
                captureRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                captureRequest.Content = new StringContent("{}", Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending PayPal capture request for Order ID: {OrderId}", orderId);

                if (captureRequest.Content != null)
                {
                    var content = await captureRequest.Content.ReadAsStringAsync();
                    _logger.LogInformation("PayPal Capture Request Body: {Content}", content);
                }
                var captureResponse = await _httpClient.SendAsync(captureRequest);
                var responseContent = await captureResponse.Content.ReadAsStringAsync();

                _logger.LogInformation("PayPal Capture Response: {StatusCode} - {Content}", captureResponse.StatusCode, responseContent);

                if (!captureResponse.IsSuccessStatusCode)
                {
                    return await HandleErrorResponseAsync(captureResponse, "CapturePayment");
                }

                var result = await captureResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
                bool isCompleted = result["status"].ToString() == "COMPLETED";

                if (!isCompleted)
                {
                    _logger.LogWarning($"PayPal payment not completed. Status: {result["status"]}");
                }

                return new PaymentResponse
                {
                    Success = isCompleted,
                    TransactionId = result["id"].ToString(),
                    Status = result["status"].ToString()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying PayPal payment");
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = "An error occurred while verifying your payment"
                };
            }
        }

        #region Helper Methods

        private async Task<string> GetAccessTokenAsync()
        {
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "v1/oauth2/token");
            tokenRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_secret}")));
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "grant_type", "client_credentials" } });

            var tokenResponse = await _httpClient.SendAsync(tokenRequest);

            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                _logger.LogError($"Failed to obtain PayPal access token: {errorContent}");
                throw new Exception("Failed to authenticate with payment provider");
            }

            var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<PayPalTokenResponse>();
            return tokenResult.access_token;
        }

        private async Task<PaymentResponse> HandleErrorResponseAsync(HttpResponseMessage response, string operation)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError($"PayPal {operation} Error: {response.StatusCode} - {errorContent}");

            try
            {
                // Try to parse the error response
                var errorJson = JsonSerializer.Deserialize<PayPalErrorResponse>(errorContent);

                // Handle specific error types
                if (errorJson?.Details?.Count > 0)
                {
                    foreach (var detail in errorJson.Details)
                    {
                        // Special handling for compliance violations
                        if (detail.Issue == "COMPLIANCE_VIOLATION")
                        {
                            return new PaymentResponse
                            {
                                Success = false,
                                ErrorCode = detail.Issue,
                                ErrorMessage = "This transaction cannot be processed due to security restrictions. Please try a different payment method.",
                                DebugId = errorJson.DebugId
                            };
                        }

                        // Handle insufficient funds
                        if (detail.Issue == "INSUFFICIENT_FUNDS")
                        {
                            return new PaymentResponse
                            {
                                Success = false,
                                ErrorCode = detail.Issue,
                                ErrorMessage = "Your account has insufficient funds. Please use a different payment method.",
                                DebugId = errorJson.DebugId
                            };
                        }
                    }
                }

                // Generic error with parsed information
                return new PaymentResponse
                {
                    Success = false,
                    ErrorCode = errorJson?.Name,
                    ErrorMessage = errorJson?.Message ?? "Payment processing failed",
                    DebugId = errorJson?.DebugId
                };
            }
            catch
            {
                // If error parsing fails, return a generic error
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = $"Payment gateway error: {response.StatusCode}"
                };
            }
        }

        #endregion
    }

    #region PayPal Response Models

    public class PayPalTokenResponse
    {
        public string scope { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string app_id { get; set; }
        public int expires_in { get; set; }
        public string nonce { get; set; }
    }

    public class PayPalErrorResponse
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public string DebugId { get; set; }
        public List<PayPalErrorDetail> Details { get; set; } = new List<PayPalErrorDetail>();
        public List<PayPalLink> Links { get; set; } = new List<PayPalLink>();
    }

    public class PayPalErrorDetail
    {
        public string Issue { get; set; }
        public string Description { get; set; }
    }

    public class PayPalLink
    {
        public string Href { get; set; }
        public string Rel { get; set; }
        public string Method { get; set; }
    }

    #endregion
}