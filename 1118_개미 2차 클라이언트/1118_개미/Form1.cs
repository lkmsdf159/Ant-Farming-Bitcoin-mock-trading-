using System;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Newtonsoft.Json;
using _1118_개미.ServiceReference1;
using System.Threading;
using System.Collections.Generic;
using System.ServiceModel;

namespace _1118_개미
{
    public partial class Form1 : Form, ITradeServiceCallback
    {
        // 서비스 클라이언트 객체 선언
        private readonly TradeServiceClient tradeService;

        // UI 요소들 선언
        private readonly System.Windows.Forms.Timer timer;  // 데이터를 주기적으로 업데이트하는 타이머
        private readonly Label currentPriceLabel;  // 현재 가격을 표시하는 라벨
        private readonly ChartManager chartManager;  // 차트를 업데이트하는 클래스
        private string currentMarket = "KRW-BTC";  // 기본 마켓
        private bool isUpdating = false;  // 데이터 업데이트 중인지 확인하는 플래그

        public Form1()
        {
            InitializeComponent();

            var context = new InstanceContext(this);

            // 필드들을 생성자에서 초기화합니다.
            tradeService = new TradeServiceClient(context);

            // UI와 관련된 필드를 생성자에서 초기화
            currentPriceLabel = new Label
            {
                AutoSize = true,
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(10, 10),
                BackColor = Color.Transparent
            };
            chartManager = new ChartManager(chart1);  // ChartManager 초기화
            timer = new System.Windows.Forms.Timer { Interval = 5000 };  // 타이머 초기화

            InitializeUI();
            InitializeTimer();
            _ = UpdateDataAsync();  // 초기 데이터 업데이트 실행
        }

        // UI 초기화 메서드
        private void InitializeUI()
        {
            InitializeListView();  // 리스트뷰 초기화
            InitializeLabel();  // 가격 라벨 초기화
            InitializeComboBox();  // 마켓 콤보박스 초기화
            textBox1.Text = "1";  // 기본 수량을 1로 설정
        }

        // 가격 라벨을 폼에 추가하는 메서드
        private void InitializeLabel()
        {
            Controls.Add(currentPriceLabel);
            currentPriceLabel.BringToFront();  // 가격 라벨을 맨 앞에 배치
        }

        // 마켓 선택 콤보박스를 초기화하는 메서드
        private void InitializeComboBox()
        {
            comboBox1.Items.Clear();  // 기존 항목을 지움
            var marketNames = tradeService.GetMarketNames();  // 마켓 이름 목록 가져오기
            foreach (var market in marketNames)
            {
                comboBox1.Items.Add($"{market.Key} - {market.Value}");  // 콤보박스에 마켓 추가
            }
            comboBox1.SelectedIndex = 0;  // 기본값으로 첫 번째 마켓 선택
        }

        // 타이머 초기화 및 이벤트 핸들러 연결
        private void InitializeTimer()
        {
            timer.Tick += new EventHandler(Timer_Tick);  // 타이머 틱 이벤트 연결
            timer.Start();  // 타이머 시작
        }

        // 타이머 틱 이벤트 핸들러 (5초마다 데이터 업데이트)
        private async void Timer_Tick(object sender, EventArgs e)
        {
            await UpdateDataAsync();  // 데이터를 주기적으로 업데이트
        }

        // 리스트뷰 초기화 메서드
        private void InitializeListView()
        {
            listView1.View = View.Details;
            listView1.GridLines = true;
            listView1.FullRowSelect = true;

            // 리스트뷰의 컬럼 설정
            listView1.Columns.Clear();
            listView1.Columns.Add("종목", 100);
            listView1.Columns.Add("보유수량", 100);
            listView1.Columns.Add("평균매수가", 100);
            listView1.Columns.Add("평가손익", 100);
            listView1.Columns.Add("평가금액", 100);
        }

