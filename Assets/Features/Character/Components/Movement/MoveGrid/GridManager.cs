using UnityEngine;

public class GridManager : MonoBehaviour
{
    // - VARIABLES -
    [Header("Grid Settings")]
    [SerializeField] 
    private float _cellSize = 1f;
    [SerializeField] 
    private Vector3 _origin = new Vector3(0.5f, 0, 0.5f);
    [SerializeField] 
    private LayerMask _obstacleMask;


    // - GET -
    public static GridManager Instance { get; private set; }
    /// <summary>
    /// Convert world position to Cell coordinates
    /// </summary>
    public Vector3Int WorldToCell(Vector3 worldPos)
    {
        Vector3 local = worldPos - _origin;

        int x = Mathf.RoundToInt(local.x / _cellSize);
        int y = Mathf.RoundToInt(local.y / _cellSize);
        int z = Mathf.RoundToInt(local.z / _cellSize);

        return new Vector3Int(x, y, z);
    }
    /// <summary>
    /// Convert cell coordinates to world position.
    /// </summary>
    public Vector3 CellToWorld(Vector3Int cell)
    {
        return _origin + new Vector3(cell.x * _cellSize,
                                    cell.y * _cellSize,
                                    cell.z * _cellSize);
    }
    /// <summary>
    /// Returns the center of the cell closest to a position in world.
    /// </summary>
    public Vector3 GetClosestCellCenter(Vector3 worldPos)
    {
        Vector3Int cell = WorldToCell(worldPos);
        return CellToWorld(cell);
    }

    public bool IsCellFree(Vector3Int cell)
    {
        // Obtener el centro de la celda en mundo
        Vector3 center = CellToWorld(cell) + (Vector3.up * (_cellSize / 2f));

        // Debug.Log($"Cell {cell.ToString()} -Center: {center.ToString()}");

        // Definir el tama�o de la caja de chequeo (ligeramente menor que la celda)
        Vector3 halfExtents = Vector3.one * (_cellSize * 0.45f);

        // Revisar si hay alg�n collider en esa celda con la m�scara de obst�culos
        // bool blocked = Physics.CheckBox(center, halfExtents, Quaternion.identity, _obstacleMask);
        Collider[] cols = Physics.OverlapBox(center, halfExtents, Quaternion.identity, _obstacleMask);
        bool blocked = cols.Length > 0;

        return !blocked;
    }
    public bool HasCellFloor(Vector3Int cell)
    {
        Vector3 center = CellToWorld(cell);

        return Tool_FloorDetector.HasSomething_Ray(center + Vector3.one * .1f, Vector3.down, .15f, _obstacleMask); //  Physics.CheckBox(center, Vector3.one * .05f, Quaternion.identity, _obstacleMask);
    }
    public bool HasWorldFloor(Vector3 worldPos)
    {
        return Tool_FloorDetector.HasSomething_Ray(worldPos + Vector3.one * .1f, Vector3.down, .15f, _obstacleMask);
    }
    public bool IsJumpTrajectoryClear(Vector3Int startCell, Vector3Int endCell, float jumpHeight, float characterHeight, int steps = 20)
    {
        Vector3 start = CellToWorld(startCell);
        Vector3 end = CellToWorld(endCell);

        Vector3 prevPoint = start;

        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;

            Vector3 point = Vector3.Lerp(start, end, t);
            point.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;

            Vector3 dir = point - prevPoint;
            float dist = dir.magnitude;

            if (Physics.Raycast(prevPoint + Vector3.up * characterHeight * 0.5f, dir.normalized, dist, _obstacleMask))
            {
                Debug.Log("Jump blocked by obstacle!");
                return false;
            }

            prevPoint = point;
        }

        return true;
    }


    // - PUBLIC -
    /// <summary>
    /// Returns the cell center in a direction and number of steps from a base cell.
    /// </summary>
    public Vector3 GetCellCenterInDirection(Vector3 worldPos, Vector3 dir, int steps = 1)
    {
        Vector3Int cell = WorldToCell(worldPos);

        // Convertir direcci�n a celda discreta
        Vector3Int offset = new Vector3Int(
            Mathf.RoundToInt(dir.x),
            Mathf.RoundToInt(dir.y),
            Mathf.RoundToInt(dir.z)
        );

        Vector3Int targetCell = cell + (offset * steps);
        return CellToWorld(targetCell);
    }
    /// <summary>
    /// Returns the cell in a direction and number of steps from a base cell.
    /// </summary>
    public Vector3Int GetCellInDirection(Vector3 worldPos, Vector3 dir, int steps = 1)
    {
        Vector3Int cell = WorldToCell(worldPos);

        // Convertir direcci�n a celda discreta
        Vector3Int offset = new Vector3Int(
            Mathf.RoundToInt(dir.x),
            Mathf.RoundToInt(dir.y),
            Mathf.RoundToInt(dir.z)
        );

        Vector3Int targetCell = cell + (offset * steps);
        return targetCell;
    }


    // - UNITY -
    private void Awake()
    {
        Instance = this;
    }
}