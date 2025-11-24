using UnityEngine;

public static class Tool_Cleaner
{
    public static void Clean_Transform(Transform container)
    {
        if (container.childCount == 0)
            return;

        for (int i = container.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            UnityEngine.Object.DestroyImmediate(container.GetChild(i).gameObject);
#else
            UnityEngine.Object.Destroy(container.GetChild(i).gameObject);
#endif
        }
    }
}