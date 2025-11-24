using System;
using UnityEngine;
using UnityEngine.Events;

public class Move_Boat : MonoBehaviour, IMove
{
    // - EVENTS -
    public event UnityAction OnMoveStart;
    public event UnityAction OnMoveEnd;

    public event UnityAction<float> OnRudderRotation; // current rudder angle
    public event UnityAction<float, float> OnDesiredSpeedChange;     // current / max
    public event UnityAction<float, float> OnBoatSpeedChange;  // current / max

    public void Event_OnGroundChange(bool isGrounded)
    {
        // Dont use for now
    }


    // - VARIABLES -
    [Header("- Meter -")]
    [SerializeField] private Panel_BoatMeter _boatMeter;

    [Header("- Components -")]
    [SerializeField] private Rigidbody _myRigidbody;
    [SerializeField] private Animator _myAnimator;
    [SerializeField] private WaterManager _waterManager;

    [Header("- Options -")]
    [SerializeField, Tooltip("Speed of animation to change")]
    private float _moveAnimToZeroMultiplier = 6;

    [Header("- Contact Points -")]
    [SerializeField] private Transform _frontPoint;
    [SerializeField] private Transform _backPoint;
    [SerializeField] private Transform _leftPoint;
    [SerializeField] private Transform _rightPoint;

    [Header("- Rudder -")]
    [SerializeField] private float _rudderSpeedAnlge = 5;
    [SerializeField] private float _rudderMaxAngle = 100;   
    private float _rudderCurrentAngle = 0;
    private float _rudderNormalized = 0;

    [Header("- Desired Speed -")]
    [SerializeField, Tooltip("Mult * DeltaTime = speed obtained")] 
    private float _desiredSpeedMultiplier = 3f;
    [SerializeField] private float _desiredSpeedMax = 8f;
    private float _desiredSpeedCurrent = 0;

    [Header("- Boat -")]
    [SerializeField, Tooltip("Mult * DeltaTime = speed obtained")]
    private float _boatSpeedMultiplier = 3f;
    [SerializeField] private float _boatSpeedMax = 8f;
    [SerializeField] private float _boatRotationMax = 60;
    private float _boatSpeedCurrent = 0;


    [Header("- Wave Effect -")]
    [SerializeField] private float _curPivotDeepValue = -.5f;
    [SerializeField] private float _maxInclinationAngle = 30f;
    [SerializeField, Range(0.05f, 1f)]
    private float _smoothingFactor = 0.1f;
    [SerializeField] private float _speedWaveRotation = 2;
    [SerializeField] private float _accelerationFactor = 1;


    [Header("- Not Modify -")]
    private Vector3 _inputDir = Vector3.zero;
    private Vector3 _inputRotate = Vector3.zero;
    private bool _isActive = true;
    private bool _isRotateActive = true;

    private float _frontBackDist;
    private float _rightLeftDist;

    //private const string ANIM_IS_MOVING = "isMoving";
    //private const string ANIM_MOVE_X = "moveX";
    //private const string ANIM_MOVE_Z = "moveZ";


    // - GET -
    public bool IsActive => _isActive;
    public bool IsRotateActive => _isRotateActive;


