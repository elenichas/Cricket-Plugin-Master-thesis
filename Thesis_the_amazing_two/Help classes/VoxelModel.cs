using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
 

namespace Thesis.Help_classes
{
    public class VoxelModel
    {
  
        public List<Voxel> Get(Color[,,] output, bool optimise = false)
        {
            var voxels = new List<Voxel>();

            for (var x = 0; x < output.GetLength(0); x++)
            {
                for (var y = 0; y < output.GetLength(1); y++)
                {
                    for (var z = 0; z < output.GetLength(2); z++)
                    {

                       // if (output[x, y, z] != 0)
                        if(!(output[x,y,z].IsEmpty))
                         {

                      
                            var cube = new Voxel(x, y, z, output[x,y,z]);
                           
                            voxels.Add(cube);
                        }
                    }
                }
            }

             
            return voxels;
        }

        
    }
}
