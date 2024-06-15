using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace GeoGrasshopper.Component.FiberWinder
{
    public class ConstructFiberWinding : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public ConstructFiberWinding()
            : base("Construct FiberWinding", "Con FW", "Construct a FiberwindingComp.", "ITE", "Tools")
        { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("WeavingPlanes", "WP", "WeavingPlanes", GH_ParamAccess.list);
            pManager.AddPlaneParameter("ArcPlanes", "ARC", "ArcPlanes", GH_ParamAccess.tree);
            pManager.AddPlaneParameter("Bendingplanes", "BP", "Bendingplanes Planes", GH_ParamAccess.tree);
            pManager.AddPlaneParameter("StartPlane", "SP", "Start Plane", GH_ParamAccess.item);
            pManager.AddPlaneParameter("EndPlane", "EP", "End Plane", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("RobotPlanes", "P", "Planes for RobotControl", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Plane> robotPlanes = new List<Plane>();
            List<Plane> weavingPlanes = new List<Plane>();
            GH_Structure<GH_Plane> arcPlaneTree;
            GH_Structure<GH_Plane> bendingPlaneTree;
            Plane start = new Plane();
            Plane end = new Plane();

            if (!DA.GetDataList(0, weavingPlanes))
                return;

            if (!DA.GetDataTree(1, out arcPlaneTree))
                return;

            if (!DA.GetDataTree(2, out bendingPlaneTree))
                return;

            if (!DA.GetData(3, ref start))
                return;

            if (!DA.GetData(4, ref end))
                return;

            //Count Test
            int count = weavingPlanes.Count;
            if (count != arcPlaneTree.PathCount)
                throw new Exception("weavingPlanes Count and the Branchcount of the arcPlanes have to be the same");

            robotPlanes.Add(start);

            for (int i = 0; i < count; i++)
            {
                List<GH_Plane> planes = arcPlaneTree[i];

                for (int p = 0; p < planes.Count; p++)
                    robotPlanes.Add(planes[p].Value);

                if (i >= bendingPlaneTree.PathCount)
                    continue;

                planes = bendingPlaneTree[i];

                for (int p = 0; p < planes.Count; p++)
                    robotPlanes.Add(planes[p].Value);
            }

            robotPlanes.Add(end);

            DA.SetDataList(0, robotPlanes);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("25d79c29-892c-4bc9-9ef3-42f09256374b");
    }
}