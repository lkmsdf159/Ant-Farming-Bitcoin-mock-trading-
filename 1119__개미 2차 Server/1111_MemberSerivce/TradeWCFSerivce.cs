using _1111_MemberSerivce.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data;

namespace _1111_MemberSerivce
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class TradeWCFSerivce : ITradeService
    {
        //private readonly RenewalService renewalService;
        private static readonly object tradeLock = new object();                    // 동시성 처리를 위한 락 객체
        private static List<TradeRecord> tradeRecords = new List<TradeRecord>();    // 거래 기록을 저장할 리스트
        private static decimal userBalance = 300000000;                             // 사용자 잔고 (초기값 설정)
        private static readonly HttpClient client = new HttpClient();               // HttpClient 객체 (Upbit API와 통신)
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
        private static DateTime lastRequestTime = DateTime.MinValue;                 // 마지막 API 요청 시간을 기록
        private const int RequestDelayMs = 100;                                      // API 요청 간격 설정 (100ms)

        private ITradeCallback callback = null;


        // 시장과 그에 해당하는 이름 매핑
        private readonly Dictionary<string, string> MarketNames = new Dictionary<string, string>
        {
            {"KRW-BTC", "비트코인"},
            {"KRW-ETH", "이더리움"},
            {"KRW-XRP", "리플"},
            {"KRW-DOGE", "도지코인"},
            {"KRW-SOL", "솔라나"},
            {"KRW-APT", "앱토스"},
            {"KRW-UNI", "유니스왑"}
        };

        #region 생성자 및 초기화


        static TradeWCFSerivce()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");          
        }
        public TradeWCFSerivce()
        {
            //renewalService = new RenewalService();

            if (OperationContext.Current != null)
            {
                try
                {
                    callback = OperationContext.Current.GetCallbackChannel<ITradeCallback>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Callback channel initialization error: {ex.Message}");
                }
            }
        }

        private async Task WaitForRateLimit()
        {
            await semaphore.WaitAsync();
            try
            {
                var timeSinceLastRequest = DateTime.Now - lastRequestTime;
                if (timeSinceLastRequest.TotalMilliseconds < RequestDelayMs)
                {
                    await Task.Delay(RequestDelayMs - (int)timeSinceLastRequest.TotalMilliseconds);
                }
                lastRequestTime = DateTime.Now;
            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion


        #region 거래 관련 메서드

        /// <summary>
        /// 매수 주문 처리
        /// </summary>
        /// <param name="userId">사용자 ID</param> 
        /// <param name="market">거래 시장</param>
        /// <param name="price">매수 가격</param>
        /// <param name="quantity">매수 수량</param>
        /// <returns>매수 성공 여부</returns>
        public async Task<bool> Buy(string userId, string market, decimal price, decimal quantity)
        {
            return await Task.Run(() =>
            {
                lock (tradeLock)
                {
                    // DB에서 현재 잔액 조회
                    decimal currentBalance = renewalService.LoadMoney(userId); // DB 연동

                    decimal totalCost = price * quantity;
                    if (totalCost > currentBalance)
                        return false;

                    // DB에 잔액 업데이트
                    if (!renewalService.SaveMoney(userId, currentBalance - totalCost)) // DB 연동
                        return false;

                    // DB에 코인 정보 저장
                    if (!renewalService.InsertCoin(userId, market, (int)quantity, price)) // DB 연동
                        return false;

                    Console.WriteLine($"매수 성공 - 유저: {userId}, 마켓: {market}, 수량: {quantity:N4}, 가격: {price:N0}");
                    return true;
                }
            });
        }

        /// <summary>
        /// 매도 주문 처리
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="market">거래 시장</param>
        /// <param name="quantity">매도 수량</param>
        /// <returns>매도 성공 여부</returns>
        public async Task<bool> Sell(string userId, string market, decimal quantity)
        {
            return await Task.Run(() =>
            {
                lock (tradeLock)
                {
                    // DB에서 현재 코인 수량 확인
                    var available = GetAvailableQuantity(userId, market);
                    if (quantity > available)
                        return false;

                    var ticker = GetCurrentPrice(market).GetAwaiter().GetResult();
                    decimal sellAmount = quantity * ticker.trade_price;

                    // DB에서 현재 잔액 조회 및 업데이트
                    decimal currentBalance = renewalService.LoadMoney(userId); // DB 연동
                    if (!renewalService.SaveMoney(userId, currentBalance + sellAmount)) // DB 연동
                        return false;

                    // DB에서 코인 정보 업데이트
                    if (quantity == available)
                    {
                        // 전량 매도 시 삭제
                        if (!renewalService.DeleteCoin(userId, market)) // DB 연동
                            return false;
                    }
                    else
                    {
                        // 부분 매도 시 업데이트
                        if (!renewalService.SaveCoin(userId, (int)(available - quantity), ticker.trade_price)) // DB 연동
                            return false;
                    }

                    return true;
                }
            });
        }

        /// <summary>
        /// 사용자 보유 수량 조회
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <param name="market">거래 시장</param>
        /// <returns>보유 수량</returns>
        public decimal GetAvailableQuantity(string userId, string market)
        {
            decimal total = 0;
            foreach (TradeRecord record in tradeRecords)
            {
                if (record.UserId == userId && record.Market == market)
                {
                    total += record.Quantity;
                }
            }
            return total;
        }

        #endregion

        #region 조회 관련 메서드

        /// <summary>
        /// 사용자 잔고 조회
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <returns>현재 잔고</returns>
        public decimal GetBalance(string userId)
        {
            // DB에서 잔액 조회
            return renewalService.LoadMoney(userId); // DB 연동
        }

        public Dictionary<string, string> GetMarketNames()
        {
            return new Dictionary<string, string>(MarketNames);
        }

        /// <summary>
        /// 사용자 거래 기록 조회
        /// </summary>
        /// <param name="userId">사용자 ID</param>
        /// <returns>사용자의 거래 기록 목록</returns>
        public async Task<List<TradeRecord>> GetTradeRecords(string userId)
        {
            return await Task.Run(() => tradeRecords.Where(r => r.UserId == userId).ToList());
            //Console.WriteLine($"거래 기록 조회 - 유저: {userId}, 기록 수: {records.Count}");
            //return records;
        }

        #endregion


        public string GetMarketByIndex(int index)
        {
            return MarketNames.Keys.ElementAt(index);
        }

        public string GetMarketName(string market)
        {
            return MarketNames.TryGetValue(market, out string name) ? name : "알 수 없음";
        }


        #region Upbit API 통신 관련 메서드


        public async Task<UpbitTicker> GetCurrentPrice(string market)
        {
            await WaitForRateLimit();
            try
            {
                var response = await client.GetStringAsync(
                    $"https://api.upbit.com/v1/ticker?markets={market}"
                );
                return JsonConvert.DeserializeObject<UpbitTicker[]>(response)[0];
            }
            catch (HttpRequestException ex)
            {
                // 429 에러 발생시 잠시 대기 후 재시도
                if (ex.Message.Contains("429"))
                {
                    await Task.Delay(1000); // 1초 대기
                    return await GetCurrentPrice(market);
                }
                throw;
            }
        }

        public async Task<UpbitCandle[]> GetCandles(string market)
        {
            await WaitForRateLimit();
            try
            {
                var response = await client.GetStringAsync(
                    $"https://api.upbit.com/v1/candles/minutes/5?market={market}&count=100"
                );
                return JsonConvert.DeserializeObject<UpbitCandle[]>(response);
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("429"))
                {
                    await Task.Delay(1000);
                    return await GetCandles(market);
                }
                throw;
            }
        }

        //일단 넣은거
        public decimal GetTotalAssets(string userId)
        {
            decimal totalAssets = userBalance;
            return totalAssets;
        }

        //public async Task<TradeInfo[]> GetAllTradeInfos(string userId)
        //{
        //    var result = new List<TradeInfo>();
        //    var groupedRecords = tradeRecords.Where(r => r.UserId == userId)
        //                                   .GroupBy(r => r.Market);

        //    foreach (var group in groupedRecords)
        //    {
        //        var market = group.Key;
        //        var ticker = await GetCurrentPrice(market);

        //        var totalQuantity = group.Sum(r => r.Quantity);
        //        var averagePrice = group.Sum(r => r.Quantity * r.BuyPrice) / totalQuantity;
        //        var currentPrice = ticker.trade_price;

        //        result.Add(new TradeInfo(
        //            market,
        //            MarketNames[market],
        //            totalQuantity,
        //            averagePrice,
        //            currentPrice
        //        ));
        //    }

        //    return result.ToArray();
        //}

        public async void TradeInfosCallback(string userId)
        {
            try
            {
                if (callback == null && OperationContext.Current != null)
                {
                    callback = OperationContext.Current.GetCallbackChannel<ITradeCallback>();
                }

                if (callback == null)
                {
                    Console.WriteLine("Callback channel is not available");
                    return;
                }

                // DB에서 코인 정보 조회
                var coins = renewalService.LoadWorker(userId); // DB 연동
                foreach (var coin in coins)
                {
                    string[] coinInfo = coin.Split('#');
                    string market = coinInfo[0];
                    int quantity = int.Parse(coinInfo[1]);
                    decimal avgPrice = decimal.Parse(coinInfo[2]);

                    var ticker = await GetCurrentPrice(market);

                    callback.UpdateTradeInfo(
                        market,
                        MarketNames[market],
                        quantity,
                        avgPrice,
                        ticker.trade_price
                    );
                }
            }
            catch (Exception ex)
            {
                callback.NotifyError($"거래 정보 조회 실패: {ex.Message}");
            }
        }
    }
    }
    #endregion
}