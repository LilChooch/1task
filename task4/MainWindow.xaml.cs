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
       
            private const double NodeRadius = 20;
            private Graph graph;
            private Grid movingNode = null;
            private Point mouseOffset;

            public MainWindow()
            {
                InitializeComponent();
                graph = new Graph();
            }

            // Добавление узла
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
                    MessageBox.Show("Узел с таким именем уже существует.");
                    return;
                }

                graph.AddNode(nodeName);
                AddNodeToCanvas(nodeName, new Point(100, 100));
                NodeNameTextBox.Clear();
            }

            // Удаление узла
            private void DeleteNode_Click(object sender, RoutedEventArgs e)
            {
                string nodeName = NodeNameTextBox.Text.Trim();
                if (string.IsNullOrEmpty(nodeName) || !graph.Nodes.ContainsKey(nodeName))
                {
                    MessageBox.Show("Узел не найден.");
                    return;
                }

                graph.RemoveNode(nodeName);

                var nodeToRemove = GraphCanvas.Children
                    .OfType<Grid>()
                    .FirstOrDefault(grid => grid.Children.OfType<TextBlock>().FirstOrDefault()?.Text == nodeName);

                if (nodeToRemove != null)
                {
                    GraphCanvas.Children.Remove(nodeToRemove);
                }

                // Удалить все рёбра, связанные с узлом
                var edgesToRemove = GraphCanvas.Children.OfType<Line>()
                    .Where(line => line.Tag is Tuple<string, string> edgeTag &&
                                   (edgeTag.Item1 == nodeName || edgeTag.Item2 == nodeName))
                    .ToList();

                foreach (var edge in edgesToRemove)
                {
                    GraphCanvas.Children.Remove(edge);
                }

                NodeNameTextBox.Clear();
            }

            // Добавление рёбра
            private void AddEdge_Click(object sender, RoutedEventArgs e)
            {
                string start = EdgeStartTextBox.Text.Trim();
                string end = EdgeEndTextBox.Text.Trim();

                if (!int.TryParse(EdgeWeightTextBox.Text.Trim(), out int weight))
                {
                    MessageBox.Show("Введите корректное значение веса.");
                    return;
                }

                if (!graph.Nodes.ContainsKey(start) || !graph.Nodes.ContainsKey(end))
                {
                    MessageBox.Show("Один из узлов не существует.");
                    return;
                }

                graph.AddEdge(start, end, weight);
                AddEdgeToCanvas(start, end, weight);
                EdgeStartTextBox.Clear();
                EdgeEndTextBox.Clear();
                EdgeWeightTextBox.Clear();
            }

            // Удаление рёбра
            private void DeleteEdge_Click(object sender, RoutedEventArgs e)
            {
                string start = EdgeStartTextBox.Text.Trim();
                string end = EdgeEndTextBox.Text.Trim();

                if (!graph.Nodes.ContainsKey(start) || !graph.Nodes.ContainsKey(end))
                {
                    MessageBox.Show("Указанные узлы не найдены.");
                    return;
                }

                graph.RemoveEdge(start, end);

                var edgeToRemove = GraphCanvas.Children.OfType<Line>()
                    .FirstOrDefault(line => line.Tag is Tuple<string, string> edgeTag &&
                                            edgeTag.Item1 == start && edgeTag.Item2 == end);

                if (edgeToRemove != null)
                {
                    GraphCanvas.Children.Remove(edgeToRemove);
                }
            }
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

        // Перемещение узлов (ПКМ)
        private void Canvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
            {
                if (movingNode != null)
                {
                    movingNode = null;
                    GraphCanvas.ReleaseMouseCapture();
                }
            }

            private void Canvas_MouseMove(object sender, MouseEventArgs e)
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

        // Логи поиска пути
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
                Log($"Длина кратчайшего пути: {graph.GetPathDistance(path)}.");

                // Подсвечиваем весь путь зелёным
                foreach (var node in path)
                {
                    HighlightNode(node, Brushes.Green);
                    await Task.Delay(500); // Задержка для подсветки каждого узла
                }
            }
            else
            {
                Log("Кратчайший путь не найден.");
                MessageBox.Show("Кратчайший путь не найден.");
            }

            // Убираем подсветку после завершения
            await Task.Delay(1000);
            ResetNodeColors();
        }
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

        private void LogStep(string currentNode, string neighbor, int distance)
            {
                Dispatcher.Invoke(() =>
                {
                    Log($"Идём из {currentNode} в {neighbor}, расстояние: {distance}.");
                });
            }

            private void Log(string message)
            {
                AlgorithmLog.AppendText($"{message}{Environment.NewLine}");
            }
        // Добавление узла на Canvas
        private void AddNodeToCanvas(string name, Point position)
        {
            // Создаём круг для узла
            Ellipse node = new Ellipse
            {
                Width = NodeRadius * 2,
                Height = NodeRadius * 2,
                Fill = Brushes.LightBlue,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            // Создаём текст для отображения имени узла
            TextBlock label = new TextBlock
            {
                Text = name,
                Foreground = Brushes.Black,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Объединяем круг и текст в Grid
            Grid nodeGroup = new Grid();
            nodeGroup.Children.Add(node);
            nodeGroup.Children.Add(label);

            // Привязываем события для перемещения
            nodeGroup.MouseRightButtonDown += Node_MouseRightButtonDown;
            nodeGroup.MouseRightButtonUp += Node_MouseRightButtonUp;
            nodeGroup.MouseMove += Node_MouseMove;

            // Добавляем узел на Canvas
            GraphCanvas.Children.Add(nodeGroup);
            Canvas.SetLeft(nodeGroup, position.X - NodeRadius);
            Canvas.SetTop(nodeGroup, position.Y - NodeRadius);

            // Сохраняем позицию узла в модели графа
            graph.Nodes[name].Position = position;
        }

        // Добавление ребра на Canvas
        private void AddEdgeToCanvas(string start, string end, int weight)
        {
            if (!graph.Nodes.ContainsKey(start) || !graph.Nodes.ContainsKey(end))
                return;

            // Получаем координаты начальной и конечной точек
            var startPosition = graph.Nodes[start].Position;
            var endPosition = graph.Nodes[end].Position;

            // Вычисляем крайние точки линии с учётом радиуса узлов
            var edgePoints = GetEdgePoints(startPosition, endPosition, NodeRadius);

            // Создаём линию
            Line edge = new Line
            {
                X1 = edgePoints.Item1.X,
                Y1 = edgePoints.Item1.Y,
                X2 = edgePoints.Item2.X,
                Y2 = edgePoints.Item2.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Tag = Tuple.Create(start, end) // Сохраняем информацию о вершинах в теге линии
            };

            // Добавляем линию на Canvas
            GraphCanvas.Children.Add(edge);

            // Создаём текст с весом ребра
            TextBlock weightLabel = new TextBlock
            {
                Text = weight.ToString(),
                Foreground = Brushes.Red,
                FontWeight = FontWeights.Bold,
                Background = Brushes.White
            };

            // Устанавливаем текст в середину линии
            double midX = (edge.X1 + edge.X2) / 2;
            double midY = (edge.Y1 + edge.Y2) / 2;
            Canvas.SetLeft(weightLabel, midX);
            Canvas.SetTop(weightLabel, midY);

            // Добавляем текст на Canvas
            GraphCanvas.Children.Add(weightLabel);
        }

        // Обновление положения рёбер, связанных с узлом
        private void UpdateEdges(string nodeName)
        {
            if (string.IsNullOrEmpty(nodeName) || !graph.Nodes.ContainsKey(nodeName))
                return;

            // Получаем текущую позицию перемещённого узла
            var nodePosition = graph.Nodes[nodeName].Position;

            // Обходим все рёбра на Canvas
            foreach (var edge in GraphCanvas.Children.OfType<Line>())
            {
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

        // Вспомогательный метод для обновления линии
        private void UpdateLine(Line edge, Point startPosition, Point endPosition)
        {
            // Получаем крайние точки линии с учётом радиуса узлов
            var edgePoints = GetEdgePoints(startPosition, endPosition, NodeRadius);

            edge.X1 = edgePoints.Item1.X;
            edge.Y1 = edgePoints.Item1.Y;
            edge.X2 = edgePoints.Item2.X;
            edge.Y2 = edgePoints.Item2.Y;

            // Обновляем текст с весом, если он связан с этой линией
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

        // Вспомогательный метод для вычисления крайних точек линии
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
        // Обработчик события при нажатии ПКМ на узел
        private void Node_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid nodeGroup)
            {
                movingNode = nodeGroup; // Запоминаем текущий узел для перемещения
                Point mousePosition = e.GetPosition(GraphCanvas);
                double nodeX = Canvas.GetLeft(nodeGroup);
                double nodeY = Canvas.GetTop(nodeGroup);

                // Вычисляем смещение мыши относительно узла
                mouseOffset = new Point(mousePosition.X - nodeX, mousePosition.Y - nodeY);

                // Устанавливаем режим захвата мыши
                GraphCanvas.CaptureMouse();

                // Подсвечиваем узел для обозначения перемещения
                var ellipse = nodeGroup.Children.OfType<Ellipse>().FirstOrDefault();
                if (ellipse != null)
                {
                    ellipse.Fill = Brushes.Orange; // Подсветка текущего узла
                }
            }
        }

        // Обработчик события при отпускании ПКМ
        private void Node_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (movingNode != null)
            {
                // Убираем подсветку узла
                var ellipse = movingNode.Children.OfType<Ellipse>().FirstOrDefault();
                if (ellipse != null)
                {
                    ellipse.Fill = Brushes.LightBlue; // Возвращаем цвет узла
                }

                movingNode = null; // Сбрасываем перемещаемый узел
                GraphCanvas.ReleaseMouseCapture(); // Отпускаем мышь
            }
        }

        // Обработчик события перемещения узла (ПКМ)
        private void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (movingNode != null && e.RightButton == MouseButtonState.Pressed)
            {
                // Получаем текущую позицию мыши
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

                // Обновляем связанные рёбра
                UpdateEdges(label?.Text);
            }
        }
        private void ClearPath_Click(object sender, RoutedEventArgs e)
        {
            // Очищаем поле логов
            AlgorithmLog.Clear();

            // Сбрасываем подсветку всех узлов
            ResetNodeColors();
        }
        private void GraphCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
        private void ResetNodeColors()
        {
            foreach (var nodeGroup in GraphCanvas.Children.OfType<Grid>())
            {
                var ellipse = nodeGroup.Children.OfType<Ellipse>().FirstOrDefault();
                if (ellipse != null)
                {
                    ellipse.Fill = Brushes.LightBlue; // Основной цвет узлов
                }
            }
        }

    }
}
        

    

