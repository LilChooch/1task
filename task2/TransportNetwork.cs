using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace task2
{
    public class TransportNetwork
    {
        public Dictionary<string, Node> Nodes { get; set; }
        public List<Edge> Edges { get; set; }

        public TransportNetwork()
        {
            Nodes = new Dictionary<string, Node>();
            Edges = new List<Edge>();
        }

        public void AddNode(string name)
        {
            if (!Nodes.ContainsKey(name))
            {
                Nodes[name] = new Node(name);
            }
        }

        public void RemoveNode(string name)
        {
            Nodes.Remove(name);
            Edges.RemoveAll(e => e.Start == name || e.End == name);
        }

        public void AddEdge(string start, string end, int capacity)
        {
            Edges.Add(new Edge(start, end, capacity));
        }

        public void RemoveEdge(string start, string end)
        {
            Edges.RemoveAll(e => e.Start == start && e.End == end);
        }

        public int FindMaxFlow(string source, string sink, Action<string> log)
        {
            int maxFlow = 0;

            var residualEdges = Edges.ToDictionary(e => (e.Start, e.End), e => e.Capacity);

            while (true)
            {
                var path = FindAugmentingPath(source, sink, residualEdges, log);
                if (path == null)
                {
                    break;
                }

                int flow = path.Min(edge => residualEdges[edge]);
                foreach (var edge in path)
                {
                    residualEdges[edge] -= flow;
                    if (!residualEdges.ContainsKey((edge.Item2, edge.Item1)))
                    {
                        residualEdges[(edge.Item2, edge.Item1)] = 0;
                    }
                    residualEdges[(edge.Item2, edge.Item1)] += flow;
                }

                maxFlow += flow;
                log($"Добавлен поток: {flow}");
            }

            return maxFlow;
        }

        private List<(string, string)> FindAugmentingPath(string source, string sink, Dictionary<(string, string), int> residualEdges, Action<string> log)
        {
            var parent = new Dictionary<string, string>();
            var distance = Nodes.ToDictionary(node => node.Key, _ => int.MaxValue);
            distance[source] = 0;

            for (int i = 0; i < Nodes.Count - 1; i++)
            {
                foreach (var edge in Edges)
                {
                    if (residualEdges[(edge.Start, edge.End)] > 0 && distance[edge.Start] != int.MaxValue)
                    {
                        if (distance[edge.Start] + 1 < distance[edge.End])
                        {
                            distance[edge.End] = distance[edge.Start] + 1;
                            parent[edge.End] = edge.Start;
                        }
                    }
                }
            }

            if (!parent.ContainsKey(sink))
            {
                return null;
            }

            var path = new List<(string, string)>();
            string current = sink;
            while (current != source)
            {
                string prev = parent[current];
                path.Add((prev, current));
                current = prev;
            }

            path.Reverse();
            log($"Найден путь: {string.Join(" -> ", path.Select(p => $"{p.Item1}->{p.Item2}"))}");
            return path;
        }
    }

    public class Node
    {
        public string Name { get; set; }
        public Point Position { get; set; }

        public Node(string name)
        {
            Name = name;
        }
    }

    public class Edge
    {
        public string Start { get; set; }
        public string End { get; set; }
        public int Capacity { get; set; }

        public Edge(string start, string end, int capacity)
        {
            Start = start;
            End = end;
            Capacity = capacity;
        }
    }
}

