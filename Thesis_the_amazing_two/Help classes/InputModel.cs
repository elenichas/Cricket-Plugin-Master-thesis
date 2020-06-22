using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis_1.Help_classes
{
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
