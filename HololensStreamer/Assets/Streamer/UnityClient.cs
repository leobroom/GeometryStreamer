using System.Collections.Generic;
using GeoStreamer;
using SocketStreamer;
using UnityEngine;

/// <summary>
/// ACHTUNG HIER DÜRFEN KEINE DEBUGLOGS DRIN STEHEN
/// </summary>
class UnityClient : GeoClient<UnityClient>
{
    private Queue<ISerializableData> geometryChanged = new Queue<ISerializableData>();


    private Queue<ISerializableData> doSomethingBefore = new Queue<ISerializableData>();

    protected override void UpdateCurves(BroadCastCurve data)
    {
        lock (geometryChanged)
        {
            geometryChanged.Enqueue(data);
        }
    }

    protected override void UpdateMesh(BroadCastMesh data)
    {
        lock (geometryChanged)
        {
            geometryChanged.Enqueue(data);
        }
    }

    protected override void UpdateText(BroadCastText data)
    {
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

            //Clears old Geometry
            geometryChanged.Clear();
        }
    }

    public void ProcessMessages()
    {
        ISerializableData updateGeometry = null;

        lock (doSomethingBefore)
            if (doSomethingBefore.Count > 0)
                updateGeometry = doSomethingBefore.Dequeue();

        if (updateGeometry != null)
            Factory.Instance.UpdateGeometry((BroadCastGeometryInfo)updateGeometry);

        //GeometryChanged
        ISerializableData broadcast = null;

        lock (geometryChanged)
            if (geometryChanged.Count > 0)
                broadcast = geometryChanged.Dequeue();

        if (broadcast == null)
            return;

        if (broadcast is BroadCastMesh)
        {
            Factory.Instance.UpdateMesh((BroadCastMesh)broadcast);
        }
        else if (broadcast is BroadCastCurve)
        {
            Factory.Instance.UpdateCurve((BroadCastCurve)broadcast);
        }
        else if (broadcast is BroadCastText)
        {
            Factory.Instance.UpdateText((BroadCastText)broadcast);
        }
    }

    public void SendIndex(int idx)
    {
        BroadCastIndex idxMsg = new BroadCastIndex
        {
            gateId = 0,
            index = idx
        };

        Send(idxMsg);
    }
}