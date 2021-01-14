using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Thesis.Help_classes;
using Thesis.Properties;

namespace Thesis.Components
{
    public class ModelSynthesis_timer : GH_Component
    {
       //This component gets updated with a timer
       //it is created to see the steps followed by the algorithm and understand contradiction cases better
        public ModelSynthesis_timer()
          : base("Model Synthesis_timer", "Model Synthesis_timer",
              "3D implementation of the wfc algorithm with timer update",
              "Thesis", "Synthesis")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBoxParameter("Voxels", "IM", "The voxels of input model", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Encoded List", "IV", "The identities of the voxels", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Pattern Size", "P", "The size of patterns to extract from the input", GH_ParamAccess.item);
            pManager.AddVectorParameter("Input Size", "IP", "Input model size in XYZ dimensions", GH_ParamAccess.item);
            pManager.AddVectorParameter("Output Size", "OP", "Input model size in XYZ dimensions", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Probabilistic", "PR", "If true, uses the input model probabilities", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Periodic", "PE", "if true infers periodic adjacencies ", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "R", "Press to restart the algorithm", GH_ParamAccess.item);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBoxParameter("Output Model Voxels", "OM", "The output model", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Output Values", "OV", "The output values", GH_ParamAccess.list);
            //pManager.AddIntegerParameter("Patterns", "P", "The number of patters", GH_ParamAccess.item);
            pManager.AddGenericParameter("Pattern Probabilities", "P", "The probabilities", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Possibilities Count", "PC", "The number of possibilities remaining at each node", GH_ParamAccess.list);
            pManager.AddGenericParameter("UniquePatters", "P", "The Patterns", GH_ParamAccess.list);
        }


        List<Box> OutBoxes2 = new List<Box>();
        List<int> OutValues = new List<int>();
        List<string> Prob = new List<string>();
        SimpleDemo demo;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Box> InputBoxes = new List<Box>();
            List<Voxel> InputVoxels = new List<Voxel>();
            List<int> InputValues = new List<int>();

            Vector3d inSize = new Vector3d();
            Vector3d outSize = new Vector3d();

            bool Reset = false;
            bool Probabilistic = true;
            bool Periodic = true;

            int Pattern_Size = 0;

            if (!DA.GetDataList(0, InputBoxes)) return;
            if (!DA.GetDataList(1, InputValues)) return;
            if (!DA.GetData(2, ref Pattern_Size)) return;
            if (!DA.GetData(3, ref inSize)) return;
            if (!DA.GetData(4, ref outSize)) return;
            if (!DA.GetData(5, ref Probabilistic)) return;
            if (!DA.GetData(6, ref Periodic)) return;
            if (!DA.GetData(7, ref Reset)) return;

            if (Reset)
            {
                InputVoxels = new List<Voxel>();
 
                for (int i = 0; i < InputBoxes.Count; i++)
                {
                    int val = InputValues[i];
                    Voxel vox = new Voxel((int)Math.Floor(InputBoxes[i].Center.X), (int)Math.Floor(InputBoxes[i].Center.Y), (int)Math.Floor(InputBoxes[i].Center.Z), val);
                    InputVoxels.Add(vox);

                }
                demo = new SimpleDemo(inSize, outSize, InputVoxels, Pattern_Size, Probabilistic, Periodic);
                demo.ClearModel();
            }
            List<int> PossibilitiesCount = new List<int>();
            List<string> PossibilitiesCountName = new List<string>();
            var UniquePatterns = new List<string>();

            if (!demo.Model.GenerationFinished)
            {
                demo.GenerateOutputOnDemand();
                var matrix = demo.Model.outputMatrix;
                for (var x = 0; x < matrix.GetLength(0); x++)
                {
                    for (var y = 0; y < matrix.GetLength(1); y++)
                    {
                        for (var z = 0; z < matrix.GetLength(2); z++)
                        {
                            PossibilitiesCount.Add(matrix[x, y, z].Count);
                            var possb = String.Join(",", matrix[x, y, z]);
                            var nodeName = String.Format("{0},{1},{2}", x, y, z);
                            PossibilitiesCountName.Add(nodeName + ";" + possb);
                        }
                    }
                }

                var patterns = demo.Model.patterns;
                
                foreach (var pattern in patterns)
                {
                    var patternList = new List<int>();
                    for (var x = 0; x < pattern.GetLength(0); x++)
                    {
                        for (var y = 0; y < pattern.GetLength(1); y++)
                        {
                            for (var z = 0; z < 1; z++)
                            {
                                patternList.Add(pattern[x, y, z]);
                            }
                        }
                    }
                    UniquePatterns.Add(String.Join(",", patternList));
                }

            }
            else { Rhino.RhinoApp.WriteLine($"Model finished"); }

            var Output_voxels = new List<Voxel>();

            Output_voxels = demo.GetOutput();

            var rawOutput = demo.Model.GetOutput();

            if (Output_voxels.Count > 0)
            {
                OutValues = new List<int>();
                OutBoxes2 = new List<Box>();
                foreach (var v in Output_voxels)
                {
                    var domain = new Interval(-0.5, 0.5);
                    var plane = new Plane(new Point3d(((int)v.X) * 1.0, ((int)v.Y) * 1.0, ((int)v.Z) * 1.0), Vector3d.ZAxis);
                    var b = new Box(plane, domain, domain, domain);
                    OutBoxes2.Add(b);
                    OutValues.Add(rawOutput[v.X, v.Y, v.Z]);

                }
            }


            DA.SetDataList(0, OutBoxes2);
            DA.SetDataList(1, OutValues);

           // DA.SetData(2, demo.Model.patterns.Count());

            Prob = new List<string>();
            foreach (KeyValuePair<int, double> kvp in demo.Model.probabilites)
            {
                var st = (kvp.Key.ToString() + "--->" + (Math.Truncate(1000 * kvp.Value) / 1000).ToString());
                Prob.Add(st);
            }


            DA.SetDataList(2, PossibilitiesCountName);
            DA.SetDataList(3, PossibilitiesCount);
            DA.SetDataList(4, UniquePatterns);

        }
    
        protected override System.Drawing.Bitmap Icon
        {
            get
            {  
                return Resource.im5;
               
            }
        }

   
        public override Guid ComponentGuid
        {
            get { return new Guid("ea2c4647-53f2-4c6d-8781-9dd38e220066"); }
        }
    }
 
    
}