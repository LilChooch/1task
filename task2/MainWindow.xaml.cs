using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace task2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TransportNetwork network; // Модель транспортной сети
        private const double NodeRadius = 20; // Радиус узлов
        private UIElement movingNode = null; // Текущий перемещаемый узел
        private Point mouseOffset;

        public MainWindow()
        {
            InitializeComponent();
            network = new TransportNetwork(); // Инициализация модели
            TransportCanvas.MouseMove += TransportCanvas_MouseMove;
            TransportCanvas.MouseRightButtonUp += TransportCanvas_MouseRightButtonUp;
        }
        private void Node_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Grid nodeGroup)
            {
                movingNode = nodeGroup; // Запоминаем перемещаемый узел
                Point mousePosition = e.GetPosition(TransportCanvas);
                double nodeX = Canvas.GetLeft(nodeGroup);
                double nodeY = Canvas.GetTop(nodeGroup);

                // Вычисляем смещение мыши относительно узла
                mouseOffset = new Point(mousePosition.X - nodeX, mousePosition.Y - nodeY);

                // Включаем режим захвата мыши для Canvas
                TransportCanvas.CaptureMouse();
            }
        }
        private void TransportCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (movingNode is Grid nodeGroup)
            {
                Point mousePosition = e.GetPosition(TransportCanvas);

                // Вычисляем новые координаты узла
                double newX = mousePosition.X - mouseOffset.X;
                double newY = mousePosition.Y - mouseOffset.Y;

                // Перемещаем узел
                Canvas.SetLeft(nodeGroup, newX);
                Canvas.SetTop(nodeGroup, newY);

                // Обновляем координаты узла в модели
                var label = nodeGroup.Children.OfType<TextBlock>().FirstOrDefault();
                if (label != null && network.Nodes.ContainsKey(label.Text))
                {
                    network.Nodes[label.Text].Position = new Point(newX + NodeRadius, newY + NodeRadius);
                }

                // Обновляем линии (рёбра), связанные с этим узлом
                UpdateEdges(label?.Text);
            }
        }
        private void TransportCanvas_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (movingNode != null)
            {
                movingNode = null; // Сбрасываем перемещаемый узел
                TransportCanvas.ReleaseMouseCapture(); // Отпускаем мышь
            }
        }
        

        private void UpdateEdges(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName)) return;

            // Обходим все линии на холсте
            foreach (var edge in TransportCanvas.Children.OfType<Line>())
            {
                // Проверяем начало и конец линии
                string startNode = edge.Tag as string; // Имя начального узла
                string endNode = edge.Tag as string;   // Имя конечного узла

                if (startNode == nodeName || endNode == nodeName)
                {
                    var startPosition = network.Nodes[startNode].Position;
                    var endPosition = network.Nodes[endNode].Position;

                    // Обновляем координаты начала и конца линии
                    double angle = Math.Atan2(endPosition.Y - startPosition.Y, endPosition.X - startPosition.X);

                    edge.X1 = startPosition.X + NodeRadius * Math.Cos(angle);
                    edge.Y1 = startPosition.Y + NodeRadius * Math.Sin(angle);
                    edge.X2 = endPosition.X - NodeRadius * Math.Cos(angle);
                    edge.Y2 = endPosition.Y - NodeRadius * Math.Sin(angle);
                }
            }
        }

        #region Узлы
        private void AddNode_Click(object sender, RoutedEventArgs e)
        {
            string nodeName = NodeNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(nodeName))
            {
                MessageBox.Show("Введите имя узла.");
                return;
            }

            if (network.Nodes.ContainsKey(nodeName))
            {
                MessageBox.Show("Узел с таким именем уже существует.");
                return;
            }

            network.AddNode(nodeName);
            AddNodeToCanvas(nodeName, new Point(100, 100));
            NodeNameTextBox.Clear();
        }

        private void DeleteNode_Click(object sender, RoutedEventArgs e)
        {
            string nodeName = NodeNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(nodeName))
            {
                MessageBox.Show("Введите имя узла.");
                return;
            }

            if (!network.Nodes.ContainsKey(nodeName))
            {
                MessageBox.Show("Узел не найден.");
                return;
            }

            network.RemoveNode(nodeName);
            RemoveNodeFromCanvas(nodeName);
            NodeNameTextBox.Clear();
        }
        private void TransportCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Получаем координаты клика
            Point position = e.GetPosition(TransportCanvas);

            // Создаём уникальное имя для нового узла
            string nodeName = ((char)('A' + network.Nodes.Count)).ToString();

            // Проверяем, существует ли узел с таким именем
            if (!network.Nodes.ContainsKey(nodeName))
            {
                // Добавляем узел в модель и на холст
                network.AddNode(nodeName);
                AddNodeToCanvas(nodeName, position);
            }
        }
        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            AlgorithmLog.Clear(); // Очищаем поле логов
        }
        private void FindMaxFlow_Click(object sender, RoutedEventArgs e)
        {
            string source = SourceNodeTextBox.Text.Trim();
            string sink = SinkNodeTextBox.Text.Trim();

            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(sink))
            {
                MessageBox.Show("Укажите источник и сток.");
                return;
            }

            if (!network.Nodes.ContainsKey(source) || !network.Nodes.ContainsKey(sink))
            {
                MessageBox.Show("Указанные узлы не найдены.");
                return;
            }

            AlgorithmLog.Clear();
            int maxFlow = network.FindMaxFlow(source, sink, UpdateLog);
            MessageBox.Show($"Максимальный поток: {maxFlow}");
        }

        private void AddNodeToCanvas(string name, Point position)
        {
            Ellipse node = new Ellipse
            {
                Width = NodeRadius * 2,
                Height = NodeRadius * 2,
                Fill = Brushes.LightBlue,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            TextBlock label = new TextBlock
            {
                Text = name,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            Grid nodeGroup = new Grid();
            nodeGroup.Children.Add(node);
            nodeGroup.Children.Add(label);

            TransportCanvas.Children.Add(nodeGroup);
            Canvas.SetLeft(nodeGroup, position.X - NodeRadius);
            Canvas.SetTop(nodeGroup, position.Y - NodeRadius);

            network.Nodes[name].Position = position;
        }

        private void RemoveNodeFromCanvas(string name)
        {
            var element = TransportCanvas.Children
                .OfType<Grid>()
                .FirstOrDefault(e =>
                    e.Children.OfType<TextBlock>().FirstOrDefault()?.Text == name);

            if (element != null)
            {
                TransportCanvas.Children.Remove(element);
            }
        }
        #endregion

        #region Рёбра
        private void AddEdge_Click(object sender, RoutedEventArgs e)
        {
            string start = EdgeStartTextBox.Text.Trim();
            string end = EdgeEndTextBox.Text.Trim();
            int capacity;

            if (!int.TryParse(EdgeCapacityTextBox.Text.Trim(), out capacity) || capacity <= 0)
            {
                MessageBox.Show("Введите корректное значение пропускной способности.");
                return;
            }

            if (!network.Nodes.ContainsKey(start) || !network.Nodes.ContainsKey(end))
            {
                MessageBox.Show("Один из узлов не существует.");
                return;
            }

            network.AddEdge(start, end, capacity);
            AddEdgeToCanvas(start, end, capacity);
            EdgeStartTextBox.Clear();
            EdgeEndTextBox.Clear();
            EdgeCapacityTextBox.Clear();
        }

        private void DeleteEdge_Click(object sender, RoutedEventArgs e)
        {
            string start = EdgeStartTextBox.Text.Trim();
            string end = EdgeEndTextBox.Text.Trim();

            if (!network.Edges.Any(edge => edge.Start == start && edge.End == end))
            {
                MessageBox.Show("Ребро не найдено.");
                return;
            }

            network.RemoveEdge(start, end);
            RemoveEdgeFromCanvas(start, end);
        }

        private void AddEdgeToCanvas(string start, string end, int capacity)
        {
            if (!network.Nodes.ContainsKey(start) || !network.Nodes.ContainsKey(end))
                return;

            // Координаты начального и конечного узлов
            var startPosition = network.Nodes[start].Position;
            var endPosition = network.Nodes[end].Position;

            // Вычисляем угол между узлами
            double angle = Math.Atan2(endPosition.Y - startPosition.Y, endPosition.X - startPosition.X);

            // Начальная и конечная точки линии с учётом радиуса круга
            Point startEdge = new Point(
                startPosition.X + NodeRadius * Math.Cos(angle),
                startPosition.Y + NodeRadius * Math.Sin(angle));

            Point endEdge = new Point(
                endPosition.X - NodeRadius * Math.Cos(angle),
                endPosition.Y - NodeRadius * Math.Sin(angle));

            // Создаём линию для связи
            Line edge = new Line
            {
                X1 = startEdge.X,
                Y1 = startEdge.Y,
                X2 = endEdge.X,
                Y2 = endEdge.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            // Добавляем линию на холст
            TransportCanvas.Children.Add(edge);

            // Отображаем текст с пропускной способностью на линии
            TextBlock capacityLabel = new TextBlock
            {
                Text = capacity.ToString(),
                Foreground = Brushes.Red,
                FontWeight = FontWeights.Bold
            };

            // Добавляем текст рядом с серединой линии
            TransportCanvas.Children.Add(capacityLabel);
            Canvas.SetLeft(capacityLabel, (edge.X1 + edge.X2) / 2);
            Canvas.SetTop(capacityLabel, (edge.Y1 + edge.Y2) / 2);
        }

        private void RemoveEdgeFromCanvas(string start, string end)
        {
            // Удаление рёбер с Canvas
        }
        #endregion

        #region Алгоритм
        private void RunAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            string source = SourceNodeTextBox.Text.Trim();
            string sink = SinkNodeTextBox.Text.Trim();

            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(sink))
            {
                MessageBox.Show("Укажите источник и сток.");
                return;
            }

            if (!network.Nodes.ContainsKey(source) || !network.Nodes.ContainsKey(sink))
            {
                MessageBox.Show("Указанные узлы не найдены.");
                return;
            }

            Action<string> logAction = UpdateLog;
            int maxFlow = network.FindMaxFlow(source, sink, logAction);
            MessageBox.Show($"Максимальный поток: {maxFlow}");
        }

        private void UpdateLog(string message)
        {
            AlgorithmLog.AppendText(message + Environment.NewLine);
        }
        #endregion

        #region Сохранение/загрузка
        private void SaveNetwork_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON Files|*.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(network));
            }
        }

        private void LoadNetwork_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files|*.json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                network = JsonConvert.DeserializeObject<TransportNetwork>(File.ReadAllText(openFileDialog.FileName));
                // Перерисовка Canvas
            }
        }
        #endregion
    }
}

