using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Thesis.Help_classes;

namespace Thesis.Components
{
    public class ModelSynthesis3 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ModelSynthesis3()
         : base("Model Synthesis3", "Model Synthesis3",
              "does the model synthesis3",
              "Thesis", "Synthesis3")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            
            pManager.AddBoxParameter("Input Model", "IM", "The input model", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Input Identities", "I", "The values of voxels int the input model", GH_ParamAccess.list);
            pManager.AddNumberParameter("Input Values", "IV", "The values of voxels int the input model", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Pattern Size", "P", "The pattern size to infer from the input model", GH_ParamAccess.item);
            pManager.AddVectorParameter("Input Size", "IP", "Size in XYZ", GH_ParamAccess.item);
            pManager.AddVectorParameter("Output Size", "OP", "Size in XYZ", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Probabilistic", "PR", " ", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Periodic", "PE", " ", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Generate", "G", " ", GH_ParamAccess.item);
            pManager.AddBooleanParameter("OptimizedModel", "", " ", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Minimize", " ", " ", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Maximize", " ", " ", GH_ParamAccess.item);
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

        //List<Box> OutBoxes2 = new List<Box>();
        //List<int> OutValues = new List<int>();
        //List<string> Prob = new List<string>();
       
        //public static bool OptimizedModel;
        //public static bool Maximize;
        //public static bool Minimize;
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

        //    //We convert our list of boxes to voxels, a voxel has x,y,z coordinates and a value       
        //    for (int i = 0; i < InputBoxes.Count; i++)
        //    {
        //        int identity = InputIDS[i];
        //        double value = Inputvalues[i];

        //        Voxel vox = new Voxel((int)Math.Floor(InputBoxes[i].Center.X), (int)Math.Floor(InputBoxes[i].Center.Y), (int)Math.Floor(InputBoxes[i].Center.Z), identity, value);
        //        InputVoxels.Add(vox);

        //    }

        //    var demo = new SimpleDemo(inSize, outSize, InputVoxels, Pattern_Size, Probabilistic, Periodic,OptimizedModel,Maximize,Minimize);

        //    string mes = " ";
        //    if (Generate)
        //    {

        //        demo.ClearModel();
        //        demo.GenerateOutputandOptimize();
        //        mes = demo.message;

        //    }
        //    DA.SetData(4, mes);

        //    var Output_voxels = new List<Voxel>();

        //    if (demo.Model.GenerationFinished)
        //    {
        //        Output_voxels = demo.GetOutput();
        //    }

        //    //we get the output values of the model
        //    var rawOutput = demo.Model.GetOutput();

        //    if (Output_voxels.Count > 0)
        //    {
        //        OutValues = new List<int>();
        //        OutBoxes2 = new List<Box>();
        //        foreach (var v in Output_voxels)
        //        {
        //            var domain = new Interval(-0.5, 0.5);
        //            var plane = new Plane(new Point3d(((int)v.X) * 1.0, ((int)v.Y) * 1.0, ((int)v.Z) * 1.0), Vector3d.ZAxis);
        //            var b = new Box(plane, domain, domain, domain);
        //            OutBoxes2.Add(b);
        //            OutValues.Add(rawOutput[v.X, v.Y, v.Z]);

        //        }
        //    }


        //    DA.SetDataList(0, OutBoxes2);
        //    DA.SetDataList(1, OutValues);

        //    DA.SetData(2, demo.Model.patterns.Count());

        //    //we output the probabilities of each pattern in the input model
        //    Prob = new List<string>();
        //    foreach (KeyValuePair<int, double> kvp in demo.Model.probabilites)
        //    {
        //        var st = (kvp.Key.ToString() + "--->" + (Math.Truncate(1000 * kvp.Value) / 1000).ToString());
        //        Prob.Add(st);
        //    }
        //    DA.SetDataList(3, Prob);


        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("9bb44b77-1c6d-4a1e-9983-b48d10d0a335"); }
        }
    }
}