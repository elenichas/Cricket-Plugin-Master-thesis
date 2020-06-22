using Rhino.Geometry;
using System.Drawing;
 

namespace Thesis.Help_classes
{
    public struct Voxel
    {
        //the x,y,z coordinates
        public byte X, Y, Z;

        //the color of each voxel
        public Color Color;
 
        public Voxel(int x, int y, int z, Color color)
        {
            X = (byte)x;
            Y = (byte)y;
            Z = (byte)z;
            Color = color;
            
        }
      
        
    }
    }
