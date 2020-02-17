using Grasshopper.Kernel.Types;

namespace GeoGrasshopper
{
    public class GH_StreamSettings : GH_Goo<StreamSettings>
    {
        public GH_StreamSettings() { this.Value = StreamSettings.Default; }
        public GH_StreamSettings(GH_StreamSettings goo) { this.Value = goo.Value; }
        public GH_StreamSettings(StreamSettings native) { this.Value = native; }
        public override IGH_Goo Duplicate() => new GH_StreamSettings(this);
        public override bool IsValid => true;
        public override string TypeName => "StreamSettings";
        public override string TypeDescription => "Geometry StreamSettings";
        public override string ToString() => this.Value?.ToString();
        public override object ScriptVariable() => Value;

        public override bool CastFrom(object source)
        {
            if (source is StreamSettings)
            {
                Value = source as StreamSettings;
                return true;
            }
            return false;
        }
    }
}