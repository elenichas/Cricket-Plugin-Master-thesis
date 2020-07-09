using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Thesis.Components
{
    public class Collection : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Decoder class.
        /// </summary>
        public Collection()
          : base("Collection", "Collection",
              "Gives the collection of existing tiles in the output model, their coed and their voxels",
              "Thesis", "Encode-Decode")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Output Values", "OV", "The code values of the output model", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Encoded List", "EL", "The list of chuncks encoded based the index they exist in UBC", GH_ParamAccess.list);
            pManager.AddBrepParameter("Input Voxels", "IV", "The input voxels", GH_ParamAccess.list);
            pManager.AddMeshParameter("Input Chuncks", "ICH", "The input chuncks of geometry contained in the voxels", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Output Unique", "OU", "All the unique tile codes found in the output model", GH_ParamAccess.list);
            pManager.AddMeshParameter("Meshes Collection", "MF", "All the unique tiles as meshes found in the output model", GH_ParamAccess.list);
            pManager.AddBrepParameter("Voxels Collection", "VF", "All the unique voxels of the tiles found in the output model", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
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

            DA.SetDataList(0, Unique);
           

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

            DA.SetDataList(1, Meshes);
            DA.SetDataList(2, voxels);
           
          
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
            get { return new Guid("b0ceaadb-52c4-4cf3-aba5-56fd24ee2b1d"); }
        }
    }
}