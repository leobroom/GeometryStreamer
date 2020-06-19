using Grasshopper.Kernel.Types;

namespace GeoGrasshopper
{
    public class GH_FiberWinding : GH_Goo<FiberWinding>
    {
        public GH_FiberWinding() { this.Value = FiberWinding.Default; }
        public GH_FiberWinding(GH_FiberWinding goo) { this.Value = goo.Value; }
        public GH_FiberWinding(FiberWinding native) { this.Value = native; }
        public override IGH_Goo Duplicate() => new GH_FiberWinding(this);
        public override bool IsValid => true;
        public override string TypeName => "StreamSettings";
        public override string TypeDescription => "Geometry StreamSettings";
        public override string ToString() => this.Value?.ToString();
        public override object ScriptVariable() => Value;

        public override bool CastFrom(object source)
        {
            if (source is FiberWinding)
            {
                Value = source as FiberWinding;
                return true;
            }
            return false;
        }
    }
}