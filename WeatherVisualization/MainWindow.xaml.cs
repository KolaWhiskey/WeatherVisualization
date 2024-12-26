using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WeatherVisualization
{
    public partial class MainWindow : Window
    {
        private readonly Random _random = new Random();
        private readonly List<double> _barometerReadings = new List<double>();
        private readonly List<double> _anemometerReadings = new List<double>();
        private DispatcherTimer _timer = new DispatcherTimer();
        private DateTime _timerNow = new DateTime();

        public MainWindow()
        {
            InitializeComponent();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += UpdateReadings;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, args) => UpdateData();
            StartClockUpdate();
            _timer.Start(); // Запускаем таймер
        }


        private void UpdateData()
        {
            Random random = new Random();

            // Генерация случайных данных в заданных интервалах
            var barometerRange = ParseRange(BarometerRange.Text, 950, 1050);
            var anemometerRange = ParseRange(AnemometerRange.Text, 0, 30);

            _barometerReadings.Add(random.Next(barometerRange.min, barometerRange.max + 1));
            _anemometerReadings.Add(random.Next(anemometerRange.min, anemometerRange.max + 1));

            // Ограничение на 10 точек
            if (_barometerReadings.Count > 10) _barometerReadings.RemoveAt(0);
            if (_anemometerReadings.Count > 10) _anemometerReadings.RemoveAt(0);

            // Обновляем график
            DrawGraph();

            // Обновляем текстовые блоки
            CurrentReadings.Text = $"Барометр: {_barometerReadings.Last()}, Анемометр: {_anemometerReadings.Last()}";
            AverageReadings.Text = $"Барометр: {_barometerReadings.Average():F2}, Анемометр: {_anemometerReadings.Average():F2}";
        }

        private (int min, int max) ParseRange(string input, int defaultMin, int defaultMax)
        {
            var parts = input.Split('-');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int min) &&
                int.TryParse(parts[1], out int max))
            {
                return (min, max);
            }
            return (defaultMin, defaultMax);
        }




        private void UpdateReadings(object sender, EventArgs e)
        {
            if (!TryParseRange(BarometerRange.Text, out double barMin, out double barMax) ||
                !TryParseRange(AnemometerRange.Text, out double aneMin, out double aneMax))
            {
                _timer.Stop();
                return;
            }

            double barometer = _random.NextDouble() * (barMax - barMin) + barMin;
            double anemometer = _random.NextDouble() * (aneMax - aneMin) + aneMin;

            _barometerReadings.Add(barometer);
            _anemometerReadings.Add(anemometer);

            DrawGraph();

            CurrentReadings.Text = $"Барометр: {barometer:F2}, Анемометр: {anemometer:F2}";
            AverageReadings.Text = $"Барометр: {_barometerReadings.Average():F2}, Анемометр: {_anemometerReadings.Average():F2}, Время: {_timerNow.TimeOfDay.ToString()}";
        }

        private void DrawGraph()
        {
            GraphCanvas.Children.Clear();

            if (_barometerReadings.Count < 2 || _anemometerReadings.Count < 2) return;

            double canvasWidth = GraphCanvas.ActualWidth;
            double canvasHeight = GraphCanvas.ActualHeight;

            // Виртуальное деление канваса
            double barometerHeight = canvasHeight * 0.5; // Верхняя половина
            double anemometerHeight = canvasHeight * 0.5; // Нижняя половина

            double xSpacing = canvasWidth / (_barometerReadings.Count - 1);

            Polyline barometerLine = new Polyline
            {
                Stroke = Brushes.LightBlue,
                StrokeThickness = 2
            };

            Polyline anemometerLine = new Polyline
            {
                Stroke = Brushes.LightGreen,
                StrokeThickness = 2
            };

            for (int i = 0; i < _barometerReadings.Count; i++)
            {
                // Барометр
                double barY = barometerHeight - (_barometerReadings[i] - _barometerReadings.Min()) /
                              (_barometerReadings.Max() - _barometerReadings.Min()) * barometerHeight;

                // Анемометр
                double aneY = canvasHeight - (_anemometerReadings[i] - _anemometerReadings.Min()) /
                              (_anemometerReadings.Max() - _anemometerReadings.Min()) * anemometerHeight;

                // Добавление точек
                barometerLine.Points.Add(new Point(i * xSpacing, barY));
                anemometerLine.Points.Add(new Point(i * xSpacing, aneY));
            }

            // Добавление линий на Canvas
            GraphCanvas.Children.Add(barometerLine);
            GraphCanvas.Children.Add(anemometerLine);
        }


        private bool TryParseRange(string text, out double min, out double max)
        {
            min = max = 0;

            string[] parts = text.Split('-');
            if (parts.Length != 2 || !double.TryParse(parts[0], out min) || !double.TryParse(parts[1], out max) || min >= max)
            {
                return false;
            }

            return true;
        }

        private void StartClockUpdate()
        {
            DispatcherTimer clockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            clockTimer.Tick += (sender, args) =>
            {
                CurrentTimeTextBlock.Text = DateTime.Now.ToString("HH:mm:ss");
            };
            clockTimer.Start();
        }

        private void EndClockUpdate() // 
        {
            DispatcherTimer clockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            clockTimer.Tick -= (sender, args) =>
            {
                CurrentTimeTextBlock.Text = DateTime.Now.ToString("HH:mm:ss");
            };
            clockTimer.Stop();
        }

        private void EndButton_Click(object sender, RoutedEventArgs e) // очистка
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick -= (s, args) => UpdateData();
            EndClockUpdate();
            GraphCanvas.Children.Clear();
            _anemometerReadings.Clear();
            _barometerReadings.Clear();
            _timer.Stop(); // стоп таймер
        }

        private void PauseTime_Click(object sender, RoutedEventArgs e) // пауза
        {
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick -= (s, args) => UpdateData();
            EndClockUpdate();
            _timer.Stop(); // стоп таймер
        }






    }
}
