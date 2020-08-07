
using System.Collections.Generic;
using System.Drawing;
using Rhino.Geometry;

namespace Thesis.Help_classes
{
    //The simple demo is the the script that calls the basic methods that generate the output,display and clear it
    public class SimpleDemo
    {

        public bool probabilisticModel;
        public bool periodic;

        public SimpleModel Model;
        public InputModel myModel;

        public  static int[,,] output;

        public string message;

        public SimpleDemo(Vector3d inSize, Vector3d outSize, List<Voxel> inputVoxels, int pattern_Size, bool probabilistic, bool periodic) 
        { 
            //from this model we get our input
            InputModel model = new InputModel(new Coord3D((int)inSize.X, (int)inSize.Y, (int)inSize.Z), inputVoxels);

            //this is the size of the output model
            var outputSizeInCoord = new Coord3D((int)outSize.X, (int)outSize.Y, (int)outSize.Z);

            //from this model we will get our output
            Model = new SimpleModel(model, pattern_Size, outputSizeInCoord, periodic, false, probabilistic);
        }

        
        //this method is used by the model Synthesis component
        public void GenerateOutput()
        {
            var Gen = 0;
            while( (!Model.GenerationFinished)&&(Gen<500))
            {
                
                Model.Observe();

                Gen++;

               if (Model.Contradiction)
               {
                   message = "I failed after " + Gen + " iterations";
                   Model.Clear();
               }
    
            }
            message = "I am done after " + Gen + " iterations";
        }

        //this method is used by the model Synthesis2 component that needs a timer to update
        public void GenerateOutputOnDemand()
        {
                Model.Observe();
            if (Model.Contradiction)
            {
                message = "I failed after " + Model.NumGen + " iterations";
            }

        }

        public void DisplayOutput()
        {
             output = Model.GetOutput();
        }

        public void ClearModel()
        {
            Model.Clear();
            message ="Model cleared";
        }

        public List<Voxel> GetOutput()
        {
            var output = Model.GetOutput();
            var voxModel = new VoxelModel();
            return voxModel.Get(output);
        }
    }
}
