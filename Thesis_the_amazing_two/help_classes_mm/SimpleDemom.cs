using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Help_classes;

namespace Thesis.help_classes_mm
{
    //The simple demo is the the script that calls the basic methods that generate the output,display and clear it
    public class SimpleDemom
    {

        public bool probabilisticModel;
        public bool periodic;

        public SimpleModelm Model;
        public InputModelm myModel;

        public static int[,,] output;

        public string message;
        //public SimpleDemo(Vector3d inSize, Vector3d outSize, List<Voxel> inputVoxels, int pattern_Size, bool probabilistic, bool periodic, bool Optimized, bool Maximize, bool Minimize)
        //{
        //    from this model we get our input
        //    InputModel model = new InputModel(new Coord3D((int)inSize.X, (int)inSize.Y, (int)inSize.Z), inputVoxels);

        //    this is the size of the output model
        //    var outputSizeInCoord = new Coord3D((int)outSize.X, (int)outSize.Y, (int)outSize.Z);

        //    from this model we will get our output
        //    Model = new SimpleModel(model, pattern_Size, outputSizeInCoord, periodic, false, probabilistic, Optimized, Maximize, Minimize);
        //}

        public SimpleDemom(Vector3d inSize, Vector3d outSize, List<Voxel> inputVoxels, int pattern_Size, bool probabilistic, bool periodic, bool OptimizedModel, bool maximize, bool minimize)
        {
            //from this model we get our input
            InputModelm model = new InputModelm(new Coord3D((int)inSize.X, (int)inSize.Y, (int)inSize.Z), inputVoxels);

            //this is the size of the output model
            var outputSizeInCoord = new Coord3D((int)outSize.X, (int)outSize.Y, (int)outSize.Z);

            //from this model we will get our output
            Model = new SimpleModelm(model, pattern_Size, outputSizeInCoord, periodic, false, probabilistic, OptimizedModel, maximize, minimize); ;
        }


        //this method is used by the model Synthesis component
        //public void GenerateOutput()
        //{
        //    var Gen = 0;
        //    while( (!Model.GenerationFinished)&&(Gen<500))
        //    {

        //        Model.Observe();

        //        Gen++;

        //       if (Model.Contradiction)
        //       {
        //           message = "I failed after " + Gen + " iterations";
        //           Model.Clear();
        //       }

        //    }
        //    message = "I am done after " + Gen + " iterations";
        //}
        public void GenerateOutputandOptimize()
        {

            var Gen = 0;
            while ((!Model.GenerationFinished) && (Gen < 500))
            {

                Model.ObserveAndOptimize();

                Gen++;

                if (Model.Contradiction)
                {
                    message += "I failed after " + Gen + " iterations";
                    Model.Clear();
                }

            }
            message += "I am done after " + Gen + " iterations";
        }


        public void DisplayOutput()
        {
            output = Model.GetOutput();
        }

        public void ClearModel()
        {
            Model.Clear();
            message = "Model cleared";
        }

        public List<Voxel> GetOutput()
        {
            var output = Model.GetOutput();
            var voxModel = new VoxelModel();
            return voxModel.Get(output);
        }
    }
}
