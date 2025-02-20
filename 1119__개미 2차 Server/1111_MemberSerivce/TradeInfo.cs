using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace _1111_MemberSerivce.User
{
    public class TradeInfo
    {

        [DataMember]
        public string Market { get; set; }

        [DataMember]
        public string MarketName { get; set; }

        [DataMember]
        public decimal Quantity { get; set; }

        [DataMember]
        public decimal AveragePrice { get; set; }

        [DataMember]
        public decimal CurrentPrice { get; set; }

        [DataMember]
        public decimal ProfitLoss { get; set; }

        [DataMember]
        public decimal TotalValue { get; set; }


        public TradeInfo()
        {          
        }

        public TradeInfo(string market, string marketName, decimal quantity,
                    decimal averagePrice, decimal currentPrice)
        {
            Market = market;
            MarketName = marketName;
            Quantity = quantity;
            AveragePrice = averagePrice;
            CurrentPrice = currentPrice;

            // 손익과 총 가치는 생성자에서 계산
            ProfitLoss = (CurrentPrice - AveragePrice) * Quantity;
            TotalValue = CurrentPrice * Quantity;
        }
    }
}
