using System.Collections.Generic;
using UnityEngine;

public class WaterManager : MonoBehaviour
{
    // - SINGLETON -
    public WaterManager Instance { get; private set; }


    // - EVENTS -
    // - VARIABLES -
    [Header("- Noise -")]
    [SerializeField] private Texture2D _noiseTexture; // Textura de ruido
    [SerializeField] private float _noiseScale = 100f;

    [Header("- Wave -")]
    [SerializeField] float _maxWaveStrength = 5f; // Fuerza máxima de las olas
    [SerializeField] float _seaCurrentStrength = 2f; // Fuerza de la corriente marina
    [SerializeField] float _maxWaveHeight = 2f; // Altura máxima de las olas
    [SerializeField] float _waveSpeed = 1f;
    [SerializeField] Vector2 _waveDirection = Vector2.right;


    [Header("- GIZMOS -")]
    public int waveGridAmount = 10;

    // - UNITY -
    private void Awake()
    {
        // Instance = this;
    }
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        //Vector3 force = CalculateExternalForces(transform.position);
        //Gizmos.color = Color.red;
        //Gizmos.DrawLine(transform.position, transform.position + force);

        Gizmos.color = Color.blue;
        float halfWaveGrid = waveGridAmount / 2f;
        for (int i = 0; i < waveGridAmount; i++)
        {
            for (int j = 0; j < waveGridAmount; j++)
            {
                Vector3 tempPos = new Vector3(transform.position.x + j - halfWaveGrid,
                    0,
                    transform.position.z + i - halfWaveGrid);

                float heightInPos = GetWaveHeight(tempPos);

                Gizmos.DrawLine(tempPos, new Vector3(tempPos.x, heightInPos, tempPos.z));
            }
        }
    }

    // Método para calcular las fuerzas externas
    public Vector3 CalculateExternalForces(Vector3 boatPosition)
    {
        // Obtener la posición del barco en el rango [0, 1] para samplear la textura de ruido
        float normalizedX = boatPosition.x / 100f;
        float normalizedZ = boatPosition.z / 100f;

        // Samplear la textura de ruido en la posición normalizada
        float waveSample = _noiseTexture.GetPixelBilinear(normalizedX, normalizedZ).r;

        // Calcular la fuerza de las olas basada en el valor de la textura de ruido
        float waveStrength = waveSample * _maxWaveStrength;

        // Calcular la fuerza de las olas (dirección y fuerza)
        Vector3 waveForce = Vector3.up * waveStrength;

        // Calcular la fuerza de la corriente marina
        Vector3 currentForce = new Vector3(_waveDirection.x, 0, _waveDirection.y) * _seaCurrentStrength; // Ejemplo de fuerza de corriente

        // Combinar todas las fuerzas externas
        Vector3 externalForces = currentForce + waveForce;

        return externalForces;
    }


    // Método para obtener la altura de las olas en una posición dada
    public float GetWaveHeight(Vector3 position)
    {
        float time = Time.time * _waveSpeed;

        // Normalizar las coordenadas y agregar tiempo para animación
        float normalizedX = (position.x / _noiseScale) + time * _waveDirection.x;
        float normalizedZ = (position.z / _noiseScale) + time * _waveDirection.y;

        // Samplear la textura de ruido
        float waveSample = _noiseTexture.GetPixelBilinear(normalizedX, normalizedZ).r;

        // Calcular la altura de la ola
        float waveHeight = waveSample * _maxWaveHeight;

        return waveHeight;
    }
    public float[] GetWaveHeights(Vector3[] _positions)
    {
        float time = Time.time * _waveSpeed;

        List<float> tempList = new List<float>();

        for (int i = 0; i < _positions.Length; i++)
        {
            // Normalizar las coordenadas y agregar tiempo para animación
            float normalizedX = (_positions[i].x / _noiseScale) + time * _waveDirection.x;
            float normalizedZ = (_positions[i].z / _noiseScale) + time * _waveDirection.y;

            // Samplear la textura de ruido
            float waveSample = _noiseTexture.GetPixelBilinear(normalizedX, normalizedZ).r;

            // Calcular la altura de la ola
            float waveHeight = waveSample * _maxWaveHeight;

            tempList.Add(waveHeight);
        }

        return tempList.ToArray();
    }

    public void CreateGroup(Panel_Stats panelStats)
    {
        Slot_GroupStats group = panelStats.CreateGroup("Wave Manager");

        // Noise
        group.CreateSlider("noiseScale").Configure("Noise: Scale",
            _noiseScale, 32, 256, data => { _noiseScale = data; }, true);

        // Wave
        group.CreateSlider("seaCurrentStrength").Configure("Sea Current: Strength",
           _seaCurrentStrength, 0, 8, data => { _seaCurrentStrength = data; });

        group.CreateSlider("waveStrengthMax").Configure("Wave: Strength Max",
           _maxWaveStrength, 0, 16, data => { _maxWaveStrength = data; });
        
        group.CreateSlider("waveHeight").Configure("Wave: Height",
           _maxWaveHeight, .1f, 8, data => { _maxWaveHeight = data; });
        group.CreateSlider("waveSpeed").Configure("Wave: Speed",
           _waveSpeed, 0, 8, data => { _waveSpeed = data; });

        group.CreateSlider("waveDirX").Configure("Wave: Direction X",
           _waveDirection.x, -2, 2, data => { _waveDirection.x = data; });
        group.CreateSlider("waveDirY").Configure("Wave: Direction Y",
           _waveDirection.y, -2, 2, data => { _waveDirection.y = data; });
    }
}