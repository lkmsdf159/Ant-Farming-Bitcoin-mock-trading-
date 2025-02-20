using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace _1111_MemberSerivce.User
{
    [DataContract]
    public class TradeRecord
    {
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string Market { get; set; }
        [DataMember]
        public decimal BuyPrice { get; set; }
        [DataMember]
        public decimal Quantity { get; set; }
        [DataMember]
        public DateTime BuyTime { get; set; }

        public TradeRecord()
        {
            BuyTime = DateTime.Now;
        }


        public TradeRecord(string userid, string market, decimal buyprice, decimal quantity)
        {
            UserId = userid;
            Market = market;
            BuyPrice = buyprice;
            Quantity = quantity;

        }

        public TradeRecord(string userid, string market, decimal buyprice, decimal quantity, DateTime date)
        {
            UserId = userid;
            Market = market;
            BuyPrice = buyprice;
            Quantity = quantity;
            BuyTime = date;
        }

    }
}