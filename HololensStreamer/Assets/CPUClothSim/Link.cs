using UnityEngine;

public class Link
{
    private float restDiff;
    private float stiffness = 0.5f;
    private PointMass pMass1;
    private PointMass pMass2;
    private Vector3 diff;
    private float d;

    public Link(PointMass pMass1, PointMass pMass2)
    {
        this.pMass1 = pMass1;
        this.pMass2 = pMass2;

        SetRestDiff();
    }

    public void Solve()
    {
        diff = pMass1.ActualPos - pMass2.ActualPos;
        d = diff.magnitude;

        float difference = (restDiff - d) / d;

        // Inverse the mass quantities and multiply by stiffness = (k/m) term
        float im1 = 1 / pMass1.mass;
        float im2 = 1 / pMass2.mass;
        float scalarP1 = (im1 / (im1 + im2)) * stiffness;
        float scalarP2 = stiffness - scalarP1;

        // Push/pull based on mass
        var pushPull = difference * diff;

        pMass1.ActualPos += pushPull * scalarP1;
        pMass2.ActualPos -= pushPull * scalarP2;
    }

    private void SetRestDiff()
        => restDiff = (pMass1.ActualPos - pMass2.ActualPos).magnitude;
}