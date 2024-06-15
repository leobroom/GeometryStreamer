using System;
using System.Collections.Generic;
using GeoStreamer;
using SocketStreamer;
using UnityEngine;

class UnityClient : GeoClient<UnityClient>
{
    private Queue<ISerializableData> geometryChanged = new Queue<ISerializableData>();


    private Queue<ISerializableData> doSomethingBefore = new Queue<ISerializableData>();

    protected override void UpdateCurves(BroadCastCurve data)
    {
        Debug.Log("OnCurveChanged");
        lock (geometryChanged)
        {
            geometryChanged.Enqueue(data);
        }
    }

    protected override void UpdateMesh(BroadCastMesh data)
    {
        Debug.Log("OnMeshChanged");
        lock (geometryChanged)
        {
            geometryChanged.Enqueue(data);
        }
    }

    protected override void UpdateGeometry(BroadCastGeometryInfo geoinfo)
    {
        lock (doSomethingBefore)
        {
            doSomethingBefore.Enqueue(geoinfo);
        }
    }

    public void ProcessMessages()
    {
        if (doSomethingBefore.Count > 0)
        {
            ISerializableData updateGeometry;

            lock (doSomethingBefore)
            {
                updateGeometry = doSomethingBefore.Dequeue();
            }

            if (updateGeometry != null)
                Factory.Instance.UpdateGeometry((BroadCastGeometryInfo)updateGeometry);
        }



        if (geometryChanged.Count == 0)
            return;

        ISerializableData broadcast;

        lock (geometryChanged)
        {
            broadcast = geometryChanged.Dequeue();
        }

        if (broadcast is BroadCastMesh)
        {
            Factory.Instance.UpdateMesh((BroadCastMesh)broadcast);
        }
        else if (broadcast is BroadCastCurve)
        {
            Factory.Instance.UpdateCurve((BroadCastCurve)broadcast);
        }
    }
}