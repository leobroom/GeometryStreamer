using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace GeometryStreamer
{
    public class GeometryStreamerInfo : GH_AssemblyInfo
    {
        public override string Name => "GeometryStreamer";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override System.Drawing.Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new("7f6ce6c6-b5c8-44f8-9c08-763a25cf1399");

        //Return a string identifying you or your company.
        public override string AuthorName => "Leon Brohmann";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}