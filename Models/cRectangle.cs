using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Face_Processing.Models
{
    public class cRectangle : List<int>
    {
        public Rectangle Rectangle { get; set; }
        public Rgb Color { get; set; }
    }
}
