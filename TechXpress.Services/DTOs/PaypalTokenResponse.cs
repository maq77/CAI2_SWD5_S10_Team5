﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Services.DTOs
{
    public class PayPalTokenResponse
    {
        public string id { get; set; }
        public string scope { get; set; }
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string nonce { get; set; }
    }

}
