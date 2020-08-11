using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.Help_classes
{
   public class ConvolutionalModelDemo
   {
        public bool probabilisticModel;
        public bool periodic;

        public ConvolutionalModel Model;
        public InputModel myModel;

        public static int[,,] output;

        public string message;
        public ConvolutionalModelDemo(Vector3d inSize, Vector3d outSize, List<Voxel> inputVoxels, int pattern_Size, bool probabilistic, bool periodic)
        {
            //from this model we get our input
            InputModel model = new InputModel(new Coord3D((int)inSize.X, (int)inSize.Y, (int)inSize.Z), inputVoxels);

            //this is the size of the output model
            var outputSizeInCoord = new Coord3D((int)outSize.X, (int)outSize.Y, (int)outSize.Z);

            //from this model we will get our output
            Model = new ConvolutionalModel(model, pattern_Size, outputSizeInCoord, periodic, probabilistic);
        }


       // private void Start()
       //{
       //     var inputModel = Init();

       //     var outputSizeInCoord = new Coord3D((int)outputSize.x, (int)outputSize.y, (int)outputSize.z);

       //     Model = new ConvolutionalModel(inputModel, patternSize, outputSizeInCoord, periodic, probabilisticModel);
       // }

        //private new void Update()
        //{
        //    base.Update();
        //}

        public void GenerateOutput()
        {

            var Gen = 0;
            while ((!Model.GenerationFinished) && (Gen < 500))
            {

                Model.Observe();

                Gen++;

                if (Model.Contradiction)
                {
                    message += "I failed after " + Gen + " iterations";
                    Model.Clear();
                }

            }
            message += "I am done after " + Gen + " iterations";
        }
       
        public void GenerateOutputOnDemand()
        {
            Model.Observe();
            if (Model.Contradiction)
            {
                message += "I failed after " + Model.NumGen + " iterations";
            }

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
