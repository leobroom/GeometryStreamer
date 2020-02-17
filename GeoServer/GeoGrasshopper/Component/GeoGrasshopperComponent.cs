using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeoStreamer;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using SocketStreamer;

namespace GeoGrasshopper
{
    public class GeoGrasshopperComponent : GH_Component
    {
        private static RhinoClient client;
        private static readonly List<string> debugLog = new List<string>();

        public GeoGrasshopperComponent() : base("GeometryStreamer", "GeoStream", "Streams Geometry, Author: Leon Brohmann - leonbrohmann@gmx.de", "Streaming", "Network") { }

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
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool isConnected = ConnectClient(DA);

            if (timer == null)
                timer = new Timer(SendLoop, "Some state", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));


            SetDebug(DA);

            if (client == null || !isConnected)
                return;

            SendGeometry(DA);
        }

        private static void SendLoop(object state)
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

            for (int id = 0; id < geoCount; id++)
            {
                var geo = geometry[id];
                if (geo == null)
                    continue;

                if (geo is GH_Curve)
                    Send.Curve(id, ((GH_Curve)geo).Value, settings, client);
                else if (geo is GH_Mesh)
                    Send.Mesh(id, ((GH_Mesh)geo).Value, settings, client);
                else if (geo is GH_StreamText)
                    Send.Text(id, ((GH_StreamText)geo).Value, settings, client);
                else
                {
                    //string error = ($"Geometry is: {geo.GetType()} and it's not supported right now. Questions?: leonbrohmann@gmx.de");
                    //AddRuntimeMessage(GH_RuntimeMessageLevel.Error, error);
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

        Timer timer;
        static Tuple<StreamSettings, List<object>> actualValues = null;

        private void SendGeometry(IGH_DataAccess DA)
        {
            GH_StreamSettings settingsGH = null;
            StreamSettings settings = (DA.GetData(4, ref settingsGH)) ?
                settingsGH?.Value : StreamSettings.Default;

            List<object> geometry = new List<object>();
            if (!DA.GetDataList(3, geometry))
                geometry = new List<object>();



            actualValues = new Tuple<StreamSettings, List<object>>(settings, geometry);

            //int geoCount = geometry.Count;
            //int curveCount = 0;
            //int meshCount = 0;
            //int textCount = 0;

            //for (int id = 0; id < geoCount; id++)
            //{
            //    var geo = geometry[id];
            //    if (geo == null)
            //        continue;

            //    if (geo is GH_Curve)
            //        curveCount++;
            //    else if (geo is GH_Mesh)
            //        meshCount++;
            //    else if (geo is GH_StreamText)
            //        textCount++;
            //}

            //Send.GeometryInfo(curveCount, meshCount, textCount, client);

            //for (int id = 0; id < geoCount; id++)
            //{
            //    var geo = geometry[id];
            //    if (geo == null)
            //        continue;

            //    if (geo is GH_Curve)
            //        Send.Curve(id, ((GH_Curve)geo).Value, settings, client);
            //    else if (geo is GH_Mesh)
            //        Send.Mesh(id, ((GH_Mesh)geo).Value, settings, client);
            //    else if (geo is GH_StreamText)
            //        Send.Text(id, ((GH_StreamText)geo).Value, settings, client);
            //    else
            //    {
            //        string error = ($"Geometry is: {geo.GetType()} and it's not supported right now. Questions?: leonbrohmann@gmx.de");
            //        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, error);
            //    }
            //}
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
            DA.GetData(0, ref ipAdress);

            if (client == null && connect)
            {
                client = RhinoClient.Initialize(ipAdress, 12345, "RhinoClient", ThreadingType.Thread, (int)ClientType.Default);
                client.Message += OnMessage;
                client.Connect();
            }
            else if (client != null && !connect)
            {
                client.Disconnect();
                client = null;
                debugLog.Add("Try to Disconnect...");
            }

            return connect;
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
        /// The Exposure property controls where in the panel a component icon 
        /// will appear. There are seven possible locations (primary to septenary), 
        /// each of which can be combined with the GH_Exposure.obscure flag, which 
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.gs;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("da5bb4b2-a7e5-4fd4-8b67-67e20eb8d9e2");
    }
}