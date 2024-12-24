using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp31
{
    public class Node
    {
        public string Name { get; set; }
        public double X { get; set; } // Координата на Canvas
        public double Y { get; set; }

        public Node(string name)
        {
            Name = name;
            X = 0;
            Y = 0;
        }
    }
}
