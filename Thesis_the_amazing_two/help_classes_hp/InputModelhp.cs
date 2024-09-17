
using System.Collections.Generic;
using Thesis.Help_classes;

namespace Thesis.help_classes_hp
{
    //test comment
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
