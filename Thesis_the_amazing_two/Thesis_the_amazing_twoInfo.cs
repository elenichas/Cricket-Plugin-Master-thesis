using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace Thesis
{
    public class Thesis_the_amazing_twoInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "Thesistheamazingtwo";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("215d6eb6-4e5a-4766-a86b-fe5376831dc6");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
