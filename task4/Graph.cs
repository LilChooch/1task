using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace task4
{
    public class Node
    {
        public string Name { get; set; }
        public Point Position { get; set; }
        public Node(string name) => Name = name;
        
    }

    public class Edge
    {
        public string Start { get; set; }
        public string End { get; set; }
        public int Weight { get; set; }

        public Edge(string start, string end, int weight)
        {
            Start = start;
            End = end;
            Weight = weight;
        }
    }


    public class Graph
    {
        public void RemoveEdge(string startNode, string endNode)
        {
            Edges.RemoveAll(edge => edge.Start == startNode && edge.End == endNode);
        }
        public void RemoveNode(string nodeName)
        {
            if (!Nodes.ContainsKey(nodeName))
                return;

            // Удаляем все рёбра, связанные с узлом
            Edges.RemoveAll(edge => edge.Start == nodeName || edge.End == nodeName);

            // Удаляем сам узел
            Nodes.Remove(nodeName);
        }
        public Dictionary<string, Node> Nodes { get; set; }
        public List<Edge> Edges { get; set; }

        public Graph()
        {
            Nodes = new Dictionary<string, Node>();
            Edges = new List<Edge>();
        }

        public void AddNode(string name) => Nodes[name] = new Node(name);

        public void AddEdge(string start, string end, int weight)
        {
            Edges.Add(new Edge(start, end, weight));
        }

        public List<string> Dijkstra(string source, string target, Action<string, string, int> logAction)
        {
            var distances = new Dictionary<string, int>();
            var previousNodes = new Dictionary<string, string>();
            var unvisited = new HashSet<string>(Nodes.Keys);

            foreach (var node in Nodes.Keys)
            {
                distances[node] = int.MaxValue;
                previousNodes[node] = null;
            }

            distances[source] = 0;

            while (unvisited.Count > 0)
            {
                var currentNode = unvisited.OrderBy(n => distances[n]).First();

                if (distances[currentNode] == int.MaxValue)
                    break;

                unvisited.Remove(currentNode);

                foreach (var edge in Edges.Where(e => e.Start == currentNode))
                {
                    string neighbor = edge.End; // Соседний узел
                    int newDistance = distances[currentNode] + edge.Weight;

                    if (newDistance < distances[neighbor])
                    {
                        distances[neighbor] = newDistance;
                        previousNodes[neighbor] = currentNode;

                        // Логируем шаг
                        logAction?.Invoke(currentNode, neighbor, newDistance);
                    }
                }

                if (currentNode == target)
                    break;
            }

            // Построение пути
            var path = new List<string>();
            for (var node = target; node != null; node = previousNodes[node])
            {
                path.Insert(0, node);
            }

            if (path.First() != source)
                return null;

            return path;
        }
    }
}