        // 데이터를 비동기적으로 업데이트하는 메서드
        private async Task UpdateDataAsync()
        {
            if (isUpdating) return;

            try
            {
                isUpdating = true;

                var ticker = await tradeService.GetCurrentPriceAsync(currentMarket);
                var candles = await tradeService.GetCandlesAsync(currentMarket);

                // 여기만 수정
                listView1.Items.Clear();
                tradeService.TradeInfosCallback("user");  // 콜백으로 변경

                var totalAssets = await tradeService.GetTotalAssetsAsync("user");

                UpdateCurrentPrice(ticker);
                chartManager.UpdateChart(candles);
                UpdateAssetInfo(totalAssets);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"데이터 로드 실패: {ex.Message}");
            }
            finally
            {
                isUpdating = false;
            }
        }

        public void UpdateTradeInfo(string market, string marketName, decimal totalQuantity, decimal averagePrice, decimal currentPrice)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateTradeInfo(market, marketName, totalQuantity, averagePrice, currentPrice)));
                return;
            }

            var profit = (currentPrice - averagePrice) * totalQuantity;
            var evaluation = currentPrice * totalQuantity;

            var item = new ListViewItem(new string[] {
            marketName,
            $"{totalQuantity:N4}",
            $"{averagePrice:N0}",
            $"{profit:N0}",
            $"{evaluation:N0}"
        })
            {
                ForeColor = profit >= 0 ? Color.Red : Color.Blue
            };

            listView1.Items.Add(item);
        }

        public void NotifyError(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => NotifyError(message)));
                return;
            }
            MessageBox.Show(message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // 총 자산 정보를 업데이트하는 메서드
        private void UpdateAssetInfo(decimal totalAssets)
        {
            if (InvokeRequired)
            {
                // UI 스레드에서 실행되도록 Invoke 호출
                Invoke(new Action(() => UpdateAssetInfo(totalAssets)));
                return;
            }
            label3.Text = $"총 자산: {totalAssets:N0} KRW";  // 자산 정보를 라벨에 업데이트
        }

        // 리스트뷰에 거래 기록을 업데이트하는 메서드
        private async Task UpdateListView(IEnumerable<TradeRecord> records)
        {
            if (InvokeRequired)
            {
                // 백그라운드 스레드에서 UI 업데이트 작업을 실행
                await Task.Run(() =>
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        UpdateListViewUI(records);
                    }));
                });
            }
            else
            {
                // InvokeRequired가 필요 없으면 바로 UI 업데이트
                UpdateListViewUI(records);
            }
        }

        // 리스트뷰 UI를 업데이트하는 메서드
        private void UpdateListViewUI(IEnumerable<TradeRecord> records)
        {
            listView1.Items.Clear();  // 기존 리스트뷰 아이템 지우기

            if (records == null || !records.Any())
            {
                // 거래 기록이 없으면 기본값으로 표시
                listView1.Items.Add(new ListViewItem(new[] { "-", "-", "-", "-", "-" }));
                return;
            }

            var marketNames = tradeService.GetMarketNames();  // 마켓 이름 가져오기
            foreach (var group in records.GroupBy(r => r.Market))
            {
                var market = group.Key;
                var currentPrice = tradeService.GetCurrentPrice(market);  // 현재 가격 조회

                // 총 수량, 평균 매수 가격, 평가 손익, 평가 금액 계산
                var totalQuantity = group.Sum(r => r.Quantity);
                var averagePrice = group.Sum(r => r.Quantity * r.BuyPrice) / totalQuantity;
                var profit = (currentPrice.trade_price - averagePrice) * totalQuantity;
                var evaluation = currentPrice.trade_price * totalQuantity;

                // 리스트뷰 항목 생성
                var item = new ListViewItem(new string[] {
                    marketNames[market],
                    $"{totalQuantity:N4}",
                    $"{averagePrice:N0}",
                    $"{profit:N0}",
                    $"{evaluation:N0}"
                })
                {
                    ForeColor = profit >= 0 ? Color.Red : Color.Blue  // 평가 손익에 따라 색상 변경 (플러스면 빨간색, 마이너스면 파란색)
                };

                listView1.Items.Add(item);  // 리스트뷰에 항목 추가
            }
        }

        // 현재 가격을 업데이트하는 메서드
        private void UpdateCurrentPrice(UpbitTicker ticker)
        {
            if (InvokeRequired)
            {
                // UI 스레드에서 실행되도록 Invoke 호출
                Invoke(new MethodInvoker(delegate { UpdateCurrentPrice(ticker); }));
                return;
            }

            var priceChange = ticker.change_price;
            var changePercent = ticker.change_rate * 100;
            var sign = priceChange >= 0 ? "+" : "";

            var marketNames = tradeService.GetMarketNames();
            var marketName = marketNames[currentMarket];

            // 현재 가격 라벨에 업데이트
            currentPriceLabel.Text = $"{marketName}: {ticker.trade_price:N0} ({sign}{changePercent:F2}%)";
            currentPriceLabel.ForeColor = priceChange >= 0 ? Color.Red : Color.Blue;  // 상승/하락에 따른 색상

            UpdateTotalPrice();  // 총 금액 업데이트
        }

        // 현재 가격을 파싱하는 메서드
        private decimal ParseCurrentPrice()
        {
            var priceText = currentPriceLabel.Text.Split(':')[1].Trim().Split(' ')[0];
            return decimal.Parse(priceText);  // 현재 가격 반환
        }

        // 총 금액을 계산하여 업데이트하는 메서드
        private void UpdateTotalPrice()
        {
            if (decimal.TryParse(textBox1.Text, out decimal quantity))
            {
                var currentPrice = ParseCurrentPrice();
                label2.Text = $"가격: {(quantity * currentPrice):N0} KRW";  // 총 금액을 라벨에 업데이트
            }
        }

        #region 이벤트 핸들러

        // 마켓 선택 시 호출되는 이벤트 핸들러
        private async void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedItem = comboBox1.SelectedItem.ToString();
            string marketCode = selectedItem.Split(' ')[0];

            currentMarket = marketCode;  // 선택한 마켓으로 현재 마켓 설정
            await UpdateDataAsync();  // 데이터 업데이트
        }

        // 매수 버튼 클릭 시 호출되는 이벤트 핸들러
        private async void button1_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(textBox1.Text, out decimal quantity) || quantity <= 0)
            {
                MessageBox.Show("올바른 수량을 입력하세요.");  // 수량이 유효하지 않으면 경고
                return;
            }

            try
            {
                var currentPrice = ParseCurrentPrice();  // 현재 가격 파싱
                bool success = await tradeService.BuyAsync("user", currentMarket, currentPrice, quantity);

                if (success)
                {
                    MessageBox.Show($"매수 완료\n수량: {quantity}\n매수금액: {(quantity * currentPrice):N0} KRW");
                    await UpdateDataAsync();  // 데이터 업데이트
                }
                else
                {
                    MessageBox.Show("매수 실패: 잔액 부족");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"매수 실패: {ex.Message}");  // 예외 처리
            }
        }

        // 매도 버튼 클릭 시 호출되는 이벤트 핸들러
        private async void button2_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(textBox1.Text, out decimal quantity) || quantity <= 0)
            {
                MessageBox.Show("올바른 수량을 입력하세요.");
                return;
            }

            try
            {
                bool success = await tradeService.SellAsync("user", currentMarket, quantity);  // 매도

                if (success)
                {
                    MessageBox.Show($"매도 완료\n수량: {quantity}");
                    await UpdateDataAsync();  // 데이터 업데이트
                }
                else
                {
                    MessageBox.Show("매도 실패: 보유 수량 부족");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"매도 실패: {ex.Message}");  // 예외 처리
            }
        }

        // 수량 +1 버튼 클릭 시 호출되는 이벤트 핸들러
        private void button3_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(textBox1.Text, out decimal current))
            {
                textBox1.Text = (current + 1).ToString();  // 수량 1 증가
                UpdateTotalPrice();  // 총 금액 업데이트
            }
        }

        // 수량 -1 버튼 클릭 시 호출되는 이벤트 핸들러
        private void button4_Click(object sender, EventArgs e)
        {
            if (decimal.TryParse(textBox1.Text, out decimal current) && current > 1)
            {
                textBox1.Text = (current - 1).ToString();  // 수량 1 감소
                UpdateTotalPrice();  // 총 금액 업데이트
            }
        }

        #endregion
    }
}