
using System.Collections.Generic;
using System.Drawing;
using Rhino.Geometry;

namespace Thesis.Help_classes
{
    public class SimpleDemo
    {

        public bool probabilisticModel;
        public bool periodic;

        public SimpleModel Model;
        public InputModel myModel;

        public  static int[,,] output;

        public SimpleDemo(Vector3d inSize, Vector3d outSize, List<Voxel> inputVoxels, int pattern_Size, bool probabilistic, bool periodic) 
        { 
            //from this model we get our input
            InputModel model = new InputModel(new Coord3D((int)inSize.X, (int)inSize.Y, (int)inSize.Z), inputVoxels);

            //this is the size of the output model
            var outputSizeInCoord = new Coord3D((int)outSize.X, (int)outSize.Y, (int)outSize.Z);

            //from this model we will get our output
            Model = new SimpleModel(model, pattern_Size, outputSizeInCoord, periodic,true, probabilistic);
        }

        public void GenerateOutput()
        {
            var Gen = 0;
            while( (!Model.GenerationFinished)&&(Gen<200))
            {
                
                Model.Observe();

                Gen++;

               if (Model.Contradiction)
               {
                   Rhino.RhinoApp.WriteLine($"I failed after {Model.NumGen} iterations!");
                   Model.Clear();
               }
    
            }
            Rhino.RhinoApp.WriteLine($"I am done after {Model.NumGen} iterations!");
        }
        public void GenerateOutputOnDemand()
        {
                Model.Observe();
        }

        public void DisplayOutput()
        {
             output = Model.GetOutput();
        }

        public void ClearModel()
        {
            Model.Clear();
            Rhino.RhinoApp.WriteLine("Model cleared!");
        }

        public List<Voxel> GetOutput()
        {
            var output = Model.GetOutput();
            var voxModel = new VoxelModel();
            return voxModel.Get(output);
        }
    }
}
