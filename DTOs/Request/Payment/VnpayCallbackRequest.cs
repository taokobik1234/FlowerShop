using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.DTOs.Request.Payment
{
    public class VnpayCallbackRequest
    {
        public IQueryCollection Query { get; set; }
    }
}