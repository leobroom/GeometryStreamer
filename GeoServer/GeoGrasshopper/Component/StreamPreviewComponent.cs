using System;
using System.Collections.Generic;
using System.Drawing;
using Grasshopper.Kernel;
using Rhino.Display;
using Rhino.Geometry;

namespace GeoGrasshopper.Component
{
    public class StreamPreviewComponent : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the StreamPreviewComponent class.
        /// </summary>
        public StreamPreviewComponent()
          : base("StreamPreviewComponent", "StreamPreview",
              "Description","Streaming", "Preview"){}

        private Dictionary<int, Tuple<Curve, double, Color>> curveTable = new Dictionary<int, Tuple<Curve, double, Color>>();
        private Dictionary<int, Tuple<Mesh, DisplayMaterial>> meshTable = new Dictionary<int, Tuple<Mesh, DisplayMaterial>>();


        private StreamSettings settings = new StreamSettings(System.Drawing.Color.Gray);

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGeometryParameter("Geometry", "Geo", "Streaming Geometry - Just Meshes and Curves are right now allowed", GH_ParamAccess.list);
            pManager.AddParameter(new StreamSettingsParameter(), "Settings", "Set", "Material and other Settings", GH_ParamAccess.item);

            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) { }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_StreamSettings settingsGH = null;
           settings = (DA.GetData(1, ref settingsGH)) ?
                settingsGH?.Value : StreamSettings.Default;

            List<GeometryBase> geometry = new List<GeometryBase>();
            if (!DA.GetDataList(0, geometry))
                geometry = new List<GeometryBase>();

            curveTable.Clear();
            meshTable.Clear();

            for (int id = 0; id < geometry.Count; id++)
            {
                var geo = geometry[id];

                if (geo is Curve)
                {
                    double width = Send.GetSettingsValue(id, settings.CurveWidths, "no Curve width");
                    Color color = Send.GetSettingsMaterial(id, settings).Diffuse;
                    var data = new Tuple<Curve, double, Color>((Curve)geo, width, color);
                    curveTable.Add(id, data);
                }
                else if (geo is Mesh)
                {
                    DisplayMaterial material = Send.GetSettingsMaterial(id, settings);
                    var data = new Tuple<Mesh, DisplayMaterial>((Mesh)geo, material);
                    meshTable.Add(id, data);
                }
            }
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            foreach (var curveData in curveTable.Values)
            {
                Curve curve = curveData.Item1;
                double width = curveData.Item2;
                Color color = curveData.Item3;

                args.Display.DrawCurve(curve, color, (int)(width*100));
            }

            foreach (var meshData in meshTable.Values)
            {
                Mesh mesh = meshData.Item1;
                DisplayMaterial material = meshData.Item2;

                args.Display.DrawMeshShaded(mesh, material);
            }
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
            get { return new Guid("ee685fdb-3f1c-47b5-a714-f7b4223d70cc"); }
        }
    }
}