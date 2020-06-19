using System;
using Grasshopper.Kernel;

namespace GeoGrasshopper.Component.FiberWinder
{
    public class DeconstructFiberWinding : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public DeconstructFiberWinding()
          : base("Deconstruct FiberWinding", "Dec FW", "DEconstruct a FiberwindingComp.", "ITE", "Tools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new FiberWindingParameter(), "FiberWinding", "FW", "A FiberWinding", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("WeavingPlanes", "WP", "WeavingPlanes", GH_ParamAccess.list);
            pManager.AddPlaneParameter("ArcPLanes", "AP", "ArcPLanes", GH_ParamAccess.tree);
            pManager.AddPlaneParameter("Bendingplanes", "BP", "Bendingplanes Planes", GH_ParamAccess.tree);
            pManager.AddPlaneParameter("StartPlane", "SP", "Start Plane", GH_ParamAccess.list);
            pManager.AddPlaneParameter("EndPlane", "EP", "End Plane", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_FiberWinding winding = new GH_FiberWinding();

            if (!DA.GetData(0, ref winding))
                return;

            DA.SetDataList(0, winding.Value.WeavingPlanes);
            DA.SetDataTree(1, winding.Value.ArcPlanes);
            DA.SetDataTree(2, winding.Value.Bendingplanes);
            DA.SetData(3, winding.Value.StartPlane);
            DA.SetData(4, winding.Value.EndPlane);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8eec68b2-be31-4225-ae01-8661527e97dc"); }
        }
    }
}