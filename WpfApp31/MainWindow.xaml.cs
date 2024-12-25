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
using System.Xml;


namespace WpfApp31
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Graph graph;
        private Dictionary<string, Ellipse> nodeElements;
        private Dictionary<(string, string), Line> edgeElements;
        private Dictionary<(string, string), TextBlock> edgeWeightLabels;
        private const double NodeRadius = 20;

        public MainWindow()
        {
            InitializeComponent();
            graph = new Graph();
            nodeElements = new Dictionary<string, Ellipse>();
            edgeElements = new Dictionary<(string, string), Line>();
            edgeWeightLabels = new Dictionary<(string, string), TextBlock>();
        }
        private void SaveGraph_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON Files|*.json",
                Title = "Сохранить граф"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string json = JsonConvert.SerializeObject(graph, (Newtonsoft.Json.Formatting)System.Xml.Formatting.Indented);
                File.WriteAllText(saveFileDialog.FileName, json);
                MessageBox.Show("Граф сохранён.");
            }
        }

        private void LoadGraph_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files|*.json",
                Title = "Загрузить граф"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string json = File.ReadAllText(openFileDialog.FileName);
                graph = JsonConvert.DeserializeObject<Graph>(json);
                RefreshCanvas();
            }
        }

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

        private void DeleteNode_Click(object sender, RoutedEventArgs e)
        {
            string nodeName = NodeNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(nodeName))
            {
                MessageBox.Show("Введите имя узла.");
                return;
            }

            if (!graph.Nodes.ContainsKey(nodeName))
            {
                MessageBox.Show("Узел не найден.");
                return;
            }

            graph.RemoveNode(nodeName);
            RemoveNodeFromCanvas(nodeName);
            NodeNameTextBox.Clear();
        }

        private void AddEdge_Click(object sender, RoutedEventArgs e)
        {
            string start = EdgeStartTextBox.Text.Trim();
            string end = EdgeEndTextBox.Text.Trim();
            int weight = 0;

            if (!int.TryParse(EdgeWeightTextBox.Text.Trim(), out weight))
            {
                MessageBox.Show("Введите корректный вес.");
                return;
            }

            if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
            {
                MessageBox.Show("Введите начальный и конечный узлы.");
                return;
            }

            if (!graph.Nodes.ContainsKey(start) || !graph.Nodes.ContainsKey(end))
            {
                MessageBox.Show("Один из узлов не существует.");
                return;
            }

            if (graph.Edges.Any(edge => edge.Start == start && edge.End == end))
            {
                MessageBox.Show("Ребро уже существует.");
                return;
            }

            graph.AddEdge(start, end, weight);
            AddEdgeToCanvas(start, end, weight);
            EdgeStartTextBox.Clear();
            EdgeEndTextBox.Clear();
            EdgeWeightTextBox.Clear();
        }

        private void DeleteEdge_Click(object sender, RoutedEventArgs e)
        {
            string start = EdgeStartTextBox.Text.Trim();
            string end = EdgeEndTextBox.Text.Trim();

            if (!graph.Edges.Any(edge => edge.Start == start && edge.End == end))
            {
                MessageBox.Show("Ребро не найдено.");
                return;
            }

            graph.RemoveEdge(start, end);
            RemoveEdgeFromCanvas(start, end);
        }

        private void StartTraversal_Click(object sender, RoutedEventArgs e)
        {
            TraversalLog.Clear();
            string traversalType = ((ComboBoxItem)TraversalTypeComboBox.SelectedItem).Content.ToString();

            if (graph.Nodes.Count == 0)
            {
                MessageBox.Show("Граф пуст.");
                return;
            }

            string startNode = graph.Nodes.Keys.First();
            if (traversalType.Contains("BFS"))
            {
                BFS(startNode);
            }
            else if (traversalType.Contains("DFS"))
            {
                DFS(startNode);
            }
        }



        #region Вспомогательные методы

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

            GraphCanvas.Children.Add(nodeGroup);
            Canvas.SetLeft(nodeGroup, position.X - NodeRadius);
            Canvas.SetTop(nodeGroup, position.Y - NodeRadius);

            graph.Nodes[name].X = position.X;
            graph.Nodes[name].Y = position.Y;

            nodeElements[name] = node;
        }

        private void RemoveNodeFromCanvas(string name)
        {
            foreach (UIElement element in GraphCanvas.Children)
            {
                if (element is Grid grid)
                {
                    TextBlock label = grid.Children.OfType<TextBlock>().FirstOrDefault();
                    if (label != null && label.Text == name)
                    {
                        GraphCanvas.Children.Remove(grid);
                        break;
                    }
                }
            }

            var connectedEdges = graph.Edges.Where(edge => edge.Start == name || edge.End == name).ToList();
            foreach (var edge in connectedEdges)
            {
                RemoveEdgeFromCanvas(edge.Start, edge.End);
                graph.RemoveEdge(edge.Start, edge.End);
            }
        }

        private void AddEdgeToCanvas(string start, string end, int weight)
        {
            Line edge = new Line
            {
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            var startNode = graph.Nodes[start];
            var endNode = graph.Nodes[end];

            edge.X1 = startNode.X;
            edge.Y1 = startNode.Y;
            edge.X2 = endNode.X;
            edge.Y2 = endNode.Y;

            GraphCanvas.Children.Add(edge);
            edgeElements[(start, end)] = edge;

            TextBlock weightLabel = new TextBlock
            {
                Text = weight.ToString(),
                Foreground = Brushes.Red,
                FontWeight = FontWeights.Bold
            };

            GraphCanvas.Children.Add(weightLabel);
            Canvas.SetLeft(weightLabel, (edge.X1 + edge.X2) / 2);
            Canvas.SetTop(weightLabel, (edge.Y1 + edge.Y2) / 2);

            edgeWeightLabels[(start, end)] = weightLabel;
        }

        private void RemoveEdgeFromCanvas(string start, string end)
        {
            if (edgeElements.ContainsKey((start, end)))
            {
                GraphCanvas.Children.Remove(edgeElements[(start, end)]);
                edgeElements.Remove((start, end));
            }

            if (edgeWeightLabels.ContainsKey((start, end)))
            {
                GraphCanvas.Children.Remove(edgeWeightLabels[(start, end)]);
                edgeWeightLabels.Remove((start, end));
            }
        }

        private void RefreshCanvas()
        {
            GraphCanvas.Children.Clear();
            nodeElements.Clear();
            edgeElements.Clear();
            edgeWeightLabels.Clear();

            foreach (var node in graph.Nodes.Values)
            {
                AddNodeToCanvas(node.Name, new Point(node.X, node.Y));
            }

            foreach (var edge in graph.Edges)
            {
                AddEdgeToCanvas(edge.Start, edge.End, edge.Weight);
            }
        }
        #region Обработчики событий

        private async void BFS_Click(object sender, RoutedEventArgs e)
        {
            if (graph.Nodes.Count == 0)
            {
                MessageBox.Show("Граф пуст.");
                return;
            }

            string startNode = graph.Nodes.Keys.First();
            await BFS(startNode);
        }

        private async void DFS_Click(object sender, RoutedEventArgs e)
        {
            if (graph.Nodes.Count == 0)
            {
                MessageBox.Show("Граф пуст.");
                return;
            }

            string startNode = graph.Nodes.Keys.First();
            await DFS(startNode);
        }

        private void GraphCanvas_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(GraphCanvas);
            string nodeName = ((char)('A' + graph.Nodes.Count)).ToString();

            if (!graph.Nodes.ContainsKey(nodeName))
            {
                graph.AddNode(nodeName);
                AddNodeToCanvas(nodeName, position);
            }
        }

       

        private async Task BFS(string startNode)
        {
            Queue<string> queue = new Queue<string>();
            HashSet<string> visited = new HashSet<string>();

            queue.Enqueue(startNode);
            visited.Add(startNode);
            Log($"Начинаем BFS с узла {startNode}.");

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();
                HighlightNode(current, Brushes.Yellow);
                Log($"Посещаем узел {current}.");

                foreach (var neighbor in graph.GetAdjacent(current))
                {
                    if (!visited.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        visited.Add(neighbor);
                        Log($"Добавляем в очередь узел {neighbor}.");
                    }
                }

                await Task.Delay(500);
                HighlightNode(current, Brushes.LightBlue);
            }
        }

        private async Task DFS(string startNode)
        {
            Stack<string> stack = new Stack<string>();
            HashSet<string> visited = new HashSet<string>();

            stack.Push(startNode);
            Log($"Начинаем DFS с узла {startNode}.");

            while (stack.Count > 0)
            {
                string current = stack.Pop();
                if (!visited.Contains(current))
                {
                    visited.Add(current);
                    HighlightNode(current, Brushes.Yellow);
                    Log($"Посещаем узел {current}.");

                    foreach (var neighbor in graph.GetAdjacent(current))
                    {
                        if (!visited.Contains(neighbor))
                        {
                            stack.Push(neighbor);
                            Log($"Добавляем в стек узел {neighbor}.");
                        }
                    }

                    await Task.Delay(500);
                    HighlightNode(current, Brushes.LightBlue);
                }
            }
        }

        private void Log(string message)
        {
            TraversalLog.AppendText(message + Environment.NewLine);
        }

        private void HighlightNode(string nodeName, Brush color)
        {
            if (nodeElements.ContainsKey(nodeName))
            {
                nodeElements[nodeName].Fill = color;
            }
        }

        #endregion
    }
}
#endregion