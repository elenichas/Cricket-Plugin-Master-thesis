using System;
using System.Collections.Generic;
 using Grasshopper.Kernel;
using Rhino.Geometry;
 

namespace Thesis.Components
{
    public class Segmentation : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Segmentation()
          : base("Segmentation", "Segmentation",
              "Segment model to infer tileset",
              "Thesis", "Segmentation")
        {
        }

      
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "the mesh to segment", GH_ParamAccess.list);
            pManager.AddBoxParameter("Voxels", "Voxels", "the voxels", GH_ParamAccess.list);
            pManager.AddNumberParameter("offset value", "OF", " ", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Chuncks", "Chuncks", "the tileset", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Mesh> Mesh_Input = new List<Mesh>();
            List<Mesh> Voxels = new List<Mesh>();
            double offset = 0;

            if (!DA.GetData(0, ref Mesh_Input)) return;
            if (!DA.GetDataList(1, Voxels)) return;
            if (!DA.GetData(2,  ref offset)) return;
           
            for (int i = 0; i < Voxels.Count; i++)
            {
                Voxels[i].Weld(0.01);
            }
            var new_input = new List<Mesh>();
            new_input.Add(Mesh_Input[0].Offset(offset, true));


            var Chuncks_list = Mesh.CreateBooleanIntersection(new_input, Voxels);
            DA.SetData(3, Chuncks_list);
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
            get { return new Guid("ddc1ab11-5ab2-46cb-abbe-54d9811f9b0d"); }
        }
    }
}