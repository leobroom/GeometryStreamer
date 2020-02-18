using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

namespace GeoGrasshopper
{
    public class StreamSettingsParameter : GH_PersistentParam<GH_StreamSettings>
    {
        public StreamSettingsParameter() : base("StreamSettings", "Settings", "Settings", "ITE", "Parameters") { }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override System.Drawing.Bitmap Icon => Resources.gs_param;
        public override Guid ComponentGuid => new Guid("{eed378e7-9e25-4d0c-8f92-5ababf33111b}");
        protected override GH_GetterResult Prompt_Singular(ref GH_StreamSettings value)
        {
            value = new GH_StreamSettings();
            return GH_GetterResult.success;
        }
        protected override GH_GetterResult Prompt_Plural(ref List<GH_StreamSettings> values)
        {
            values = new List<GH_StreamSettings>();
            return GH_GetterResult.success;
        }
    }
}