using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace GeoGrasshopper
{
    public class FiberWindingParameter : GH_PersistentParam<GH_FiberWinding>
    {
        public FiberWindingParameter() : base("FiberWinding", "FW", "FiberWindingData", "ITE", "Parameters") { }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override System.Drawing.Bitmap Icon => Resources.gs_param;
        public override Guid ComponentGuid => new Guid("{5875a079-1289-4c53-ad96-a4d9a992ca72}");
        protected override GH_GetterResult Prompt_Singular(ref GH_FiberWinding value)
        {
            value = new GH_FiberWinding();
            return GH_GetterResult.success;
        }
        protected override GH_GetterResult Prompt_Plural(ref List<GH_FiberWinding> values)
        {
            values = new List<GH_FiberWinding>();
            return GH_GetterResult.success;
        }
    }
}