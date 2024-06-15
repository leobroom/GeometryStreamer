using System;
using System.Windows.Threading;
using GeoStreamer;

namespace GeoGrasshopper
{
    public class IndexEventArgs(int gateId, int index) : EventArgs
    {
        public int index = index;
        public int gateId = gateId;
    }

    public class RhinoClient : GeoClient<RhinoClient>
    {
        public static event EventHandler<IndexEventArgs> OnIndexChanged;

        Dispatcher dispatcher;

        public RhinoClient() : base() => dispatcher = Dispatcher.CurrentDispatcher;

        protected override void UpdateIndex(BroadCastIndex updateIndex)
        {
            dispatcher.Invoke(() =>{ OnIndexChanged?.Invoke(this, new IndexEventArgs(updateIndex.gateId,updateIndex.index));});    
        }

        /// <summary>
        /// Send the actual GeometryIndex to the Server - Rhino reads it, actualise its StreamingGate
        /// - Geometry will change afterwards
        /// </summary>
        /// <param name="idx"></param>
        public void SendIndex(int idx)
        {
            BroadCastIndex idxMsg = new()
            {
                gateId = 0,
                index = idx
            };

            Send(idxMsg);
        }
    }
}