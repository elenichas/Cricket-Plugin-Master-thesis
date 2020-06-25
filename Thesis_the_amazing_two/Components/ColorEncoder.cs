using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Rhino.Geometry;

namespace Thesis
{
    public class ColorEncoder : GH_Component
    {
        public override void CreateAttributes()
        {
            m_attributes = new CustomAttributes(this);
        }
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ColorEncoder()
          : base("CEncoder", "CEncoder",
              "does color encoding",
              "Thesis", "Encoding")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("Input Colors", "IC", "The voxels' colors in the input model", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Unique Elements", "UE", "The unique colors in the input list", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Encoded List", "EL", "The encoded version of the colorlist", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Color> InputColors= new List<Color>();
            List<string> Unique = new List<string>();
            List<int> Encoded = new List<int>();


            if (!DA.GetDataList(0, InputColors)) return;
           
            Unique = UniqueC(InputColors);
            Encoded = Encode(InputColors, Unique);

            DA.SetData(0,Unique);
            DA.SetData(1, Encoded);

        }

        public List <string> UniqueC(List<Color> InputColors )
        {
            //get the unique items from the color list
            var unique_items = new HashSet<Color>(InputColors);
 
            //create an array with the unique colors
            var  unique_array = new Color[unique_items.Count + 1];
            unique_items.CopyTo(unique_array, 1);
            var unique_list = unique_array.ToList();

            var strings = new List<string>();
            foreach (var item in unique_list)
            {
                strings.Add(item.ToArgb().ToString());
            }




            return strings;
        }
        public List<int> Encode(List<Color> InputColors,List<string> unique_list)
        {
            //encode the list of colors based on the indices in the unique array
            var encoded_list = new List<int>();
            for (int i = 0; i < InputColors.Count; i++)
            {
                for (int j = 0; j < unique_list.Count; j++)
                {
                    if (unique_list[j] == InputColors[i].ToString())
                    {
                        encoded_list.Add(j);
                    }
                }
            }
            return encoded_list;
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
            get { return new Guid("8c7b12ab-8c53-4743-98d8-6baae3c656b0"); }
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
 