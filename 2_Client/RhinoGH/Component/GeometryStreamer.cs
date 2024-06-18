using System;
using System.Collections.Generic;
using System.Threading;
using GeoGrasshopper;
using GeoStreamer;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

namespace RhinoGH.Component
{
    public class GeometryStreamer : GH_Component
    {
        private double timeIntervalInSeconds = 0.1;

        private RhinoClient client;
        private readonly List<string> debugLog = new List<string>();
        private Timer timer;
        private static Tuple<StreamSettings, List<object>> actualValues = null;

        public GeometryStreamer()
          : base("GeometryStreamer", "GeoStream", "Streams Geometry, Author: Leon Brohmann - leonbrohmann@gmx.de", "LB", "Network")
        {
        
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            string defaultIP = Utils.GetTestIpAdress();

            pManager.AddTextParameter("IpAdress", "IP", "Ip Adress to Connect to Server", GH_ParamAccess.item, defaultIP);
            pManager.AddBooleanParameter("Connect", "C", "Connection to the Server", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("UpdateDebug", "U/D", "Updates theDebugLog - put a Button there", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Geometry", "Geo", "Streaming Geometry - Just Meshes and Curves are right now allowed", GH_ParamAccess.list);
            pManager.AddParameter(new StreamSettingsParameter(), "Settings", "Set", "Material and other Settings", GH_ParamAccess.item);

            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Debug", "D", "Debug Messages", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool isConnected = ConnectClient(DA);

            if (timer == null)
                timer = new Timer(SendLoop, "Some state", TimeSpan.FromSeconds(timeIntervalInSeconds), TimeSpan.FromSeconds(timeIntervalInSeconds));

            SetDebug(DA);

            if (client == null || !isConnected)
            {
                return;
            }

            SendGeometry(DA);
        }

        private void SendLoop(object state)
        {
            StreamSettings settings;
            List<object> geometry;

            if (actualValues == null)
                return;

            lock (actualValues)
            {
                settings = actualValues.Item1;
                geometry = actualValues.Item2;

                actualValues = null;
            }

            int geoCount = geometry.Count;
            int curveCount = 0;
            int meshCount = 0;
            int textCount = 0;

            for (int id = 0; id < geoCount; id++)
            {
                var geo = geometry[id];
                if (geo == null)
                    continue;

                if (geo is GH_Curve)
                    curveCount++;
                else if (geo is GH_Mesh)
                    meshCount++;
                else if (geo is GH_StreamText)
                    textCount++;
            }

            Send.GeometryInfo(curveCount, meshCount, textCount, client);

            int meshNr = 0;
            int curveNr = 0;
            int textNr = 0;

            for (int id = 0; id < geoCount; id++)
            {
                var geo = geometry[id];
                if (geo == null)
                    continue;

                if (geo is GH_Curve)
                {
                    Send.Curve(id, curveNr, ((GH_Curve)geo).Value, settings, client);
                    curveNr++;
                }

                else if (geo is GH_Mesh)
                {
                    Send.Mesh(id, meshNr, ((GH_Mesh)geo).Value, settings, client);
                    meshNr++;
                }

                else if (geo is GH_StreamText)
                {

                    StreamText bla = ((GH_StreamText)geo).Value;

                    var df = bla.Text;

                    Send.Text(id, textNr, ((GH_StreamText)geo).Value, settings, client);
                    textNr++;
                }

                else
                {
                    throw new Exception("TYPE IS NOT ALLOWED" + ((GH_ObjectWrapper)geo).Value.GetType());
                }
            }
        }

        private void SetDebug(IGH_DataAccess DA)
        {
            //lock (debugLog)
            //{
            //    lock (DA)
            //    {
            //        if (debugLog.Count != 0)
            //            DA.SetDataList(0, debugLog);
            //    }
            //}
        }



        private void SendGeometry(IGH_DataAccess DA)
        {
            GH_StreamSettings settingsGH = null;
            StreamSettings settings = (DA.GetData(4, ref settingsGH)) ?
                settingsGH?.Value : StreamSettings.Default;

            List<object> geometry = new List<object>();
            if (!DA.GetDataList(3, geometry))
                geometry = new List<object>();

            actualValues = new Tuple<StreamSettings, List<object>>(settings, geometry);
        }

        /// <summary>
        /// Connects to clientwhen there is no connection
        /// </summary>
        /// <param name="connect"></param>
        /// <param name="ipAdress"></param>
        private bool ConnectClient(IGH_DataAccess DA)
        {
            bool connect = false;
            DA.GetData(1, ref connect);

            string ipAdress = "";
            int port = Utils.GetTestPort();

            bool isConnected = false;

            DA.GetData(0, ref ipAdress);

            if (connect)
            {
                if (client == null)
                {
                    client = RhinoClient.Initialize(ipAdress, port, "RhinoClient", ThreadingType.Task, 20, (int)ClientType.Default);
                }

                if (!client.IsConnected)
                {
                    //  client = RhinoClient.Initialize(ipAdress, port, "RhinoClient", ThreadingType.Thread, 20, (int)ClientType.Default);
                    // client.Message += OnMessage;
                    client.Connect();
                }

                isConnected = client.IsConnected;
            }
            else
            {
                if (client != null)
                {
                    client.Disconnect();
                }

                //client = null;
                isConnected = false;
                debugLog.Add("Try to Disconnect...");
            }

            return isConnected;
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            client?.Disconnect();

            base.RemovedFromDocument(document);
        }

        private void OnMessage(object sender, MessageArgs e)
        {
            debugLog.Add(e.Message);
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
            get { return new Guid("8CF070D8-B217-48B3-A7FE-8EFB34174A6E"); }
        }
    }
}