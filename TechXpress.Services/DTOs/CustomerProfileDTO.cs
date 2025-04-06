﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Services.DTOs
{
    public class CustomerProfileDTO
    {
        public UserProfileDTO User { get; set; }
        public IEnumerable<OrderDTO> OrderHistory { get; set; }
    }
}
