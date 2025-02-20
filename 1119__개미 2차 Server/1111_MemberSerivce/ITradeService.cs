using _1111_MemberSerivce.User;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;


namespace _1111_MemberSerivce
{


    [ServiceContract(CallbackContract = typeof(ITradeCallback))]
    public interface ITradeService
    {
        #region 거래 관련 메서드

        /// <summary>
        /// 코인 매수
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="market">거래 시장 (예: KRW-BTC)</param>
        /// <param name="price">매수 가격</param>
        /// <param name="quantity">매수 수량</param>
        /// <returns>매수 성공 여부</returns>
        [OperationContract]
        Task<bool> Buy(string userId, string market, decimal price, decimal quantity);

        /// <summary>
        /// 코인 매도
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="market">거래 시장 (예: KRW-BTC)</param>
        /// <param name="quantity">매도 수량</param>
        /// <returns>매도 성공 여부</returns>
        [OperationContract]
        Task<bool> Sell(string userId, string market, decimal quantity);

        #endregion

        #region 잔고 및 자산 관련 메서드

        /// <summary>
        /// 사용자 잔고 조회
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <returns>현재 보유 KRW 잔고</returns>
        [OperationContract]
        decimal GetBalance(string userId);

        /// <summary>
        /// 자산 조회
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <returns>사용자 보유 자산 총액</returns>
        [OperationContract]
        decimal GetTotalAssets(string userId);

        /// <summary>
        /// 특정 코인의 보유 수량 조회
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="market">거래 시장 (예: KRW-BTC)</param>
        /// <returns>보유 수량</returns>
        [OperationContract]
        decimal GetAvailableQuantity(string userId, string market);

        #endregion

        #region 마켓 관련 메서드

        /// <summary>
        /// 지원하는 모든 마켓 정보 조회
        /// </summary>
        /// <returns>마켓 코드와 이름 딕셔너리</returns>
        [OperationContract]
        Dictionary<string, string> GetMarketNames();

        /// <summary>
        /// 마켓 인덱스를 통해 마켓 코드 조회
        /// </summary>
        /// <param name="index">마켓 인덱스</param>
        /// <returns>마켓 코드</returns>
        [OperationContract]
        string GetMarketByIndex(int index);

        /// <summary>
        /// 마켓 코드에 해당하는 마켓 이름 조회
        /// </summary>
        /// <param name="market">마켓 코드 (예: KRW-BTC)</param>
        /// <returns>마켓 이름</returns>
        [OperationContract]
        string GetMarketName(string market);

        #endregion

        #region 거래 기록 관련 메서드

        /// <summary>
        /// 거래 기록 조회
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <returns>사용자의 거래 기록 목록</returns>
        [OperationContract]
        Task<List<TradeRecord>> GetTradeRecords(string userId);

        #endregion

        #region 실시간 가격 및 캔들 관련 메서드

        /// <summary>
        /// 특정 마켓의 현재 가격 조회
        /// </summary>
        /// <param name="market">마켓 코드 (예: KRW-BTC)</param>
        /// <returns>마켓의 현재 가격 정보</returns>
        [OperationContract]
        Task<UpbitTicker> GetCurrentPrice(string market);

        /// <summary>
        /// 특정 마켓의 5분 간격 캔들 데이터 조회
        /// </summary>
        /// <param name="market">마켓 코드 (예: KRW-BTC)</param>
        /// <returns>마켓의 5분 캔들 데이터</returns>
        [OperationContract]
        Task<UpbitCandle[]> GetCandles(string market);

        #endregion

        #region 거래 정보 관련 메서드

        /// <summary>
        /// 사용자의 모든 거래 정보 조회
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <returns>사용자의 거래 정보</returns>
        //[OperationContract(IsOneWay = true)]
        //Task<TradeInfo[]> GetAllTradeInfos(string userId);


        [OperationContract(IsOneWay = true)]
        void TradeInfosCallback(string userId);

        #endregion

    }


    public interface ITradeCallback
    {
        [OperationContract(IsOneWay = true)]
        void UpdateTradeInfo(string market, string marketName, decimal totalQuantity, decimal averagePrice, decimal currentPrice);

        [OperationContract(IsOneWay = true)]
        void NotifyError(string message);
    }
}



