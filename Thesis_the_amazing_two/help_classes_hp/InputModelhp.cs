using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Help_classes;

namespace Thesis.help_classes_hp
{
    //The input model is a list of voxels and has a size  in x,y,z  direction (Coord)
    public struct InputModelhp
    {
        public Coord3D Size;

        public List<Voxel> Voxels;

        public InputModelhp(Coord3D size, List<Voxel> voxels)
        {
            Size = size;
            Voxels = voxels;
        }
    }
}
