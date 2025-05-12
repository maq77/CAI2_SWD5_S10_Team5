using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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

        public string Name => "PayPal";

        public PayPalGateway(IOptions<PayPalSettings> paypalSettings, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("PayPal");
            _clientId = paypalSettings.Value.ClientId;
            _secret = paypalSettings.Value.Secret;
            _isSandbox = paypalSettings.Value.UseSandbox;

            // Configure the base address based on environment
            _httpClient.BaseAddress = new Uri(_isSandbox
                ? "https://api-m.sandbox.paypal.com/"
                : "https://api-m.paypal.com/");
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            // Get access token
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "v1/oauth2/token");
            tokenRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_secret}")));
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "grant_type", "client_credentials" } });

            var tokenResponse = await _httpClient.SendAsync(tokenRequest);
            tokenResponse.EnsureSuccessStatusCode();
            var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            var accessToken = tokenResult["access_token"].ToString();

            // Create order
            var paymentRequest = new HttpRequestMessage(HttpMethod.Post, "v2/checkout/orders");
            paymentRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var orderRequest = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                new {
                    reference_id = request.OrderId,
                    amount = new {
                        currency_code = request.Currency,
                        value = request.Amount
                    },
                    description = request.Description
                }
            },
                application_context = new
                {
                    return_url = request.ReturnUrl,
                    cancel_url = request.CancelUrl
                }
            };

            paymentRequest.Content = new StringContent(
                JsonSerializer.Serialize(orderRequest),
                Encoding.UTF8,
                "application/json");

            var paymentResponse = await _httpClient.SendAsync(paymentRequest);
            if (!paymentResponse.IsSuccessStatusCode)
            {
                var errorJson = await paymentResponse.Content.ReadAsStringAsync();
                Console.WriteLine("PayPal Error Response: " + errorJson);
                throw new Exception($"PayPal payment failed: {paymentResponse.StatusCode} - {errorJson}");
            }

            paymentResponse.EnsureSuccessStatusCode();
            var result = await paymentResponse.Content.ReadFromJsonAsync<Dictionary<string,object>>();
            var links = ((JsonElement)result["links"]).EnumerateArray().ToList();
            var approvalUrl = links.FirstOrDefault(x => x.GetProperty("rel").GetString() == "approve").GetProperty("href").GetString();

            return new PaymentResponse
            {
                Success = true,
                TransactionId = result["id"].ToString(),
                RedirectUrl = approvalUrl
            };
        }

        public async Task<PaymentResponse> VerifyPaymentAsync(HttpRequest request)
        {
            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();
            }
            var queryString = request.QueryString.Value;
            // Handle PayPal IPN or return URL validation
            var query = HttpUtility.ParseQueryString(queryString);
            var orderId = query["token"];

            if (string.IsNullOrEmpty(orderId))
            {
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = "Order ID not found"
                };
            }

            // Get access token
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "v1/oauth2/token");
            tokenRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_secret}")));
            tokenRequest.Content = new FormUrlEncodedContent(new Dictionary<string, string> { { "grant_type", "client_credentials" } });

            var tokenResponse = await _httpClient.SendAsync(tokenRequest);

            tokenResponse.EnsureSuccessStatusCode();
            var tokenResult = await tokenResponse.Content.ReadFromJsonAsync<PayPalTokenResponse>();
            var accessToken = tokenResult.access_token;

            // Capture payment
            var captureRequest = new HttpRequestMessage(HttpMethod.Post, $"v2/checkout/orders/{orderId}/capture");
            captureRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            captureRequest.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var captureResponse = await _httpClient.SendAsync(captureRequest);
            if (!captureResponse.IsSuccessStatusCode)
            {
                var errorContent = await captureResponse.Content.ReadAsStringAsync();
                Console.WriteLine("PayPal Error Response: " + errorContent);
                return new PaymentResponse
                {
                    Success = false,
                    ErrorMessage = $"Failed to capture payment: {errorContent}"
                };
            }


            var result = await captureResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();

            return new PaymentResponse
            {
                Success = result["status"].ToString() == "COMPLETED",
                TransactionId = result["id"].ToString()
            };
        }
    }
}
