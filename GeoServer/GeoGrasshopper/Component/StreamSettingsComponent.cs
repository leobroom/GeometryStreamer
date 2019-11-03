using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Display;

namespace GeoGrasshopper
{
    public class StreamSettingsComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public StreamSettingsComponent()
          : base("StreamSettings", "GS-Settings",
                "Settings for the Geometry Streamer", "Streaming", "Network")
        { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddParameter(new Param_OGLShader(), "Material", "Mat", "Material for Streaming Objects", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Object Material Ids", "MatID", "Object Material Ids", GH_ParamAccess.list);
            pManager.AddNumberParameter("Curve Division", "CrvDiv", "The curve Intersection", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Curve Width", "CrvW", "The curve width", GH_ParamAccess.list);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddParameter(new StreamSettingsParameter(), "Settings", "S", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            StreamSettings settings = new StreamSettings(System.Drawing.Color.Gray);

            List<DisplayMaterial> mat = new List<DisplayMaterial>();
            if (DA.GetDataList(0, mat))
                settings.Materials = mat;

            List<int> objIds = new List<int>();
            if (DA.GetDataList(1, objIds))
                settings.ObjMatIds = objIds;

            List<double> curveDivisions = new List<double>();
            if (DA.GetDataList(2, curveDivisions))
                settings.CurveDivisions = curveDivisions;

            List<int> curveWidths = new List<int>();
            if (DA.GetDataList(3, curveWidths))
                settings.CurveWidths = curveWidths;

            DA.SetData(0, settings);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.gs_settings;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7b71e06a-0725-44d7-b3d7-2485a9c2508c"); }
        }
    }
}