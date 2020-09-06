using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
 using Thesis.Help_classes;
using Thesis.help_classes_mm;
using System.Linq;
using Thesis.Properties;

namespace Thesis.Components
{
    public class ModelSynthesis_max_min : GH_Component
    {
        
        public ModelSynthesis_max_min()
          : base("ModelSynthesis_max_min", "ModelSynthesis_max_min",
              "3D implementation of the wfc algorithm to max-min value",
              "Thesis", "Synthesis")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBoxParameter("Input Model", "IM", "The input model", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Input Identities", "I", "The values of voxels int the input model", GH_ParamAccess.list);
            pManager.AddNumberParameter("Input Values", "IV", "The values of voxels int the input model", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Pattern Size", "P", "The size of patterns to extract from the input", GH_ParamAccess.item);
            pManager.AddVectorParameter("Input Size", "IP", "Input model size in XYZ dimensions", GH_ParamAccess.item);
            pManager.AddVectorParameter("Output Size", "OP", "Input model size in XYZ dimensions", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Probabilistic", "PR", "If true, uses the input model probabilities", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Periodic", "PE", "if true infers periodic adjacencies ", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Generate", "G", "Press to create output model ", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Optimized Model","OM" , "If true, the input values are used to create the output", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Minimize", "MIN ", "The objective is to minimize the total value ", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Maximize", "MAX ", " The objective is to maximize the total value", GH_ParamAccess.item);
        }

      
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBoxParameter("Output Model", "OM", "The output model", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Output Values", "OV", "The output values", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Patterns", "P", "The number of patters", GH_ParamAccess.item);
            pManager.AddGenericParameter("Probabilities", "P", "The probabilities", GH_ParamAccess.list);
            pManager.AddGenericParameter("Messages", "M", "Message display", GH_ParamAccess.list);
        }

        List<Box> OutBoxes2 = new List<Box>();
        List<int> OutValues = new List<int>();
        List<string> Prob = new List<string>();

        public static bool OptimizedModel;
        public static bool Maximize;
        public static bool Minimize;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Box> InputBoxes = new List<Box>();
            List<Voxel> InputVoxels = new List<Voxel>();
            List<int> InputIDS = new List<int>();
            List<double> Inputvalues = new List<double>();

            Vector3d inSize = new Vector3d();
            Vector3d outSize = new Vector3d();

            bool Generate = false;
            bool Probabilistic = true;
            bool Periodic = true;
            bool OptimizedModel = true;
            bool Minimize = true;
            bool Maximize = false;


            int Pattern_Size = 0;

            if (!DA.GetDataList(0, InputBoxes)) return;
            if (!DA.GetDataList(1, InputIDS)) return;
            if (!DA.GetDataList(2, Inputvalues)) return;
            if (!DA.GetData(3, ref Pattern_Size)) return;
            if (!DA.GetData(4, ref inSize)) return;
            if (!DA.GetData(5, ref outSize)) return;
            if (!DA.GetData(6, ref Probabilistic)) return;
            if (!DA.GetData(7, ref Periodic)) return;
            if (!DA.GetData(8, ref Generate)) return;
            if (!DA.GetData(9, ref OptimizedModel)) return;
            if (!DA.GetData(10, ref Minimize)) return;
            if (!DA.GetData(11, ref Maximize)) return;

            //We convert our list of boxes to voxels, a voxel has x,y,z coordinates and a value       
            for (int i = 0; i < InputBoxes.Count; i++)
            {
                int identity = InputIDS[i];
                double value = Inputvalues[i];

                Voxel vox = new Voxel((int)Math.Floor(InputBoxes[i].Center.X), (int)Math.Floor(InputBoxes[i].Center.Y), (int)Math.Floor(InputBoxes[i].Center.Z), identity, value);
                InputVoxels.Add(vox);

            }

            var demo = new SimpleDemom(inSize, outSize, InputVoxels, Pattern_Size, Probabilistic, Periodic, OptimizedModel, Maximize, Minimize);

            string mes = " ";
            if (Generate)
            {

                demo.ClearModel();
                demo.GenerateOutputandOptimize();
                mes = demo.message;

            }
            DA.SetData(4, mes);

            var Output_voxels = new List<Voxel>();

            if (demo.Model.GenerationFinished)
            {
                Output_voxels = demo.GetOutput();
            }

            //we get the output values of the model
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

            DA.SetData(2, demo.Model.patterns.Count());

            //we output the probabilities of each pattern in the input model
            Prob = new List<string>();
            foreach (KeyValuePair<int, double> kvp in demo.Model.probabilites)
            {
                var st = (kvp.Key.ToString() + "--->" + (Math.Truncate(1000 * kvp.Value) / 1000).ToString());
                Prob.Add(st);
            }
            DA.SetDataList(3, Prob);

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {               
                return Resource.im4;

            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("2f9fc628-c3e1-4ccc-a3ad-cec894f5aa0b"); }
        }
    }
 
   
}