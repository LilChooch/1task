using System;
using System.Collections.Generic;
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

namespace task4
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Graph graph;
        private const double NodeRadius = 20;
        private Grid movingNode = null;
        private Point mouseOffset;

        public MainWindow()
        {
            InitializeComponent();
            graph = new Graph();
        }

        /*private void GraphCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(GraphCanvas);
            string nodeName = ((char)('A' + graph.Nodes.Count)).ToString();

            if (!graph.Nodes.ContainsKey(nodeName))
            {
                graph.AddNode(nodeName);
                AddNodeToCanvas(nodeName, position);
            }
        }*/
       /* private void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (movingNode != null && e.LeftButton == MouseButtonState.Pressed)
            {
                // Получаем позицию мыши
                Point mousePosition = e.GetPosition(GraphCanvas);

                // Вычисляем новые координаты узла
                double newX = mousePosition.X - mouseOffset.X;
                double newY = mousePosition.Y - mouseOffset.Y;

                // Перемещаем узел
                Canvas.SetLeft(movingNode, newX);
                Canvas.SetTop(movingNode, newY);

                // Обновляем координаты узла в модели графа
                var label = movingNode.Children.OfType<TextBlock>().FirstOrDefault();
                if (label != null && graph.Nodes.ContainsKey(label.Text))
                {
                    graph.Nodes[label.Text].Position = new Point(newX + NodeRadius, newY + NodeRadius);
                }

                // Подсвечиваем узел
                var ellipse = movingNode.Children.OfType<Ellipse>().FirstOrDefault();
                if (ellipse != null)
                {
                    ellipse.Fill = Brushes.Yellow; // Подсветка узла
                }

                // Обновляем связанные рёбра
                var nodeName = label?.Text;
                UpdateEdges(nodeName);
            }
        }*/

        /*private void Node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (movingNode != null)
            {
                // Убираем подсветку с узла
                var ellipse = movingNode.Children.OfType<Ellipse>().FirstOrDefault();
                if (ellipse != null)
                {
                    ellipse.Fill = Brushes.LightBlue; // Возвращаем основной цвет
                }

                // Завершаем перемещение
                movingNode = null;
                GraphCanvas.ReleaseMouseCapture();
            }
        }*/


       /* private void AddNodeToCanvas(string name, Point position)
        {
            // Создаём круг узла
            Ellipse node = new Ellipse
            {
                Width = NodeRadius * 2,
                Height = NodeRadius * 2,
                Fill = Brushes.LightBlue, // Основной цвет узла
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            // Создаём текст с именем узла
            TextBlock label = new TextBlock
            {
                Text = name,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
           
            // Группа для объединения круга и текста
            Grid nodeGroup = new Grid();
            nodeGroup.Children.Add(node);
            nodeGroup.Children.Add(label);

            // Привязываем события мыши для подсветки и перемещения
            nodeGroup.MouseLeftButtonDown += Node_MouseLeftButtonDown;
            nodeGroup.MouseLeftButtonUp += Node_MouseLeftButtonUp;
            nodeGroup.MouseMove += Node_MouseMove;

            // Добавляем узел на Canvas
            GraphCanvas.Children.Add(nodeGroup);
            Canvas.SetLeft(nodeGroup, position.X - NodeRadius);
            Canvas.SetTop(nodeGroup, position.Y - NodeRadius);

            // Добавляем узел в граф
            graph.Nodes[name].Position = position;

           
        }*/
        

        private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid nodeGroup)
            {
                movingNode = nodeGroup;
                Point mousePosition = e.GetPosition(GraphCanvas);
                double nodeX = Canvas.GetLeft(nodeGroup);
                double nodeY = Canvas.GetTop(nodeGroup);

                mouseOffset = new Point(mousePosition.X - nodeX, mousePosition.Y - nodeY);
                GraphCanvas.CaptureMouse();
            }
        }

        private void GraphCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (movingNode != null)
            {
                Point mousePosition = e.GetPosition(GraphCanvas);

                double newX = mousePosition.X - mouseOffset.X;
                double newY = mousePosition.Y - mouseOffset.Y;

                Canvas.SetLeft(movingNode, newX);
                Canvas.SetTop(movingNode, newY);

                var label = movingNode.Children.OfType<TextBlock>().FirstOrDefault();
                if (label != null && graph.Nodes.ContainsKey(label.Text))
                {
                    graph.Nodes[label.Text].Position = new Point(newX + NodeRadius, newY + NodeRadius);
                }

                UpdateEdges(label?.Text);
            }
        }

        private void GraphCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Получаем координаты клика
            Point position = e.GetPosition(GraphCanvas);

            // Генерируем имя узла (по алфавиту)
            string nodeName = ((char)('A' + graph.Nodes.Count)).ToString();

            // Проверяем, существует ли узел с таким именем
            if (!graph.Nodes.ContainsKey(nodeName))
            {
                // Добавляем узел в модель графа
                graph.AddNode(nodeName);

                // Добавляем узел на Canvas
                AddNodeToCanvas(nodeName, position);
            }
        }

        private void AddEdge_Click(object sender, RoutedEventArgs e)
        {
            string start = EdgeStartTextBox.Text.Trim();
            string end = EdgeEndTextBox.Text.Trim();
            if (int.TryParse(EdgeWeightTextBox.Text.Trim(), out int weight))
            {
                if (!graph.Nodes.ContainsKey(start) || !graph.Nodes.ContainsKey(end))
                {
                    MessageBox.Show("Указанные узлы не найдены.");
                    return;
                }

                graph.AddEdge(start, end, weight);
                AddEdgeToCanvas(start, end, weight);

                MessageBox.Show($"Ребро от {start} до {end} с весом {weight} добавлено.");
            }
            else
            {
                MessageBox.Show("Введите корректное значение веса.");
            }
        }
        /*private Tuple<Point, Point> GetEdgePoints(Point start, Point end, double radius)
        {
            // Вычисляем угол между вершинами
            double angle = Math.Atan2(end.Y - start.Y, end.X - start.X);

            // Начальная точка (край первого круга)
            Point startEdge = new Point(
                start.X + radius * Math.Cos(angle),
                start.Y + radius * Math.Sin(angle)
            );

            // Конечная точка (край второго круга)
            Point endEdge = new Point(
                end.X - radius * Math.Cos(angle),
                end.Y - radius * Math.Sin(angle)
            );

            return Tuple.Create(startEdge, endEdge);
        }*/
       /* private void UpdateLine(Line edge, Point startPosition, Point endPosition)
        {
            // Вычисляем новые точки для линии
            var edgePoints = GetEdgePoints(startPosition, endPosition, NodeRadius);

            edge.X1 = edgePoints.Item1.X;
            edge.Y1 = edgePoints.Item1.Y;
            edge.X2 = edgePoints.Item2.X;
            edge.Y2 = edgePoints.Item2.Y;

            // Находим и обновляем текст с весом, если он связан с этой линией
            var weightLabel = GraphCanvas.Children.OfType<TextBlock>()
                .FirstOrDefault(label => Canvas.GetLeft(label) == (edge.X1 + edge.X2) / 2 &&
                                         Canvas.GetTop(label) == (edge.Y1 + edge.Y2) / 2);

            if (weightLabel != null)
            {
                double midX = (edge.X1 + edge.X2) / 2;
                double midY = (edge.Y1 + edge.Y2) / 2;
                Canvas.SetLeft(weightLabel, midX);
                Canvas.SetTop(weightLabel, midY);
            }
        }*/

        private void AddEdgeToCanvas(string start, string end, int weight)
        {
            if (!graph.Nodes.ContainsKey(start) || !graph.Nodes.ContainsKey(end))
                return;

            var startPosition = graph.Nodes[start].Position;
            var endPosition = graph.Nodes[end].Position;

            // Вычисляем координаты начала и конца линии с учётом радиуса узла
            var edgePoints = GetEdgePoints(startPosition, endPosition, NodeRadius);

            Line edge = new Line
            {
                X1 = edgePoints.Item1.X,
                Y1 = edgePoints.Item1.Y,
                X2 = edgePoints.Item2.X,
                Y2 = edgePoints.Item2.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Tag = Tuple.Create(start, end) // Сохраняем имена вершин в теге линии
            };

            // Добавляем линию на холст
            GraphCanvas.Children.Add(edge);

            // Добавляем текст для веса
            TextBlock weightLabel = new TextBlock
            {
                Text = weight.ToString(),
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                Background = Brushes.White
            };

            // Устанавливаем положение текста в середине линии
            double midX = (edge.X1 + edge.X2) / 2;
            double midY = (edge.Y1 + edge.Y2) / 2;
            Canvas.SetLeft(weightLabel, midX);
            Canvas.SetTop(weightLabel, midY);

            GraphCanvas.Children.Add(weightLabel);
        }

        /*private void UpdateEdges(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName) || !graph.Nodes.ContainsKey(nodeName))
                return;

            // Получаем текущую позицию перемещённого узла
            var nodePosition = graph.Nodes[nodeName].Position;

            // Обходим все рёбра на холсте
            foreach (var edge in GraphCanvas.Children.OfType<Line>())
            {
                // Если ребро связано с перемещённым узлом
                if (edge.Tag is Tuple<string, string> edgeNodes)
                {
                    string startNode = edgeNodes.Item1;
                    string endNode = edgeNodes.Item2;

                    if (startNode == nodeName)
                    {
                        // Узел является началом ребра, обновляем начальную точку
                        var endPosition = graph.Nodes[endNode].Position;
                        UpdateLine(edge, nodePosition, endPosition);
                    }
                    else if (endNode == nodeName)
                    {
                        // Узел является концом ребра, обновляем конечную точку
                        var startPosition = graph.Nodes[startNode].Position;
                        UpdateLine(edge, startPosition, nodePosition);
                    }
                }
            }
        }*/

       

        /*private void FindShortestPath_Click(object sender, RoutedEventArgs e)
        {
            string source = SourceNodeTextBox.Text.Trim();
            string target = TargetNodeTextBox.Text.Trim();

            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target) ||
                !graph.Nodes.ContainsKey(source) || !graph.Nodes.ContainsKey(target))
            {
                MessageBox.Show("Укажите корректные узлы.");
                return;
            }

            AlgorithmLog.Clear();
            var path = graph.Dijkstra(source, target, LogStep);
            if (path != null)
            {
                AlgorithmLog.AppendText("Кратчайший путь: " + string.Join(" -> ", path) + Environment.NewLine);
            }
            else
            {
                AlgorithmLog.AppendText("Путь не найден." + Environment.NewLine);
            }
        }*/
        private void AddNode_Click(object sender, RoutedEventArgs e)
        {
            string nodeName = NodeNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(nodeName))
            {
                MessageBox.Show("Введите имя узла.");
                return;
            }

            if (graph.Nodes.ContainsKey(nodeName))
            {
                MessageBox.Show($"Узел с именем {nodeName} уже существует.");
                return;
            }

            Point position = new Point(50 + graph.Nodes.Count * 50, 50 + graph.Nodes.Count * 50); // Автоматическое размещение узлов
            graph.AddNode(nodeName);
            AddNodeToCanvas(nodeName, position);

            MessageBox.Show($"Узел {nodeName} успешно добавлен.");
        }

        private void DeleteNode_Click(object sender, RoutedEventArgs e)
        {
            string nodeName = NodeNameTextBox.Text.Trim();

            if (string.IsNullOrEmpty(nodeName))
            {
                MessageBox.Show("Введите имя удаляемого узла.");
                return;
            }

            if (!graph.Nodes.ContainsKey(nodeName))
            {
                MessageBox.Show($"Узел с именем {nodeName} не найден.");
                return;
            }

            // Удаляем узел из модели графа
            graph.RemoveNode(nodeName);

            // Удаляем узел с холста
            var nodeToRemove = GraphCanvas.Children.OfType<Grid>()
                                .FirstOrDefault(node => node.Children.OfType<TextBlock>().FirstOrDefault()?.Text == nodeName);

            if (nodeToRemove != null)
            {
                GraphCanvas.Children.Remove(nodeToRemove);
            }

            // Удаляем рёбра, связанные с узлом
            var edgesToRemove = GraphCanvas.Children.OfType<Line>()
                                   .Where(line => line.Tag is Tuple<string, string> edgeNodes &&
                                                  (edgeNodes.Item1 == nodeName || edgeNodes.Item2 == nodeName))
                                   .ToList();

            foreach (var edge in edgesToRemove)
            {
                GraphCanvas.Children.Remove(edge);
            }

            MessageBox.Show($"Узел {nodeName} и все связанные рёбра успешно удалены.");
        }

        private void DeleteEdge_Click(object sender, RoutedEventArgs e)
        {
            string startNode = EdgeStartTextBox.Text.Trim();
            string endNode = EdgeEndTextBox.Text.Trim();

            if (string.IsNullOrEmpty(startNode) || string.IsNullOrEmpty(endNode))
            {
                MessageBox.Show("Введите начальный и конечный узлы.");
                return;
            }

            if (!graph.Nodes.ContainsKey(startNode) || !graph.Nodes.ContainsKey(endNode))
            {
                MessageBox.Show("Указанные узлы не найдены.");
                return;
            }

            // Удаляем ребро из модели графа
            graph.RemoveEdge(startNode, endNode);

            // Удаляем ребро с холста
            var edgeToRemove = GraphCanvas.Children.OfType<Line>()
                                .FirstOrDefault(line => line.Tag is Tuple<string, string> edgeNodes &&
                                                        edgeNodes.Item1 == startNode && edgeNodes.Item2 == endNode);

            if (edgeToRemove != null)
            {
                GraphCanvas.Children.Remove(edgeToRemove);
                MessageBox.Show($"Ребро между {startNode} и {endNode} успешно удалено.");
            }
            else
            {
                MessageBox.Show($"Ребро между {startNode} и {endNode} не найдено.");
            }
        }

        private void LogStep(string message)
        {
            AlgorithmLog.AppendText(message + Environment.NewLine);
        }

        private void ClearPath_Click(object sender, RoutedEventArgs e)
        {
            AlgorithmLog.Clear();
        }
        #region Новые методы

        // Метод для подсветки узлов при перемещении
        /*private void HighlightNode(string nodeName, Brush color)
        {
            var nodeGroup = GraphCanvas.Children.OfType<Grid>()
                .FirstOrDefault(grid => grid.Children.OfType<TextBlock>().FirstOrDefault()?.Text == nodeName);

            if (nodeGroup != null)
            {
                var ellipse = nodeGroup.Children.OfType<Ellipse>().FirstOrDefault();
                if (ellipse != null)
                {
                    ellipse.Fill = color;
                }
            }
        }*/

        // Метод для обновления положения линий (рёбер)
        private void UpdateEdges(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName) || !graph.Nodes.ContainsKey(nodeName))
                return;

            // Получаем текущую позицию перемещённого узла
            var nodePosition = graph.Nodes[nodeName].Position;

            // Обходим все рёбра на холсте
            foreach (var edge in GraphCanvas.Children.OfType<Line>())
            {
                // Если ребро связано с перемещённым узлом
                if (edge.Tag is Tuple<string, string> edgeNodes)
                {
                    string startNode = edgeNodes.Item1;
                    string endNode = edgeNodes.Item2;

                    if (startNode == nodeName)
                    {
                        // Узел является началом ребра, обновляем начальную точку
                        var endPosition = graph.Nodes[endNode].Position;
                        UpdateLine(edge, nodePosition, endPosition);
                    }
                    else if (endNode == nodeName)
                    {
                        // Узел является концом ребра, обновляем конечную точку
                        var startPosition = graph.Nodes[startNode].Position;
                        UpdateLine(edge, startPosition, nodePosition);
                    }
                }
            }
        }

        // Метод для обновления линии
        private void UpdateLine(Line edge, Point startPosition, Point endPosition)
        {
            // Вычисляем новые точки для линии
            var edgePoints = GetEdgePoints(startPosition, endPosition, NodeRadius);

            edge.X1 = edgePoints.Item1.X;
            edge.Y1 = edgePoints.Item1.Y;
            edge.X2 = edgePoints.Item2.X;
            edge.Y2 = edgePoints.Item2.Y;

            // Находим и обновляем текст с весом, если он связан с этой линией
            var weightLabel = GraphCanvas.Children.OfType<TextBlock>()
                .FirstOrDefault(label => Canvas.GetLeft(label) == (edge.X1 + edge.X2) / 2 &&
                                         Canvas.GetTop(label) == (edge.Y1 + edge.Y2) / 2);

            if (weightLabel != null)
            {
                double midX = (edge.X1 + edge.X2) / 2;
                double midY = (edge.Y1 + edge.Y2) / 2;
                Canvas.SetLeft(weightLabel, midX);
                Canvas.SetTop(weightLabel, midY);
            }
        }

        // Метод для получения точек начала и конца линии
        private Tuple<Point, Point> GetEdgePoints(Point start, Point end, double radius)
        {
            // Вычисляем угол между вершинами
            double angle = Math.Atan2(end.Y - start.Y, end.X - start.X);

            // Начальная точка (край первого круга)
            Point startEdge = new Point(
                start.X + radius * Math.Cos(angle),
                start.Y + radius * Math.Sin(angle)
            );

            // Конечная точка (край второго круга)
            Point endEdge = new Point(
                end.X - radius * Math.Cos(angle),
                end.Y - radius * Math.Sin(angle)
            );

            return Tuple.Create(startEdge, endEdge);
        }

        #endregion

        #region Методы из вашего кода

        private void GraphCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(GraphCanvas);
            string nodeName = ((char)('A' + graph.Nodes.Count)).ToString();

            if (!graph.Nodes.ContainsKey(nodeName))
            {
                graph.AddNode(nodeName);
                AddNodeToCanvas(nodeName, position);
            }
        }

        private void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (movingNode != null && e.LeftButton == MouseButtonState.Pressed)
            {
                // Получаем позицию мыши
                Point mousePosition = e.GetPosition(GraphCanvas);

                // Вычисляем новые координаты узла
                double newX = mousePosition.X - mouseOffset.X;
                double newY = mousePosition.Y - mouseOffset.Y;

                // Перемещаем узел
                Canvas.SetLeft(movingNode, newX);
                Canvas.SetTop(movingNode, newY);

                // Обновляем координаты узла в модели графа
                var label = movingNode.Children.OfType<TextBlock>().FirstOrDefault();
                if (label != null && graph.Nodes.ContainsKey(label.Text))
                {
                    graph.Nodes[label.Text].Position = new Point(newX + NodeRadius, newY + NodeRadius);
                }

                // Подсвечиваем узел
                var ellipse = movingNode.Children.OfType<Ellipse>().FirstOrDefault();
                if (ellipse != null)
                {
                    ellipse.Fill = Brushes.Yellow; // Подсветка узла
                }

                // Обновляем связанные рёбра
                var nodeName = label?.Text;
                UpdateEdges(nodeName);
            }
        }

        private void Node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (movingNode != null)
            {
                // Убираем подсветку с узла
                var ellipse = movingNode.Children.OfType<Ellipse>().FirstOrDefault();
                if (ellipse != null)
                {
                    ellipse.Fill = Brushes.LightBlue; // Возвращаем основной цвет
                }

                // Завершаем перемещение
                movingNode = null;
                GraphCanvas.ReleaseMouseCapture();
            }
        }

        private void AddNodeToCanvas(string name, Point position)
        {
            // Создаём круг узла
            Ellipse node = new Ellipse
            {
                Width = NodeRadius * 2,
                Height = NodeRadius * 2,
                Fill = Brushes.LightBlue, // Основной цвет узла
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            // Создаём текст с именем узла
            TextBlock label = new TextBlock
            {
                Text = name,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Группа для объединения круга и текста
            Grid nodeGroup = new Grid();
            nodeGroup.Children.Add(node);
            nodeGroup.Children.Add(label);

            // Привязываем события мыши для подсветки и перемещения
            nodeGroup.MouseLeftButtonDown += Node_MouseLeftButtonDown;
            nodeGroup.MouseLeftButtonUp += Node_MouseLeftButtonUp;
            nodeGroup.MouseMove += Node_MouseMove;

            // Добавляем узел на Canvas
            GraphCanvas.Children.Add(nodeGroup);
            Canvas.SetLeft(nodeGroup, position.X - NodeRadius);
            Canvas.SetTop(nodeGroup, position.Y - NodeRadius);

            // Добавляем узел в граф
            graph.Nodes[name].Position = position;
        }

        #endregion
        private async void FindShortestPath_Click(object sender, RoutedEventArgs e)
        {
            string source = SourceNodeTextBox.Text.Trim();
            string target = TargetNodeTextBox.Text.Trim();

            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target) ||
                !graph.Nodes.ContainsKey(source) || !graph.Nodes.ContainsKey(target))
            {
                MessageBox.Show("Укажите корректные узлы.");
                Log("Ошибка: указаны некорректные узлы для поиска кратчайшего пути.");
                return;
            }

            AlgorithmLog.Clear();
            Log($"Начат поиск кратчайшего пути от '{source}' до '{target}'.");

            // Асинхронный вызов алгоритма Дейкстры
            var path = await Task.Run(() => graph.Dijkstra(source, target, (currentNode, neighbor, newDistance) =>
            {
                // Обновление UI на каждом шаге
                Dispatcher.Invoke(() =>
                {
                    // Лог текущего шага
                    Log($"Из вершины '{currentNode}' идём в вершину '{neighbor}' с расстоянием {newDistance}.");

                    // Подсвечиваем текущую вершину
                    HighlightNode(currentNode, Brushes.Orange);

                    // Подсвечиваем соседнюю вершину
                    HighlightNode(neighbor, Brushes.Yellow);
                });

                // Задержка для визуализации
                Task.Delay(1000).Wait();
            }));

            // Если путь найден, показываем его
            if (path != null)
            {
                Log($"Кратчайший путь: {string.Join(" -> ", path)}.");

                // Подсвечиваем весь путь зелёным
                foreach (var node in path)
                {
                    HighlightNode(node, Brushes.Green);
                    await Task.Delay(1000); // Задержка между подсветками
                }
            }
            else
            {
                Log("Кратчайший путь не найден.");
            }

            // Убираем подсветку после завершения
            await Task.Delay(1000);
            ResetNodeColors();
        }

        private void Log(string message)
        {
            AlgorithmLog.AppendText($" {message}{Environment.NewLine}");
            AlgorithmLog.ScrollToEnd();
        }

        // Подсветка узла определённым цветом
        private void HighlightNode(string nodeName, Brush color)
        {
            var nodeGroup = GraphCanvas.Children.OfType<Grid>()
                .FirstOrDefault(grid => grid.Children.OfType<TextBlock>().FirstOrDefault()?.Text == nodeName);

            if (nodeGroup != null)
            {
                var ellipse = nodeGroup.Children.OfType<Ellipse>().FirstOrDefault();
                if (ellipse != null)
                {
                    ellipse.Fill = color;
                }
            }
        }

        // Сброс цвета всех узлов
        private void ResetNodeColors()
        {
            foreach (var nodeGroup in GraphCanvas.Children.OfType<Grid>())
            {
                var ellipse = nodeGroup.Children.OfType<Ellipse>().FirstOrDefault();
                if (ellipse != null)
                {
                    ellipse.Fill = Brushes.LightBlue;
                }
            }
        }
    }
}
    

