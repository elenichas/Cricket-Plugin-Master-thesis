﻿using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Thesis.Properties;

namespace Thesis.Components
{
    public class Decoder : GH_Component
    {

        public Decoder()
          : base("Decoder", "Decoder",
              "Decodes the values given to get the final geometry placed in the output model",
              "Thesis", "Encode-Decode")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Output Values", "OV", "The code values of the output model", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Encoded List", "EL", "The list of geometries encoded based the index they exist in UBC", GH_ParamAccess.list);
            pManager.AddBrepParameter("Input Voxels", "IV", "The input voxels", GH_ParamAccess.list);
            pManager.AddMeshParameter("Input Geometries", "IG", "The input geometries contained in the voxels", GH_ParamAccess.list);
        }

 
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Output Geometries", "OG", "All the tiles existing in the output model", GH_ParamAccess.list);

            pManager.AddPointParameter("Voxel Centers", "GC", "The centers of the voxels of every geometry in the output model", GH_ParamAccess.list);
     
        }


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<int> OutputValues = new List<int>();
            List<int> EncodedList = new List<int>();

            List<Brep> InputVoxels = new List<Brep>();
            List<Mesh> InputChunks = new List<Mesh>();

            if (!DA.GetDataList(0, OutputValues)) return;
            if (!DA.GetDataList(1, EncodedList)) return;
            if (!DA.GetDataList(2, InputVoxels)) return;
            if (!DA.GetDataList(3, InputChunks)) return;

            //the items that appear in the output value
            var unique_items = new HashSet<int>(OutputValues);
            List<int> Unique = unique_items.ToList();

            var Indices = new List<int>();
            foreach (int code in Unique)
            {
                Indices.Add(EncodedList.IndexOf(code));

            }

            var Meshes = new List<Mesh>();
            var voxels = new List<Brep>();
            foreach (int index in Indices)
            {
                Meshes.Add(InputChunks[index]);
                voxels.Add(InputVoxels[index]);

            }


            var Output_Meshes = new List<Mesh>();
            var Output_Centers = new List<Point3d>();


            for (int i = 0; i < OutputValues.Count; i++)
            {
                for (int j = 0; j < Unique.Count; j++)
                {
                    if (OutputValues[i] == Unique[j])
                    {
                        Output_Meshes.Add(Meshes[j]);
                        Output_Centers.Add(voxels[j].GetBoundingBox(true).Center);
                    }
                }
            }
            DA.SetDataList(0, Output_Meshes);
            DA.SetDataList(1, Output_Centers);


        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                 return Resource.one;
                //return null;
            }
        }


        public override Guid ComponentGuid
        {
            get { return new Guid("b0ceaadb-52c4-4cf3-aba5-56fd24ee2b1d"); }
        }
    }
}