using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Help_classes;

namespace Thesis.help_classes_mm
{
    public struct InputModelm
    {
        public Coord3D Size;

        public List<Voxel> Voxels;

        public InputModelm(Coord3D size, List<Voxel> voxels)
        {
            Size = size;
            Voxels = voxels;
        }
    }
}
