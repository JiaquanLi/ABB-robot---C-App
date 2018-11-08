using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABBpainting
{
    class BrushStatus
    {
        private int x, y, z;

        public int X
        {
            get
            {
                return x;
            }

            set
            {
                x = value;
            }
        }

        public int Y
        {
            get
            {
                return y;
            }

            set
            {
                y = value;
            }
        }

        public int Z
        {
            get
            {
                return z;
            }

            set
            {
                z = value;
            }
        }

        public BrushStatus()
        {
            this.X = 0;
            this.Y = 0;
            this.Z = 0;
        }

        public BrushStatus(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public void Mirroring()
        {
            var c = x;
            x = y;
            y = c;
        }

        public override string ToString()
        {
            return "[ " +X + " ; "+ Y +" ; " + Z +" ]\n";
        }
    }
}
