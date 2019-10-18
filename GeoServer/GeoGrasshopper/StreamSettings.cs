using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;

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

    public class StreamSettings
    {
        private static StreamSettings _default = new StreamSettings();

        /// <summary>
        /// Default Values
        /// </summary>
        public StreamSettings()
        {
            curveMaterial = new Material() { };
            curveMaterial.DiffuseColor = System.Drawing.Color.White;

            meshMaterial = new Material() { };
            meshMaterial.DiffuseColor = System.Drawing.Color.DarkSlateGray;

            curveDivision = 100;
            curveWidth = 0.02f;
        }

        private Material curveMaterial;
        private double curveDivision;
        private float curveWidth;

        private Material meshMaterial;

        public static StreamSettings Default
        {
            get { return _default; }
            set { _default = value; }
        }

        public float CurveWidth
        {
            get { return curveWidth; }
            set { curveWidth = value; }
        }

        /// <summary>
        /// Curve SegmentationLength
        /// </summary>
        public double CurveDivision
        {
            get { return curveDivision; }
            set { curveDivision = value; }
        }
        public Material CurveMaterial
        {
            get { return curveMaterial; }
            set { curveMaterial = value; }
        }

        public Material MeshMaterial
        {
            get { return meshMaterial; }
            set { meshMaterial = value; }
        }
    }

    public class StreamSettingsParameter : GH_PersistentParam<GH_StreamSettings>
    {
        public StreamSettingsParameter() : base("StreamSettings", "Settings", "Settings", "Streaming", "Network") { }
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override System.Drawing.Bitmap Icon => null;
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