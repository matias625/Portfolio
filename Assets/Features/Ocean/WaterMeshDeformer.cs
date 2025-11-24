using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WaterMeshDeformer : MonoBehaviour
{
    public WaterManager waterManager; // Referencia al script WaterManager
    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] displacedVertices;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        originalVertices = mesh.vertices;
        displacedVertices = new Vector3[originalVertices.Length];
    }

    void Update()
    {
        for (int i = 0; i < originalVertices.Length; i++)
        {
            // Obtener la posición mundial del vértice
            Vector3 worldPosition = transform.TransformPoint(originalVertices[i]);

            // Calcular la altura de la ola en esa posición
            float waveHeight = waterManager.GetWaveHeight(worldPosition);

            // Modificar el vértice desplazándolo en Y
            displacedVertices[i] = new Vector3(originalVertices[i].x, waveHeight, originalVertices[i].z);
        }

        // Actualizar la malla con los nuevos vértices
        mesh.vertices = displacedVertices;
        mesh.RecalculateNormals(); // Recalcular las normales para iluminación correcta
    }
}