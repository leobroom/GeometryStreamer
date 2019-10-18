using System;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino.DocObjects;

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

        Rhino.RhinoDoc doc;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            doc = Rhino.RhinoDoc.ActiveDoc;

            pManager.AddParameter(new Param_OGLShader(), "Mesh Material", "MeshMat", "Material for Streaming Meshes", GH_ParamAccess.item);
            pManager.AddParameter(new Param_OGLShader(), "Curve Material", "CrvMat", "Material for Streaming Curves", GH_ParamAccess.item);
            pManager.AddNumberParameter("Curve Division", "CrvDiv", "The curve Intersection", GH_ParamAccess.item);
            pManager.AddNumberParameter("Curve Width", "CrvW", "The curve width", GH_ParamAccess.item);

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
            StreamSettings settings = new StreamSettings();

            Material meshMat = GetMaterial(0, DA);
            if (meshMat!= null)
                settings.MeshMaterial = meshMat;

            Material curveMat = GetMaterial(1, DA);
            if (curveMat != null)
                settings.CurveMaterial = curveMat;

            double curveDivision = -1;
            if (DA.GetData(2, ref curveDivision))
                settings.CurveDivision = curveDivision;

            double curveWidth = -1;
            if (DA.GetData(3, ref curveWidth))
                settings.CurveWidth = (float)curveWidth;

            DA.SetData(0, settings);
        }

        private Material GetMaterial(int idx, IGH_DataAccess DA)
        {
            GH_Material mat_GH = null;
            Material mat = null;

            if (DA.GetData(idx, ref mat_GH))
            {
                Guid id = mat_GH.RdkMaterialId;
                mat = doc.Materials.FindId(id);
            }

            return mat;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.bitmap;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7b71e06a-0725-44d7-b3d7-2485a9c2508c"); }
        }
    }
}