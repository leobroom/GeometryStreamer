using Grasshopper.Kernel;
using Rhino.Display;
using Rhino.Geometry;
using System.Collections.Generic;

namespace GeoGrasshopper.FiberWinding
{
    public partial class FiberWinder : GH_Component
    {
        private readonly double curveSmall = 0.001;
        private readonly double curveBig = 0.002;

        private readonly int prevColorId = 0;
        private readonly int nextColorId = 1;
        private readonly int crvD = 8;

        private StreamSettings GetStreamSettings()
        {
            StreamSettings streamSet = new StreamSettings(System.Drawing.Color.Gray);

            //#####MAT#####
            DisplayMaterial matPrev = new DisplayMaterial { Diffuse = previousColor };
            DisplayMaterial matNext = new DisplayMaterial { Diffuse = nextColor };

            streamSet.Materials = new List<DisplayMaterial>() { matPrev, matNext };

            //#####MATID#####
            if (previous != null)
                SetCrvSetting(previous.ToNurbsCurve(), prevColorId, curveSmall, crvD);
            SetCrvSetting(next, nextColorId, curveBig, crvD);
            SetCrvSetting(arrowLine, nextColorId, curveBig, crvD);

            streamSet.CurveDivisions = crvDiv;
            streamSet.CurveWidths = crvWidth;
            streamSet.ObjMatIds = matId;

            return streamSet;
        }

        public void SetCrvSetting(NurbsCurve crv, int mat, double crvWdth, int crvD)
        {
            if (crv == null)
                return;

            geometry.Add(crv);
            matId.Add(mat);
            crvWidth.Add(crvWdth);
            crvDiv.Add(crvD);
        }
    }
}
