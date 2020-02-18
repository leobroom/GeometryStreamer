using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;

namespace GeoGrasshopper.Component
{
    public class StreamGateComponent : GH_Component
    {
        private GH_Document ghDocument;

        private double newIndex = 0;
        private double oldIndex = 0;
        private double oldNumber = 0;
        private double newNumber = 0;
        private double output = 0;

        private int gateId = 0;

        private IGH_Param numberParam;
        private bool drawWire = false;

        /// <summary>
        /// Initializes a new instance of the IndexTestComponent class.
        /// </summary>
        public StreamGateComponent()
          : base("StreamGate", "S-Gate",
              "Shares an Input with a Network and a Grasshopper Input",
              "ITE", "Network")
        {
            RhinoClient.OnIndexChanged += OnIndexChanged;
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            Grasshopper.Instances.ActiveCanvas.CanvasPrePaintWires += PrePaintWires;
            ghDocument = document;
        }

        private void PrePaintWires(GH_Canvas canvas)
        {
            if (!drawWire)
                return;

            // We should only draw wires if the document loaded in the canvas is the document we're in.
            if (!ReferenceEquals(ghDocument, canvas.Document))
                return;

            if (numberParam.SourceCount == 0)
                return;

            IGH_Param source = numberParam.Sources[0];

            var input = numberParam.Attributes.InputGrip;
            var output = source.Attributes.OutputGrip;

            var path = GH_Painter.ConnectionPath
                (input, output,GH_WireDirection.left,GH_WireDirection.right);
           
            var edge = new Pen(Color.Red, 14)
            {
                DashCap = DashCap.Triangle,
                DashPattern = new float[] { 0.3f, 0.3f }
            };

            canvas.Graphics.DrawPath(edge, path);

            edge.Dispose();
            path.Dispose();
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            RhinoClient.OnIndexChanged -= OnIndexChanged;
            Grasshopper.Instances.ActiveCanvas.CanvasPrePaintWires -= PrePaintWires;
            base.RemovedFromDocument(document);
        }

        private void OnIndexChanged(object sender, IndexEventArgs e)
        {
            if (e.gateId != gateId)
                return;

            newIndex = e.index;

            ExpireSolution(true);
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("GateId", "G- ID", "The Id for the Network ID - normally 0", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Number", "N", "Number", GH_ParamAccess.item, 0);
            numberParam = pManager[1];
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Index", "Idx", "Test", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Get GH Comp Input
            double numb = -1;
            if (DA.GetData(1, ref numb))
                newNumber = numb;

            int gId = 0;
            if (DA.GetData(0, ref gId))
                gateId = gId;

            //Decide which input to choose
            if (newIndex != oldIndex)
            {
                drawWire = true;
                output = newIndex;
            }

            else if (newNumber != oldNumber)
            {
                drawWire = false;
                output = newNumber;
            }

            // Set Output
            DA.SetData(0, output);

            //Set statingcondition
            oldNumber = newNumber;
            oldIndex = newIndex;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override Bitmap Icon => Resources.gs_gate;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
            => new Guid("4dd98c3a-a054-4ea0-8d3c-e85401f14f07");
    }
}