using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Data.Enums
{
    public enum OrderStatus
    {
        Pending = 0,      // Order placed but not yet processed
        Processing = 1,   // Order is being processed
        Shipped = 2,      // Order has been shipped
        Delivered = 3,    // Order has been delivered
        Canceled = 4,      // Order was canceled
        Paid = 5,        // Order has been paid
    }
}
