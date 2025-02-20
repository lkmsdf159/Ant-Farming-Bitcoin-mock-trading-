using _1118_개미.ServiceReference1;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace _1118_개미
{
    // 차트 관리를 위한 클래스
    public class ChartManager
    {
        private readonly Chart _chart;  // 차트 객체
        private readonly ChartArea _chartArea;  // 차트 영역

        // 생성자 - 차트 객체를 받아 초기화
        public ChartManager(Chart chart)
        {
            _chart = chart;
            InitializeChart();  // 차트 초기화
            _chartArea = _chart.ChartAreas[0];  // 첫 번째 차트 영역 참조
        }

        // 차트를 초기화하는 메서드
        private void InitializeChart()
        {
            _chart.ChartAreas.Clear();  // 기존 차트 영역 삭제
            _chart.Series.Clear();  // 기존 시리즈 삭제
            _chart.Legends.Clear();  // 기존 범례 삭제

            var area = new ChartArea("Default");  // 새로운 차트 영역 생성
            ConfigureChartArea(area);  // 차트 영역 설정
            _chart.ChartAreas.Add(area);  // 차트에 영역 추가

            // "Price"라는 이름의 시리즈 생성
            var series = new Series("Price")
            {
                ChartType = SeriesChartType.Candlestick,  // 캔들스틱 차트 타입 설정
                ChartArea = "Default",  // 차트 영역 설정
                YValuesPerPoint = 4,  // 각 포인트에 대해 Y 값 4개 사용 (시작가, 최고가, 최저가, 종가)
                Color = Color.Blue  // 기본 색상 설정
            };
            _chart.Series.Add(series);  // 차트에 시리즈 추가
        }

        // 차트 영역의 속성을 설정하는 메서드
        private void ConfigureChartArea(ChartArea area)
        {
            // X축 설정
            area.AxisX.LabelStyle.Format = "HH:mm";  // 시간 형식으로 표시
            area.AxisX.IntervalType = DateTimeIntervalType.Hours;  // X축 간격을 시간 단위로 설정
            area.AxisX.Interval = 1;  // 1시간 간격
            area.AxisX.MajorGrid.LineColor = Color.LightGray;  // X축 그리드 라인의 색상
            area.AxisX.LabelStyle.Angle = 0;  // X축 라벨의 각도 설정

            // Y축 설정
            area.AxisY.LabelStyle.Format = "N0";  // Y축 숫자 형식 설정 (천 단위로 구분)
            area.AxisY.MajorGrid.LineColor = Color.LightGray;  // Y축 그리드 라인의 색상

            // 차트 영역 설정
            area.BackColor = Color.White;  // 차트 배경색
            area.BorderColor = Color.LightGray;  // 차트 영역 테두리 색상
            area.BorderWidth = 1;  // 차트 영역 테두리 두께
        }

        // 차트를 업데이트하는 메서드
        public void UpdateChart(UpbitCandle[] candles)
        {
            _chart.Series[0].Points.Clear();  // 기존의 데이터를 지우고 새로운 데이터를 추가할 준비

            // 캔들 데이터를 차트에 추가 (역순으로 추가하여 최신 데이터가 우측에 위치)
            foreach (var candle in candles.Reverse())
            {
                var point = _chart.Series[0].Points.Add();  // 새로운 데이터 포인트 추가
                point.XValue = candle.candle_date_time_kst.ToOADate();  // X축 값 (시간)

                // Y축 값 (시작가, 최고가, 최저가, 종가)
                point.YValues = new double[]
                {
                    (double)candle.opening_price,  // 시작가
                    (double)candle.high_price,     // 최고가
                    (double)candle.low_price,      // 최저가
                    (double)candle.trade_price    // 종가
                };

                SetCandleColor(point, candle);  // 캔들의 색상 설정
            }

            // Y축의 범위 및 X축의 범위를 업데이트
            UpdateAxisRanges(candles);
        }

        // 캔들의 색상을 설정하는 메서드
        private void SetCandleColor(DataPoint point, UpbitCandle candle)
        {
            // 종가가 시작가보다 높으면 상승, 낮으면 하락, 같으면 변동 없음
            Color color = candle.trade_price > candle.opening_price ? Color.Red :
                          candle.trade_price < candle.opening_price ? Color.Blue :
                          Color.Gray;

            // 캔들의 색상 및 테두리 색상 설정
            point.Color = color;
            point.BorderColor = color;  // 테두리 색상
            point.BackSecondaryColor = color;  // 배경 색상 (그라데이션 효과)
        }

        // 차트의 X축과 Y축 범위를 업데이트하는 메서드
        private void UpdateAxisRanges(UpbitCandle[] candles)
        {
            // 캔들의 가격 데이터를 가져와서 Y축의 범위를 설정
            var prices = candles.SelectMany(c => new[]
            {
                c.opening_price,
                c.high_price,
                c.low_price,
                c.trade_price
            });

            var minPrice = prices.Min();  // 최저 가격
            var maxPrice = prices.Max();  // 최고 가격
            var margin = (maxPrice - minPrice) * 0.1m;  // 가격 범위의 10%를 마진으로 설정

            // Y축의 최소값과 최대값 설정 (마진을 추가하여 범위 설정)
            _chartArea.AxisY.Minimum = (double)(minPrice - margin);
            _chartArea.AxisY.Maximum = (double)(maxPrice + margin);

            // X축의 범위 계산 (시간 범위)
            var minX = candles.Min(c => c.candle_date_time_kst).ToOADate();  // 가장 오래된 시간
            var maxX = candles.Max(c => c.candle_date_time_kst).ToOADate();  // 가장 최신 시간

            _chartArea.AxisX.Minimum = minX;  // X축의 최소값
            _chartArea.AxisX.Maximum = maxX;  // X축의 최대값
        }
    }
}