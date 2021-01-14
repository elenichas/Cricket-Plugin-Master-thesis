using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using Thesis.Properties;

namespace Thesis
{
    public class Encoder : GH_Component
    {
       
        public Encoder()
          : base("Encoder", "Encoder",
              "does the encoding",
              "Thesis", "Encode-Decode")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Input Voxels", "IV", "The input voxels.", GH_ParamAccess.list);
            pManager.AddMeshParameter("Input Geometries", "IG", "The geometries contained in each voxel.", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddTextParameter("Binary Codes", "BC", "The  binary encoding of each voxel.", GH_ParamAccess.list);
           // pManager.AddTextParameter("Unique Binary Codes", "UBC", "The  unique binary codes found.", GH_ParamAccess.list);
            pManager.AddTextParameter("Encoded List", "EL", "The generated codes of every geometry.", GH_ParamAccess.list);
        }

  
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Brep> InputVoxels = new List<Brep>();
            List<Mesh> InputChunks = new List<Mesh>();

            if (!DA.GetDataList(0, InputVoxels)) return;
            if (!DA.GetDataList(1, InputChunks)) return;
         
            var OutputCodes = Encode(InputVoxels, InputChunks);
           // DA.SetDataList(0, OutputCodes);

            var Uniques = FindUnique(OutputCodes);
           // DA.SetDataList(1, Uniques);

            var IntegersList = MakeIntList(Uniques, OutputCodes);
            DA.SetDataList(0, IntegersList);
        }

        public List<int> MakeIntList(string[] unique_array,List<string> OutputCodes)
        {
            //encode the list of geometries based on the indices in the unique array
            var encoded_list = new List<int>();
            for (int i = 0; i < OutputCodes.Count; i++)
            {
                for (int j = 0; j < unique_array.Length; j++)
                {
                    if (unique_array[j] == OutputCodes[i])
                    {
                        encoded_list.Add(j);

                    }
                }
            }
            return encoded_list;
        }
        public string[] FindUnique(List<string> OutputCodes)
        {
            //get the unique items from the code list
            var unique_items = new HashSet<string>(OutputCodes);

            //create an array with the unique strings
            string[] unique_array = new string[unique_items.Count + 1];
            unique_items.CopyTo(unique_array, 1);
            return unique_array;
        }

        public List<string> Encode(List<Brep> InputVoxels,List <Mesh> InputGeometries)
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
                        if (!(InputGeometries[i] == null))
                            ClosestPts.Add(InputGeometries[i].ClosestPoint(DivTree.Branch(m)[e]));
                        else
                            ClosestPts.Add(new Point3d());
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

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                return Resource.zero;
               // return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("e4eb62d9-7de9-48b3-84b2-3a7236ccae67"); }
        }

         
    }
}