    // - PUBLIC -
    public void CreateMeter(RectTransform container)
    {
        Instantiate(_boatMeter.gameObject, container).GetComponent<Panel_BoatMeter>().AssignBoat(this);
    }
    public void CreateGroup(Panel_Stats panelStats)
    {
        Slot_GroupStats group = panelStats.CreateGroup("Move Boat");

        // Rudder
        group.CreateSlider("rudderAngleSpeed").Configure("Rudder: Angle Speed",
            _rudderSpeedAnlge, 1, 50, data => { _rudderSpeedAnlge = data; });
        group.CreateSlider("rudderAngleMax").Configure("Rudder: Angle Max",
            _rudderMaxAngle, 90, 360, data => { _rudderMaxAngle = data; });

        // Accel
        group.CreateSlider("desiredSpeed").Configure("Desired Speed: Speed",
            _desiredSpeedMultiplier, 1, 20, data => { _desiredSpeedMultiplier = data; });
        group.CreateSlider("desiredMax").Configure("Desired Speed: Max",
            _desiredSpeedMax, 1, 20, data => { _desiredSpeedMax = data; });

        // Boat
        group.CreateSlider("boatSpeed").Configure("Boat: Speed",
         _boatSpeedMultiplier, 1, 20, data => { _boatSpeedMultiplier = data; });
        group.CreateSlider("boatSpeedMax").Configure("Boat: Speed Max",
         _boatSpeedMax, 1, 20, data => { _boatSpeedMax = data; });
        group.CreateSlider("boatMaxRot").Configure("Boat: Max Rotation Speed",
            _boatRotationMax, 10, 120, data => { _boatRotationMax = data; });

        // Wave
        group.CreateSlider("maxIncliAngle").Configure("Wave: Max Inclination Angle",
            _maxInclinationAngle, 0, 90, data => { _maxInclinationAngle = data; });
        group.CreateSlider("waveSmoothFactor").Configure("Wave: Smooth Factor",
            _smoothingFactor, 0, 4, data => { _smoothingFactor = data; });

        // Water Manager
        _waterManager.CreateGroup(panelStats);
    }
    public void SetActive(bool isActive) => _isActive = isActive;
    public void SetRotateActive(bool isActive) => _isRotateActive = isActive;

    public void Move(Vector3 direction) => _inputDir = direction;
    public void Rotate(Vector3 direction) => _inputRotate = direction;

    public void Warp(Vector3 newPosition)
    {
        _inputDir = Vector3.zero;
        _myRigidbody.MovePosition(newPosition);
    }


    // - UNITY -
    void Awake()
    {
        _frontBackDist = Vector3.Distance(_frontPoint.position, _backPoint.position);
        _rightLeftDist = Vector3.Distance(_leftPoint.position, _rightPoint.position);
    }
    void Update()
    {
        float deltaTime = Time.deltaTime;

        UpdateRudder(_inputDir.x, deltaTime);
        UpdateDesiredSpeed(_inputDir.z, deltaTime);
        ApplyWaveRotation(deltaTime);
    }
    void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;

        // Calculate acceleration to reach desired accel
        float acceleration = (_desiredSpeedCurrent - _boatSpeedCurrent) * _accelerationFactor;
        // Update current speed
        _boatSpeedCurrent += acceleration * deltaTime;
        // limit to max speed
        if (_boatSpeedCurrent > _boatSpeedMax)
            _boatSpeedCurrent = _boatSpeedMax;

        if (_desiredSpeedCurrent > 0)
        {
            // Apply external forces (waves)
            Vector3 externalForces = _waterManager.CalculateExternalForces(_myRigidbody.position);
            float alignment = Vector3.Dot(_myRigidbody.transform.forward, externalForces.normalized);
            Vector3 adjustedForce = externalForces * alignment;
            _boatSpeedCurrent = (acceleration + adjustedForce.magnitude);
        }

        OnBoatSpeedChange?.Invoke(_boatSpeedCurrent, _boatSpeedMax);

        // Obtain height of wave at boat position
        float waveHeight = _waterManager.GetWaveHeight(_myRigidbody.position);

