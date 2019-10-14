using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace GeoGrasshopper
{
    public class GeoGrasshopperInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "GeoGrasshopper";
            }
        }
        public override Bitmap Icon
        {
            get
            {
          
                return null;
                //Return a 24x24 pixel bitmap to represent this GHA library.

            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "Hopefully it will work";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("43beed8c-65c3-4700-a514-7024d00e58ff");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "Leon Brohmann";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "leonbrohmann@gmx.de";
            }
        }
    }
}
