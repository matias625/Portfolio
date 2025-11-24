using UnityEngine;

public static class Tool_FloorDetector
{
    public static bool HasFloor_Box(Vector3 positionWOffset, Vector3 halfBox, Quaternion rotation, LayerMask layers)
    {
        return Physics.CheckBox(positionWOffset, halfBox, rotation, layers, QueryTriggerInteraction.Ignore);
    }
    public static bool HasFloor_Sphere(Vector3 positionWOffset, float radius, LayerMask layers)
    {
        return Physics.CheckSphere(positionWOffset, radius, layers, QueryTriggerInteraction.Ignore);
    }
    public static bool HasSomething_Ray(Vector3 positionWOffset, Vector3 direction, float distance, LayerMask layers)
    {
        return Physics.Raycast(positionWOffset, direction, distance, layers, QueryTriggerInteraction.Ignore);
    }
}