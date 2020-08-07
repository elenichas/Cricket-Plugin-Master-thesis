using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.Help_classes
{
    //The input model is a list of voxels and has a size  in x,y,z  direction (Coord)
    public struct InputModel
    {
        public Coord3D Size;

        public List<Voxel> Voxels;

        public InputModel(Coord3D size, List<Voxel> voxels)
        {
            Size = size;
            Voxels = voxels;
        }
    }
}
