using System;
using System.Collections.Generic;
using GeoStreamer;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace GeoGrasshopper
{
    public class GeoGrasshopperComponent : GH_Component
    {
        private static EventClient client;
        private static readonly List<string> debugLog = new List<string>();

        public GeoGrasshopperComponent() : base("GeometryStreamer", "GeoStream", "Streams Geometry", "Streaming", "Network") { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            string defaultIP = Utils.GetTestIpAdress();

            pManager.AddTextParameter("IpAdress", "IP", "Ip Adress to Connect to Server", GH_ParamAccess.item, defaultIP);
            pManager.AddBooleanParameter("Connect", "C", "Connection to the Server", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("UpdateDebug", "U", "Update TheDebugLog - put a Button there", GH_ParamAccess.item, false);
            pManager.AddMeshParameter("Meshes", "M", "Streaming Meshes", GH_ParamAccess.list);
            pManager.AddCurveParameter("Curves", "Crvs", "Streaming Curves", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("IpAdress", "IP", "IpAdress To the Server", GH_ParamAccess.item);
            pManager.AddTextParameter("Debug", "D", "Debug Messages", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string ipAdress = "";
            bool connect = false;
            List<Mesh> meshes = new List<Mesh>();
            List<Curve> curves = new List<Curve>();

            DA.GetData(0, ref ipAdress);
            DA.GetData(1, ref connect);

            DA.SetData(0,  ipAdress);

            if (!DA.GetDataList(3, meshes))
                meshes = new List<Mesh>();

            if (!DA.GetDataList(4, curves))
                curves = new List<Curve>();

            ConnectClient(connect, ipAdress);

            lock (debugLog)
            {
                if (debugLog.Count != 0)
                    DA.SetDataList(1, debugLog);
            }

            //GEOMETRY STUFF


            if (client == null || !connect)
                return;

            if (meshes != null && meshes.Count !=0)
            {
                int mCount = meshes.Count;

                for (int i = 0; i < mCount; i++)
                {
                    Mesh mesh = meshes[i];

                    if (mesh == null || mesh.Vertices == null)
                        continue;

                    BroadCastMesh netMesh = GetMeshChanged(i, mesh);
                    client.Send(netMesh);
                }
            }
       

            if (curves != null && curves.Count != 0)
            {
                int mCount = meshes.Count;

                double length = 100; //Hier ausbessern

                BroadCastCurves netCurves = GetCurvesChanged(mCount, curves, length);
                client.Send(netCurves);
            }
        }

        /// <summary>
        /// Connects to clientwhen there is no connection
        /// </summary>
        /// <param name="connect"></param>
        /// <param name="ipAdress"></param>
        private void ConnectClient(bool connect, string ipAdress)
        {
            if (client == null && connect)
            {
                client = EventClient.Initialize(ipAdress, 12345, "RhinoClient", ThreadingType.Task, ClientType.Default);
                client.Message += OnMessage;
                client.Connect();
            }
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
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Grasshopper.Properties.Resources.GetLogo;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("da5bb4b2-a7e5-4fd4-8b67-67e20eb8d9e2"); }
        }

        //MESHES

        public BroadCastMesh GetMeshChanged(int id, Mesh mesh)
        {
            BroadCastMesh netMesh = new BroadCastMesh();
            netMesh.id = id; 
            netMesh.vertices = GetVertices(mesh);
            netMesh.triangles = Triangulate(mesh).ToArray();
            netMesh.normals = GetNormals(mesh);

            return netMesh;
        }


        public float[] GetVertices(Mesh m)
        {
            int count = m.Vertices.Count;

            float[] vertices = new float[count * 3];

            for (int i = 0; i < count; i++)
            {
                var vert = m.Vertices[i];
                int idx = i * 3;
                vertices[idx] = vert.X;
                vertices[idx + 1] = vert.Y;
                vertices[idx + 2] = vert.Z;
            }

            return vertices;
        }

        public float[] GetNormals(Mesh m)
        {
            int count = m.Normals.Count;

            float[] normals = new float[count * 3];

            for (int i = 0; i < count; i++)
            {
                var norm = m.Normals[i];
                int idx = i * 3;
                normals[idx] = norm.X;
                normals[idx + 1] = norm.Y;
                normals[idx + 2 + 0] = norm.Z;
            }

            return normals;
        }

        public List<int> Triangulate(Mesh m)
        {
            int facecount = m.Faces.Count;
            List<int> lst = new List<int>(facecount);

            for (int i = 0; i < facecount; i++)
            {
                var mf = m.Faces[i];

                if (mf.IsQuad)
                {
                    double dist1 = m.Vertices[mf.A].DistanceTo(m.Vertices[mf.C]);
                    double dist2 = m.Vertices[mf.B].DistanceTo(m.Vertices[mf.D]);
                    if (dist1 > dist2)
                    {
                        lst.Add(mf.A);
                        lst.Add(mf.B);
                        lst.Add(mf.D);
                        lst.Add(mf.B);
                        lst.Add(mf.C);
                        lst.Add(mf.D);
                    }
                    else
                    {
                        lst.Add(mf.A);
                        lst.Add(mf.B);
                        lst.Add(mf.C);
                        lst.Add(mf.A);
                        lst.Add(mf.C);
                        lst.Add(mf.D);
                    }
                }
                else
                {
                    lst.Add(mf.A);
                    lst.Add(mf.B);
                    lst.Add(mf.C);
                }
            }
            return lst;
        }

        //CURVES

        private BroadCastCurves GetCurvesChanged(int id, List<Curve> curves, double length)
        {
            int crvCount = curves.Count;

            List<float> positions = new List<float>();
            int[] curveLength = new int[crvCount];
            int[] ids = new int[crvCount];

            //For every Curve
            for (int crvIdx = 0; crvIdx < crvCount; crvIdx++)
            {
                Curve curve = curves[crvIdx];

                if (curve.IsPolyline())
                {
                    var nc = curve.ToNurbsCurve();

                    int ptCount = nc.Points.Count;

                    for (int i = 0; i < ptCount; i++)
                        AddPointValues(nc.Points[i].Location, positions);

                    curveLength[crvIdx] = ptCount;
                }
                else
                {
                    AddPointValues(curve.PointAt(curve.Domain.Min), positions);
                    double maxLength = curve.GetLength();
                    if (length != 0 && length < maxLength)
                    {
                        Point3d[] pts;
                        curve.DivideByLength(length, false, out pts);

                        foreach (var pt in pts)
                            AddPointValues(pt, positions);

                        curveLength[crvIdx] = pts.Length + 2;
                    }
                    else
                        curveLength[crvIdx] = 2;
                    AddPointValues(curve.PointAt(curve.Domain.Max), positions);
                }

                ids[crvIdx] = id++;
            }

            BroadCastCurves netCurves = new BroadCastCurves();
            netCurves.ids = ids;
            netCurves.curveLength = curveLength;
            netCurves.positions = positions.ToArray();

            return netCurves;
        }

        private void AddPointValues(Point3d pt, List<float> positions)
        {
            positions.Add((float)pt.X);
            positions.Add((float)pt.Y);
            positions.Add((float)pt.Z);
        }
    }
}