        // Move Forward 
        Vector3 forwardMovement = (transform.forward * _boatSpeedCurrent) * deltaTime;
        _myRigidbody.MovePosition(new Vector3(_myRigidbody.position.x, waveHeight + _curPivotDeepValue, _myRigidbody.position.z)
            + forwardMovement);
    }


    // - PRIVATE -
    void UpdateRudder(float rudderInput, float deltaTime)
    {
        // Si no tiene input, se queda donde está
        if (rudderInput != 0)
        {
            // Calcular el ángulo de rotación basándose en la entrada del timón
            _rudderCurrentAngle += rudderInput * (deltaTime * _rudderSpeedAnlge);
            _rudderCurrentAngle = Mathf.Clamp(_rudderCurrentAngle, -_rudderMaxAngle, _rudderMaxAngle);

            OnRudderRotation?.Invoke(_rudderCurrentAngle / _rudderMaxAngle);

            _rudderNormalized = _rudderCurrentAngle / _rudderMaxAngle;
        }

        // Rotar el barco aplicando el ángulo de rotación al Rigidbody
        float boatRot = (_rudderNormalized * _boatRotationMax) * deltaTime;

        _myRigidbody.rotation = Quaternion.Euler(0, boatRot, 0) * _myRigidbody.rotation;
    }
    void UpdateDesiredSpeed(float desiredSpeedInput, float deltaTime)
    {
        if (desiredSpeedInput == 0)
            return;

        _desiredSpeedCurrent += desiredSpeedInput * (_desiredSpeedMultiplier * deltaTime);

        if (_desiredSpeedCurrent > _desiredSpeedMax)
            _desiredSpeedCurrent = _desiredSpeedMax;
        else if (_desiredSpeedCurrent < 0)
            _desiredSpeedCurrent = 0;

        OnDesiredSpeedChange?.Invoke(_desiredSpeedCurrent, _desiredSpeedMax);
    }


    void ApplyWaveRotation(float deltaTime)
    {
        Vector3[] listPositions = { _frontPoint.position,
            _backPoint.position,
            _leftPoint.position,
            _rightPoint.position };

        float[] listHeights = _waterManager.GetWaveHeights(listPositions);


        // Obtener las alturas de las olas en los puntos de referencia
        float frontWaveHeight = listHeights[0]; // waterManager.GetWaveHeight(frontPoint.position);
        float backWaveHeight = listHeights[1]; //waterManager.GetWaveHeight(backPoint.position);
        float leftWaveHeight = listHeights[2]; //waterManager.GetWaveHeight(leftPoint.position);
        float rightWaveHeight = listHeights[3]; //waterManager.GetWaveHeight(rightPoint.position);

        // Convertir radianes a grados
        float toGrad = (180 / MathF.PI);
        // Front / Back
        float frontBack_dif = frontWaveHeight - backWaveHeight;
        float angleX = MathF.Atan(frontBack_dif / _frontBackDist) * toGrad;
        angleX = Mathf.Clamp(angleX, -_maxInclinationAngle, _maxInclinationAngle);
        //if (backWaveHeight > frontWaveHeight)
        //    angleX *= -1;
        // Right / Left
        float rightLeft_dif = rightWaveHeight - leftWaveHeight;
        float angleZ = MathF.Atan(rightLeft_dif / _rightLeftDist) * toGrad;
        angleZ = Mathf.Clamp(angleX, -_maxInclinationAngle, _maxInclinationAngle);
        //if (rightWaveHeight < leftWaveHeight)
        //    angleZ *= -1;

        //Quaternion targetRotation = Quaternion.Euler(angleX, 0, angleZ);
        //myRigidbody.MoveRotation(Quaternion.Slerp(myRigidbody.rotation, targetRotation, smoothingFactor));

        float boatRot = (_rudderNormalized * _boatRotationMax) * deltaTime;
        Quaternion targetRotation = Quaternion.Euler(angleX, _myRigidbody.rotation.eulerAngles.y, angleZ);
        _myRigidbody.MoveRotation(Quaternion.Slerp(_myRigidbody.rotation, targetRotation, _smoothingFactor));

        //float pitch = Mathf.Clamp(frontWaveHeight - backWaveHeight, -maxInclinationAngle, maxInclinationAngle);
        //float roll = Mathf.Clamp(rightWaveHeight - leftWaveHeight, -maxInclinationAngle, maxInclinationAngle);
        //Quaternion targetRotation = Quaternion.Euler(pitch, myRigidbody.rotation.eulerAngles.y, roll);
        //myRigidbody.MoveRotation(Quaternion.Slerp(myRigidbody.rotation, targetRotation, smoothingFactor));


        // Calcular las diferencias de altura para determinar la inclinación
        //float pitch = frontWaveHeight - backWaveHeight; // Inclinación en el eje X
        //float roll = rightWaveHeight - leftWaveHeight; // Inclinación en el eje Z

        // Aplicar la rotación basada en la inclinación calculada

        //Quaternion targetRotation = Quaternion.Euler(pitch * _deltaTime * speedWaveRotation, 
        //    0,
        //    roll * _deltaTime * speedWaveRotation);
        //myRigidbody.rotation *= targetRotation;
    }
}