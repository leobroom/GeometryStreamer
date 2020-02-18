using System;
using System.Windows.Threading;
using GeoStreamer;

namespace GeoGrasshopper
{
    public class IndexEventArgs : EventArgs
    {
        public int index = -999;
        public int gateId = -999;

        public IndexEventArgs(int gateId, int index)
        {
            this.index = index;
            this.gateId = gateId;
        }
    }

    public class RhinoClient : GeoClient<RhinoClient>
    {
        public static event EventHandler<IndexEventArgs> OnIndexChanged;

        Dispatcher dispatcher;

        public RhinoClient() : base()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
        }

        protected override void UpdateIndex(BroadCastIndex updateIndex)
        {
            dispatcher.Invoke(() =>
            {
                OnIndexChanged?.Invoke(this, new IndexEventArgs(updateIndex.gateId,updateIndex.index));
            });    
        }
    }
}