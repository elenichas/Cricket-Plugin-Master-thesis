﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

namespace Thesis
{
    public class Encoder : GH_Component
    {
        public override void CreateAttributes()
        {
            m_attributes = new CustomAttributes(this);
        }
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Encoder()
          : base("Encoder", "Encoder",
              "does the encoding",
              "Thesis", "Encoding")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Input Voxels", "IV", "The input voxels", GH_ParamAccess.list);
            pManager.AddMeshParameter("Input Chuncks", "ICH", "The input chuncks of geometry contained in the voxels", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Binary Code", "BC", "The  binary encoding of each voxel", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> InputVoxels = new List<Brep>();
            List<Mesh> InputChunks = new List<Mesh>();


            if (!DA.GetDataList(0, InputVoxels)) return;
            if (!DA.GetDataList(1, InputChunks)) return;

           
            
            var OutputCodes = Encode(InputVoxels, InputChunks);
            DA.SetDataList(0, OutputCodes);
        }

        public List<string> Encode(List<Brep> InputVoxels,List <Mesh> InputChunks)
        {
            List<string> Codes = new List<string>();
           
            double num0 = 0;
           
            for (int i = 0; i < InputVoxels.Count; i++)
            {
                //make new encoding strings
                String code = "";
              


                //make a new tree for every voxel
                DataTree<Point3d> DivTree = new DataTree<Point3d>();
                DataTree<Point3d> ClosestTree = new DataTree<Point3d>();
                DataTree<int> BoolsTree = new DataTree<int>();

                List<Surface> faces = new List<Surface>();
                faces.AddRange(InputVoxels[i].Surfaces);

                for (int j = 0; j < faces.Count; j++)
                {
                    //one list for every face
                    List<Point3d> DivPts = new List<Point3d>();

                    var dom0 = faces[j].Domain(0).Length;
                    var dom1 = faces[j].Domain(1).Length;
                    num0 = dom0 / 9;
                    double num1 = dom1 / 9;

                    var myU = new Interval(faces[j].Domain(0).Min, faces[j].Domain(0).Max);
                    var myV = new Interval(faces[j].Domain(1).Min, faces[j].Domain(1).Max);

                    for (int k = 0; k < 10; k++)
                    {
                        for (int l = 0; l < 10; l++)
                        {
                            Point3d temp = faces[j].PointAt(myU[0] + (num0 * k), myV[0] + (num1 * l));
                            DivPts.Add(temp);
                        }
                    }
                    DivTree.AddRange(DivPts, new GH_Path(new int[] { j, 0 }));
                }

                //mesh intersection between the chunck and the points of the faces
                for (int m = 0; m < DivTree.BranchCount; m++)
                {
                    List<Point3d> ClosestPts = new List<Point3d>();
                    for (int e = 0; e < DivTree.Branch(m).Count; e++)
                    {
                        ClosestPts.Add(InputChunks[i].ClosestPoint(DivTree.Branch(m)[e]));
                    }
                    ClosestTree.AddRange(ClosestPts, new GH_Path(new int[] { m, 0 }));
                }

                //Create the true false list
                for (int p = 0; p < DivTree.BranchCount; p++)
                {
                    List<int> Bools = new List<int>();
                    for (int q = 0; q < DivTree.Branch(p).Count; q++)
                    {
                        if (DivTree.Branch(p)[q].DistanceTo(ClosestTree.Branch(p)[q]) <= (num0 / 2))
                            Bools.Add(1);
                        else
                            Bools.Add(0);
                    }
                    BoolsTree.AddRange(Bools, new GH_Path(new int[] { p, 0 }));
                }


                for (int x = 0; x < BoolsTree.BranchCount; x++)
                {

                    for (int z = 0; z < BoolsTree.Branch(x).Count; z++)
                    {
                        code += BoolsTree.Branch(x)[z];
                    }
                }
                Codes.Add(code);

            }
            return Codes;

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
            get { return new Guid("e4eb62d9-7de9-48b3-84b2-3a7236ccae67"); }
        }

        public class CustomAttributes : GH_ComponentAttributes
        {
            public CustomAttributes(IGH_Component component)
              : base(component)
            { }

            protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
            {
                if (channel == GH_CanvasChannel.Objects)
                {
                    // Cache the existing style.
                    GH_PaletteStyle style = GH_Skin.palette_normal_standard;

                    // Swap out palette for normal, unselected components.
                    GH_Skin.palette_normal_standard = new GH_PaletteStyle(Color.Pink, Color.Black, Color.Black);

                    base.Render(canvas, graphics, channel);

                    // Put the original style back.
                    GH_Skin.palette_normal_standard = style;
                }
                else
                    base.Render(canvas, graphics, channel);
            }
        }
    }
}