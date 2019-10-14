using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using GeoStreamer;
using UnityEngine;

class UnityClient : Client<UnityClient>
{
    public event EventHandler<MessageArgs> Message;
    private Queue<ISerializableData> meshChanged = new Queue<ISerializableData>();

    protected override void SendLog(string message)
    {
        Message?.Invoke(this, new MessageArgs(message));
    }

    protected override void GetCurves(BroadCastCurves data)
    {
        Debug.Log("OnCurveChanged");
        lock (meshChanged)
        {
            meshChanged.Enqueue(data);
        }
    }

    protected override void GetMesh(BroadCastMesh data)
    {
        Debug.Log("OnMeshChanged");
        lock (meshChanged)
        {
            meshChanged.Enqueue(data);
        }
    }

    public void ProcessMessages()
    {
        if (meshChanged.Count == 0)
            return;

        ISerializableData broadcast;

        lock (meshChanged)
        {
            broadcast = meshChanged.Dequeue();
        }

        if (broadcast is BroadCastMesh)
        {
            Factory.Instance.CreateMesh((BroadCastMesh)broadcast);
        }
        else if (broadcast is BroadCastCurves)
        {
            Factory.Instance.CreateCurves((BroadCastCurves)broadcast);
        }
    }
}