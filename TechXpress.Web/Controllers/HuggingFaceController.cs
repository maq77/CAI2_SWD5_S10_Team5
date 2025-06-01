using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using TechXpress.Services.DTOs;
using TechXpress.Services.DTOs.ASPNETCoreChatGPT.Models;

namespace TechXpress.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class HuggingFaceController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public HuggingFaceController(IOptions<HuggingFaceSettings> huggingFaceOptions)
        {
            _apiKey = huggingFaceOptions.Value.ApiKey;
            _httpClient = new HttpClient();
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskAI([FromBody] Message message)
        {
            try
            {

                // Check if message is null or empty
                if (message == null || string.IsNullOrEmpty(message.content))
                {
                    return BadRequest(new { Success = false, Message = "Message content cannot be empty" });
                }

                // Hugging Face API endpoint for a free model
                string apiUrl = "https://router.huggingface.co/hf-inference/models/mistralai/Mistral-7B-Instruct-v0.3/v1/chat/completions";
                StringBuilder sb = new StringBuilder();

                HttpClient client = _httpClient ?? new HttpClient();

                // Ensure we have the API key
                if (string.IsNullOrEmpty(_apiKey))
                {
                    return StatusCode(500, new { Success = false, Message = "API key is not configured" });
                }

                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

                // Prepare the prompt with system and user message
                Message oMessage_admin = new Message();
                oMessage_admin.role = "system";
                oMessage_admin.content = "You are a helpful assistant for the TechXpress e-commerce store that sells tech products.";
                Message oMessage_user = new Message();
                oMessage_user.role = "user";
                oMessage_user.content = message.content;
                ChatRequest oRequest = new();
                oRequest.model = "mistralai/Mistral-7B-Instruct-v0.3";
                oRequest.messages = new Message[] { oMessage_admin, oMessage_user };
                string jsonPayload = JsonConvert.SerializeObject(oRequest);
                HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseJson = await response.Content.ReadAsStringAsync();

                    // Log the raw response for debugging
                    Console.WriteLine($"Raw Hugging Face response: {responseJson}");

                    // Handle response as JSON array
                    ChatResponse oResponse = JsonConvert.DeserializeObject<ChatResponse>(responseJson);

                    foreach (Choice c in oResponse.choices)
                    {
                        sb.Append(c.message.content);
                    }
                    HttpChatGPTResponse oHttpResponse = new HttpChatGPTResponse()
                    {
                        Success = true,
                        Data = sb.ToString()
                    };

                    return Ok(oHttpResponse);
                }
                else
                {
                    string failReason = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API call failed: {response.StatusCode}, Reason: {failReason}");

                    // If the model is loading, return a specific message
                    if (failReason.Contains("loading") || failReason.Contains("initializing"))
                    {
                        return Ok(new HttpChatGPTResponse()
                        {
                            Success = true,
                            Data = "The AI model is currently loading. This happens when the model hasn't been used recently. Please try your request again in about 20-30 seconds."
                        });
                    }

                    return Ok(new HttpChatGPTResponse
                    {
                        Success = false,
                        Data = null,
                        Error = "Error from AI service: " + failReason
                    });

                }
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON Parsing Error: {jsonEx.Message}");
                return Ok(new HttpChatGPTResponse
                {
                    Success = false,
                    Data = null,
                    Error = "Message content cannot be empty"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return Ok(new HttpChatGPTResponse
                {
                    Success = false,
                    Data = null,
                    Error = "An unexpected error occurred: " + ex.Message
                });

            }
        }
    }
}