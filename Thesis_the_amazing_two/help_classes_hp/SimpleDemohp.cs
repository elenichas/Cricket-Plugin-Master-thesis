using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using Thesis.Help_classes;

namespace Thesis.help_classes_hp
{
    //The simple demo is the the script that calls the basic methods that generate the output,display and clear it
    public class SimpleDemohp
    {
        public bool probabilisticModel;
        public bool periodic;
        public bool buildingProgram;

        public SimpleModelhp Model;
        public InputModelhp myModel;

        public static int[,,] output;

        public string message;

        public SimpleDemohp(
            Vector3d inSize,
            Vector3d outSize,
            List<Voxel> inputVoxels,
            int pattern_Size,
            bool probabilistic,
            bool periodic,
            List<double> InputPercentages,
            bool buildingProgram
        )
        {
            //from this model we get our input
            InputModelhp model = new InputModelhp(
                new Coord3D((int)inSize.X, (int)inSize.Y, (int)inSize.Z),
                inputVoxels
            );

            //this is the size of the output model
            var outputSizeInCoord = new Coord3D((int)outSize.X, (int)outSize.Y, (int)outSize.Z);

            //from this model we will get our output
            Model = new SimpleModelhp(
                model,
                pattern_Size,
                outputSizeInCoord,
                periodic,
                false,
                probabilistic,
                InputPercentages,
                buildingProgram
            );
        }

        public void GenerateOutputandProgram()
        {
            var Gen = 0;
            while ((!Model.GenerationFinished) && (Gen < 1000))
            {
                Model.ObserveAndProgram();

                Gen++;

                if (Model.Contradiction)
                {
                    message = "I failed after " + Gen + " iterations";
                    Model.Clear();
                }
            }
            message = "I am done after " + Gen + " iterations";
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

        // Method to return the output as a list of Voxel objects
        public List<Voxel> GetOutput()
        {
            var output = Model.GetOutput();
            var voxModel = new VoxelModel();
            return voxModel.Get(output);
        }
    }
}
