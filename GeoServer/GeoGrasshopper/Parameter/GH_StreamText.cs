using Grasshopper.Kernel.Types;

namespace GeoGrasshopper
{
    public class GH_StreamText : GH_Goo<StreamText>
    {
        public GH_StreamText() { this.Value = StreamText.Default; }
        public GH_StreamText(GH_StreamText goo) { this.Value = goo.Value; }
        public GH_StreamText(StreamText native) { this.Value = native; }
        public override IGH_Goo Duplicate() => new GH_StreamText(this);
        public override bool IsValid => true;
        public override string TypeName => "StreamSettings";
        public override string TypeDescription => "Geometry StreamSettings";
        public override string ToString() => this.Value?.ToString();
        public override object ScriptVariable() => Value;

        public override bool CastFrom(object source)
        {
            if (source is StreamSettings)
            {
                Value = source as StreamText;
                return true;
            }
            return false;
        }
    }
}