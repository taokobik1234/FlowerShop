using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd_FLOWER_SHOP.DTOs.Request.Payment
{
    public class BankingInfor
    {
        public string BankCode { get; set; }

        /// <summary>
        /// Mã giao dịch ở phía ngân hàng, được dùng để theo dõi và đối soát giao dịch với ngân hàng.
        /// </summary>
        public string BankTransactionId { get; set; }
    }
}