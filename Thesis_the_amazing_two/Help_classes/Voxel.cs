using Rhino.Geometry;
using System.Drawing;
 

namespace Thesis.Help_classes
{
    public class Voxel
    {
        //the x,y,z coordinates
        public byte X, Y, Z;

        //the color of each voxel
        public int Identity;
        public double Value;

        //simple constructor
        public Voxel(int x, int y, int z, int identity)
        {
            X = (byte)x;
            Y = (byte)y;
            Z = (byte)z;
            Identity = identity;
         

        }
        public Voxel(int x, int y, int z, int identity,double value)
        {
            X = (byte)x;
            Y = (byte)y;
            Z = (byte)z;
            Identity = identity;
            Value = value;

        }


    }
    }
