using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System.Collections.Generic;

namespace GeoGrasshopper
{
    public class FiberWinding
    {
        private static FiberWinding _default = new FiberWinding() { };

        private List<Plane> weavingPlanes = new List<Plane>();
        private GH_Structure<GH_Plane> arcPlanes = new GH_Structure<GH_Plane>();
        private GH_Structure<GH_Plane> bendingPlanes = new GH_Structure<GH_Plane>();
        private Plane start = new Plane();
        private Plane end = new Plane();

        public FiberWinding() { }
        public FiberWinding(List<Plane> weavingPlanes, GH_Structure<GH_Plane> arcPlanes, GH_Structure<GH_Plane> bendingPlanes, Plane start, Plane end)
        {
            this.weavingPlanes = weavingPlanes;
            this.arcPlanes = arcPlanes;
            this.bendingPlanes = bendingPlanes;
            this.start = start;
            this.end = end;
        }

        public GH_Structure<GH_Plane> ArcPlanes
        {
            get { return arcPlanes; }
            set { arcPlanes = value; }
        }

        public List<Plane> WeavingPlanes
        {
            get { return weavingPlanes; }
            set { weavingPlanes = value; }
        }

        public GH_Structure<GH_Plane> Bendingplanes
        {
            get { return bendingPlanes; }
            set { bendingPlanes = value; }
        }

        public Plane StartPlane
        {
            get { return start; }
            set { start = value; }
        }

        public Plane EndPlane
        {
            get { return end; }
            set { end = value; }
        }

        public static FiberWinding Default
        {
            get { return _default; }
            set { _default = value; }
        }
    }
}