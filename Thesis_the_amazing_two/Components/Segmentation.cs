using System;
using System.Collections.Generic;
 using Grasshopper.Kernel;
using Rhino.Geometry;
using Thesis.Properties;

namespace Thesis.Components
{
    public class Segmentation : GH_Component
    {
       
        public Segmentation()
          : base("Segmentation", "Segmentation",
              "Segmentation of model to infer tileset.",
              "Thesis", "Segmentation")
        {
        }

      
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "Mesh", "the mesh to segment.", GH_ParamAccess.list);
            pManager.AddMeshParameter("Voxels", "Voxels", "the voxels.", GH_ParamAccess.list);
            pManager.AddNumberParameter("offset value", "OF", "The offset value affect the accuracy of the segmentation.", GH_ParamAccess.item);
        }
 
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Segments", "Segments", "The geometries generated from segmentation.", GH_ParamAccess.list);
        }

    
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Mesh> Mesh_Input = new List<Mesh>();
            List<Mesh> Voxels = new List<Mesh>();
            double offset = 0;

            if (!DA.GetDataList(0,Mesh_Input)) return;
            if (!DA.GetDataList(1, Voxels)) return;
            if (!DA.GetData(2,  ref offset)) return;
           
            for (int i = 0; i < Voxels.Count; i++)
            {
                Voxels[i].Weld(0.01);
            }
            var new_input = new List<Mesh>();
            new_input.Add(Mesh_Input[0].Offset(offset, true));


            var Segments_list = Mesh.CreateBooleanIntersection(new_input, Voxels);
            DA.SetDataList(0, Segments_list);
        }
 
        protected override System.Drawing.Bitmap Icon
        {
            get
            {                
                return Resource.seg;             
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("ddc1ab11-5ab2-46cb-abbe-54d9811f9b0d"); }
        }
    }
}