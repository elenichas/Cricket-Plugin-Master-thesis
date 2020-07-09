using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Thesis.Components
{
    public class Decoder : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Decoder()
          : base("Decoder", "Decoder",
              "decodes the values given to get the final geometry placed in the output model",
              "Thesis", "Encode-Decode")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Output Model", "OM", "The voxels in the output model", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Output Values", "OV", "The values associated with the voxels in the output model", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Output Unique", "OU", "All the unique tile codes found in the output model", GH_ParamAccess.list);
            pManager.AddMeshParameter("Meshes Collection", "MF", "All the unique tiles as meshes found in the output model", GH_ParamAccess.list);
            pManager.AddBrepParameter("Voxels Collection", "VF", "All the unique voxels of the tiles found in the output model", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Output Geometries", "OG", "All the tiles existing in the output model", GH_ParamAccess.list);
            
            pManager.AddPointParameter("Voxels' Centers", "GC", "The centers of the voxels of every geometry in the output model", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var Output_Model = new List<Brep>();
            
            var Output_Values = new List<int>();
            var Output_Unique = new List<int>();

            var Meshes_Collection = new List<Mesh>();
            var Voxels_Collection = new List<Brep>();

            if (!DA.GetDataList(0, Output_Model)) return;
            if (!DA.GetDataList(1, Output_Values)) return;
            if (!DA.GetDataList(2, Output_Unique)) return;
            if (!DA.GetDataList(3, Meshes_Collection)) return;
            if (!DA.GetDataList(4, Voxels_Collection)) return;

            var  Output_Meshes = new List<Mesh>();
            var  Output_Centers = new List<Point3d>();

           
            
            for (int i = 0; i < Output_Values.Count; i++)
            {
                for (int j = 0; j < Output_Unique.Count; j++)
                {
                    if (Output_Values[i] == Output_Unique[j])
                    {
                        Output_Meshes.Add(Meshes_Collection[j]);
                        Output_Centers.Add(Voxels_Collection[j].GetBoundingBox(true).Center);
                    }
                }
            }
            DA.SetDataList(0, Output_Meshes);
            DA.SetDataList(1, Output_Centers);
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
            get { return new Guid("065dfc12-efe4-440b-8ee2-ccecd5c5bc2a"); }
        }
    }
}