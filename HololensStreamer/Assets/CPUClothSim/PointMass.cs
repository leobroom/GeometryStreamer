using System.Collections.Generic;
using UnityEngine;

public class PointMass
{
    public float mass = 1f;

    private float elapsedTime = 0;
    private const float TIMEFACTOR = 0.009f;
    private const float DAMPING = 0.09f;

    private Vector3 nextPos;
    private Vector3 actualPos;
    private Vector3 lastPos;
    private Vector3 pinPos;
    private Vector3 vel;
    private Vector3 acc;

    public List<Link> links = new List<Link>();
    public bool pinned = false;

    public PointMass(Vector3 position)
    {
        pinPos = position;
        lastPos = position;
        actualPos = position;
    }

    public Vector3 ActualPos
    {
        get { return actualPos; }
        set
        {
            if (!pinned)
                actualPos = value;
        }
    }

    public void UpdateForce(Vector3 force)
    {
        acc = force / mass;

        elapsedTime = Time.deltaTime * TIMEFACTOR;

        for (int i = 0; i < 1; i++)
        {
            for (int u = 0; u < links.Count; u++)
            {
                var link = links[u];

                if (link != null)
                    link.Solve();
            }

            vel = actualPos - lastPos;

            vel *= DAMPING;
            nextPos = actualPos + vel + acc * elapsedTime;
            lastPos = actualPos;
            actualPos = nextPos;

            if (pinned)
                actualPos = pinPos;
        }
    }
}