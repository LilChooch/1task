using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WpfApp31;

namespace WpfApp31
{
    public class Graph
    {
        public Dictionary<string, Node> Nodes { get; set; }
        public List<Edge> Edges { get; set; }

        public Graph()
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
            if (Nodes.ContainsKey(name))
            {
                Nodes.Remove(name);
                Edges.RemoveAll(e => e.Start == name || e.End == name);
            }
        }

        public void AddEdge(string start, string end, int weight)
        {
            if (Nodes.ContainsKey(start) && Nodes.ContainsKey(end))
            {
                Edges.Add(new Edge(start, end, weight));
            }
        }

        public void RemoveEdge(string start, string end)
        {
            Edges.RemoveAll(e => e.Start == start && e.End == end);
        }

        public List<string> GetAdjacent(string nodeName)
        {
            List<string> adjacentNodes = new List<string>();

            foreach (var edge in Edges)
            {
                if (edge.Start == nodeName)
                {
                    adjacentNodes.Add(edge.End);
                }
                else if (edge.End == nodeName)
                {
                    adjacentNodes.Add(edge.Start);
                }
            }

            return adjacentNodes;
        }
    }
}
