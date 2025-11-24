using UnityEngine;

public class Tool_Teleporter : MonoBehaviour
{
    // - VARIABLES -
    [Header("- Teleport -")]
    [SerializeField] Transform _destination;


    // - UNITY -
    private void OnTriggerEnter(Collider other)
    {
        // If is NOT a Player : return
        if (!other.CompareTag("Player")) return;

        IMove tempMove = other.GetComponent<IMove>();
        if (tempMove == null)
        {
            Debug.Log($"Cant warp: {other.name}");
            return;
        }

        tempMove.Warp(_destination.position);
    }
}
