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
    public class ChatGPTController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        public ChatGPTController(IOptions<OpenAISettings> openAIOptions)
        {
            _apiKey = openAIOptions.Value.ApiKey;
            _httpClient = new HttpClient();
        }
        [HttpPost("ask")]
        public async Task<IActionResult> AskChatGPT([FromBody] Message message)
        {
            string chatURL = "https://api.openai.com/v1/chat/completions";
            StringBuilder sb = new StringBuilder();

            HttpClient oClient = _httpClient ?? new HttpClient();
            oClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            //payload
            ChatRequest oRequest = new ChatRequest();
            oRequest.model = "gpt-3.5-turbo";
            //message from admin
            Message oMessage_admin = new Message();
            oMessage_admin.role = "system";
            oMessage_admin.content = "You are Helpful assisatant for E-comeerce Store for Tech Products, Store name is TechXpress.";
            Message oMessage_user = new Message();
            oMessage_user.role = "user";
            oMessage_user.content = message.content;

            oRequest.messages = new Message[] { oMessage_admin, oMessage_user };

            string oReqJSON = JsonConvert.SerializeObject(oRequest);
            HttpContent oContent = new StringContent(oReqJSON, Encoding.UTF8, "application/json");

            HttpResponseMessage oResponseMessage = await oClient.PostAsync(chatURL, oContent);

            if (oResponseMessage.IsSuccessStatusCode)
            {
                string oResJSON = await oResponseMessage.Content.ReadAsStringAsync();

                ChatResponse oResponse = JsonConvert.DeserializeObject<ChatResponse>(oResJSON);

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
                string oFailReason = await oResponseMessage.Content.ReadAsStringAsync();
                return BadRequest(oFailReason); ;
            }
        }

    }
}
