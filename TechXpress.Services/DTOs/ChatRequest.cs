using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Services.DTOs
{
    namespace ASPNETCoreChatGPT.Models
    {
        public class ChatRequest
        {
            public string model { get; set; }
            public Message[] messages { get; set; }
        }

        public class Message
        {
            public string role { get; set; } = "user";
            public string content { get; set; }
        }

    }
}
