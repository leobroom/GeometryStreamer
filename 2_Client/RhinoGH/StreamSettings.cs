﻿using Rhino.Display;
using System.Collections.Generic;

namespace GeoGrasshopper
{
    public class StreamSettings
    {
        private static StreamSettings _default = new(System.Drawing.Color.Gray) { };

        /// <summary>
        /// Default Values
        /// </summary>
        public StreamSettings() { }

        public StreamSettings(System.Drawing.Color defaultColor)
        {
            var defaultMaterial = new DisplayMaterial
            {
                Diffuse = defaultColor
            };

            materials.Add(defaultMaterial);

            double curveDivision = 100;
            int curveWidth = 1;
            int id = 0;

            curveDivisions.Add(curveDivision);
            curveWidths.Add(curveWidth);
            objMatIds.Add(id);
        }

        private List<DisplayMaterial> materials = new List<DisplayMaterial>();
        private List<int> objMatIds = new List<int>();
        private List<double> curveDivisions = new List<double>(), curveWidths = new List<double>();

        public static StreamSettings Default
        {
            get { return _default; }
            set { _default = value; }
        }

        public List<int> ObjMatIds
        {
            get { return objMatIds; }
            set { objMatIds = value; }
        }

        public List<double> CurveWidths
        {
            get { return curveWidths; }
            set { curveWidths = value; }
        }

        /// <summary>
        /// Curve SegmentationLength
        /// </summary>
        public List<double> CurveDivisions
        {
            get { return curveDivisions; }
            set { curveDivisions = value; }
        }
        public List<DisplayMaterial> Materials
        {
            get { return materials; }
            set { materials = value; }
        }

        public override string ToString()
        {
            string s = "StreamSettings: ";

            foreach (var mat in materials)
                s+= ", C: "+ DisplayColor(mat.Diffuse);

            s += " | ";

            foreach (var curveDivision in curveDivisions)
                s += ", CDiv: " + curveDivision;

            s += " | ";

            foreach (var curveWidth in curveWidths)
                s += ", CWidth: " + curveWidth;

            return s;
        }

        private string DisplayColor(System.Drawing.Color c) => $"R: {c.R}, G: {c.G}, B: {c.B}, A: {c.A}";
    }
}