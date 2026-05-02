using UnityEngine;

public static class PlanarTransformExtensions
{
    public static Vector2 WorldToLocalPlanar(this Transform space, Vector3 worldOffset)
    {
        Vector3 local = space.InverseTransformDirection(worldOffset);
        return new Vector2(local.x, local.z);
    }

    public static Vector3 LocalPlanarToWorld(this Transform space, Vector2 localPlanar)
    {
        return space.TransformDirection(new Vector3(localPlanar.x, 0f, localPlanar.y));
    }
}
