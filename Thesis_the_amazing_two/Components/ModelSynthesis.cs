using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Thesis.Help_classes;
using Thesis.Properties;

namespace Thesis
{
    public class ModelSynthesis : GH_Component 
    {
      
        public ModelSynthesis()
          : base("Model Synthesis", "Model Synthesis",
              "3D implementation of the wfc algorithm",
              "Thesis", "Synthesis")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {           
            pManager.AddBoxParameter("Input Model", "IM", "The voxels of the input model", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Input Identities", "II", "The identities of the voxels", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Pattern Size", "P", "The size of patterns to extract from the input", GH_ParamAccess.item);
            pManager.AddVectorParameter("Input Size", "IP", "Input model size in XYZ dimensions", GH_ParamAccess.item);
            pManager.AddVectorParameter("Output Size", "OP", "Input model size in XYZ dimensions", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Probabilistic", "PR", "If true, uses the input model probabilities", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Periodic", "PE", "if true infers periodic adjacencies ", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Generate", "G", "Press to create output model ", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Fixed Corners", "FC", "The first node of the matrix collapsed into the first pattern", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
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
        public static  bool FixedCorners ;


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Box> InputBoxes = new List<Box>();
            List<Voxel> InputVoxels = new List<Voxel>();
            List<int> InputIdentities = new List<int>();

            Vector3d inSize = new Vector3d();
            Vector3d outSize = new Vector3d();

            bool Generate = false;          
            bool Probabilistic = true;
            bool Periodic = true;
           

            int Pattern_Size = 0;

            if (!DA.GetDataList(0,  InputBoxes)) return;
            if (!DA.GetDataList(1,  InputIdentities)) return;
            if (!DA.GetData(2, ref Pattern_Size)) return;
            if (!DA.GetData(3, ref inSize)) return;
            if (!DA.GetData(4, ref outSize)) return;
            if (!DA.GetData(5, ref Probabilistic)) return;
            if (!DA.GetData(6, ref Periodic)) return;
            if (!DA.GetData(7, ref Generate)) return;
            if (!DA.GetData(8, ref FixedCorners)) return;

            //We convert our list of boxes to voxels, a voxel has x,y,z coordinates and a value       
            for (int i = 0; i < InputBoxes.Count; i++)
            {
              int identity = InputIdentities[i];  
               Voxel vox = new Voxel((int)Math.Floor(InputBoxes[i].Center.X), (int)Math.Floor(InputBoxes[i].Center.Y), (int)Math.Floor(InputBoxes[i].Center.Z), identity);
               InputVoxels.Add(vox);
                 
            }

            var demo = new SimpleDemo(inSize, outSize, InputVoxels, Pattern_Size, Probabilistic, Periodic);

            string mes = " ";
            if (Generate)
            {

                demo.ClearModel();
                demo.GenerateOutput();
                mes =demo.message;

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

            // output the probabilities of each pattern in the input model
            Prob = new List<string>();
            foreach (KeyValuePair<int, double> kvp in demo.Model.probabilites)
            {
                var st =(kvp.Key.ToString() + "--->" + (Math.Truncate(1000* kvp.Value)/1000).ToString());
                Prob.Add(st);
            }
            DA.SetDataList(3, Prob);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
               
               return  Resource.im1;
            }
        }

 
        public override Guid ComponentGuid
        {
            get { return new Guid("887674AC-7B8D-4E16-88E5-AA6F5C3573CA"); }
        }
    }
   
}
