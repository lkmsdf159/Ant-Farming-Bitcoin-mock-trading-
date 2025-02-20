using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace _1111_MemberSerivce.User
{
    /// <summary>
    /// 업비트 현재가 정보 데이터 계약
    /// </summary>
    [DataContract]
    public class UpbitTicker
    {
        /// <summary>
        /// 현재가
        /// </summary>
        [DataMember]
        public decimal trade_price { get; set; }

        /// <summary>
        /// 전일 대비 변화액
        /// </summary>
        [DataMember]
        public decimal change_price { get; set; }

        /// <summary>
        /// 전일 대비 변화율
        /// </summary>
        [DataMember]
        public decimal change_rate { get; set; }


    }

    /// <summary>
    /// 업비트 캔들 데이터 계약
    /// </summary>
    [DataContract]
    public class UpbitCandle
    {
        /// <summary>
        /// 캔들 생성 시각 (KST)
        /// </summary>
        [DataMember]
        public DateTime candle_date_time_kst { get; set; }

        /// <summary>
        /// 시가
        /// </summary>
        [DataMember]
        public decimal opening_price { get; set; }

        /// <summary>
        /// 고가
        /// </summary>
        [DataMember]
        public decimal high_price { get; set; }

        /// <summary>
        /// 저가
        /// </summary>
        [DataMember]
        public decimal low_price { get; set; }

        /// <summary>
        /// 종가
        /// </summary>
        [DataMember]
        public decimal trade_price { get; set; }
    }
}