using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Thesis_1.Help_classes;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel.Attributes;


namespace Thesis_1
{
    public class ModelSynthesisColors : GH_Component 
    {

        public override void CreateAttributes()
        {
            m_attributes = new CustomAttributes(this);
        }
        public ModelSynthesisColors()
          : base("Model SynthesisColors", "Model SynthesisColors",
              "for colors",
              "Thesis_1", "Colors")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {           
            pManager.AddBoxParameter("Input Model", "IM", "The input model", GH_ParamAccess.list);
            pManager.AddColourParameter("Input Colors", "IC", "The colors of voxels int the input model", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Pattern Size", "P", "The pattern size to infer from the input model", GH_ParamAccess.item);
            pManager.AddVectorParameter("Input Size", "IP", "Size in XYZ", GH_ParamAccess.item);
            pManager.AddVectorParameter("Output Size", "OP", "Size in XYZ", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Probabilistic", "PR", " ", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Periodic", "PE", " ", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Generate", "G", "Generate?", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBoxParameter("Output Model", "OM", "The output model", GH_ParamAccess.list);
            pManager.AddColourParameter("Output Colors", "OC", "The output colors", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Patterns", "P", "The number of patters", GH_ParamAccess.item);
            pManager.AddGenericParameter("Probabilities", "P", "The probabilities", GH_ParamAccess.list);
        }


        List<Box> OutBoxes2 = new List<Box>();
        List<Color> OutColors = new List<Color>();
        List<string> Prob = new List<string>();


        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Box> InputBoxes = new List<Box>();
            List<Voxel> InputVoxels = new List<Voxel>();
            List<Color> InputColors = new List<Color>();

            Vector3d inSize = new Vector3d();
            Vector3d outSize = new Vector3d();

            bool Generate = false;          
            bool Probabilistic = true;
            bool Periodic = true;
           
            int Pattern_Size = 0;

            if (!DA.GetDataList(0,  InputBoxes)) return;
            if (!DA.GetDataList(1,  InputColors)) return;
            if (!DA.GetData(2, ref Pattern_Size)) return;
            if (!DA.GetData(3, ref inSize)) return;
            if (!DA.GetData(4, ref outSize)) return;
            if (!DA.GetData(5, ref Probabilistic)) return;
            if (!DA.GetData(6, ref Periodic)) return;
            if (!DA.GetData(7, ref Generate)) return;

            //We convert our list of boxes to voxels, a voxel has x,y,z coordinates and a color
            for (int i = 0; i < InputBoxes.Count; i++)
            {
               Color col = InputColors[i];              
               Voxel vox = new Voxel((int)Math.Floor(InputBoxes[i].Center.X), (int)Math.Floor(InputBoxes[i].Center.Y), (int)Math.Floor(InputBoxes[i].Center.Z), col);
               InputVoxels.Add(vox);
                 
            }

            var demo = new SimpleDemo(inSize, outSize, InputVoxels, Pattern_Size, Probabilistic, Periodic);
            
            if (Generate)
            {

                demo.ClearModel();
                demo.GenerateOutput();
                Rhino.RhinoApp.WriteLine(demo.Model.GenerationFinished.ToString());
            }
            var Output_voxels = new List<Voxel>();
           
            if (demo.Model.GenerationFinished)
            {
                Output_voxels = demo.GetOutput();
            }

            //I will get the colors form this one
            var rawOutput = demo.Model.GetOutput();

            if (Output_voxels.Count > 0)
            {
                OutColors = new List<Color>();
                OutBoxes2 = new List<Box>();
                foreach (var v in Output_voxels)
                {
                    var domain = new Interval(-0.5, 0.5);
                    var plane = new Plane(new Point3d(((int)v.X) * 1.0, ((int)v.Y) * 1.0, ((int)v.Z) * 1.0), Vector3d.ZAxis);
                    var b = new Box(plane, domain, domain, domain);
                    OutBoxes2.Add(b);
                   OutColors.Add(rawOutput[v.X, v.Y, v.Z]);
                    
                }
            }


            DA.SetDataList(0, OutBoxes2);
            DA.SetDataList(1, OutColors);

            DA.SetData(2, demo.Model.patterns.Count());

            Prob = new List<string>();
            foreach (KeyValuePair<int, double> kvp in demo.Model.probabilites)
            {
                var st =(kvp.Key.ToString() + "--->" + (Math.Truncate(1000* kvp.Value)/1000).ToString());
                Prob.Add(st);
            }
            DA.SetDataList(3, Prob);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ccb5823e-690e-4cb7-ac6d-dd5bfc54fa55"); }
        }
    }
    //the customAttributes class changed the color of my GA component to pink :)
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
