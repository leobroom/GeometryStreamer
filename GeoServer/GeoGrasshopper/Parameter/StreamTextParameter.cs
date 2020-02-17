using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace GeoGrasshopper
{
    public class StreamTextParameter : GH_PersistentParam<GH_StreamText>
    {
        public StreamTextParameter() : base("StreamText", "S-Text", "Text for Streaming", "Streaming", "Parameters") { }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override System.Drawing.Bitmap Icon => Resources.gs_param;
        public override Guid ComponentGuid => new Guid("{fbf3a281-86be-420e-b9f3-73b4d5b7e420}");
        protected override GH_GetterResult Prompt_Singular(ref GH_StreamText value)
        {
            value = new GH_StreamText();
            return GH_GetterResult.success;
        }
        protected override GH_GetterResult Prompt_Plural(ref List<GH_StreamText> values)
        {
            values = new List<GH_StreamText>();
            return GH_GetterResult.success;
        }
    }
}