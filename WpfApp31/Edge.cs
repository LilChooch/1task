using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp31
{
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
}
