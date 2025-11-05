using UnityEngine;

/**
 * Create the basic experimental environment.
 * 
 * Copyright Michael Jenkin 2025
 * Version History
 * 
 * V1.2 - set up for new two task version
 * V1.1 - use a pointer for the final task rather than moving the point of view.
 *      - do some general cleanup in code style
 * V1.0 - based on the OSC software
 **/

public class WolrdCreator : MonoBehaviour
{

    private enum UIState
    {
        Initialize,
        WelcomeScreen,
        ConfirmScreen,
        DoingExperiment,
        ExperimentDone,
    };

    private enum ExperimentState
    {
        BeforeMotion,
        LegOne,
        AdjustTarget1,
        Turning,
        AdjustOrientation,
        LegTwo,
        AdjustTarget2,
        TargetDirection,
        TargetDistance,
        Done
    };

    private const float _motionStep = 0.01f; // how big a step to make for a keypress
    private const float _turnStep = 0.1f; // step size for turn keypress
    private const float _reticleDistance = 1.0f; // distance to orientaiton reticle
    private const float _velocity = 2.0f;  // speed along a straight line
    private const float _spinV = 30.0f;  // abs rotational velocity

    

    private UIState _uiState = UIState.Initialize;


    private const float SLEEP_TIME = 0.0f;
    private SphereField sf;
    private ExperimentState experimentState = ExperimentState.BeforeMotion;
    private float _motion1Start = 0.0f;
    private float _turnStart = 0.0f;
    private float _sleepStart = 0.0f;
    private float _motion2Start = 0.0f;

    private float _length1 = 4.0f;
    private float _length2 = 6.0f;
    private float _turn = 60.0f;  // absolute value of turn

    private float _spinDir = 1.0f;  // +1 right, -1 left

    private bool _pitch = true;   // pitch (true) or yaw (false)
    private bool _turnRight = false; // true is to the rigght (yaw) or up (pitch)
    private float _pan, _tilt;

    private GameObject _adjustTarget = null;
    private GameObject _reticle = null;
    private GameObject _camera = null;
    private GameObject _welcome = null;
    private GameObject _dialog = null;
    private GameObject _home = null;

    private float _targetDistance1, _targetDistance2, _turnAngle, _directionAngle, _directionDistance;

    private const int NCONDS = 12;
    private int _cond = 0;
    private int _experiment = -1;
    
    float[] c1 = new float[5] { 4.0f, 4.0f, 70.0f, 1.0f, 1.0f }; // l1, l2, theta, pan/tilt, dir1/dir2
    float[] c2 = new float[5] { 4.0f, 4.0f, 105.0f, 1.0f, -1.0f }; // l1, l2, theta, pan/tilt, dir1/dir2
    float[] c3 = new float[5] { 4.0f, 4.0f, 120.0f, -1.0f, 1.0f }; // l1, l2, theta, pan/tilt, dir1/dir2
    float[] c4 = new float[5] { 4.0f, 8.0f, 70.0f, -1.0f, -1.0f }; // l1, l2, theta, pan/tilt, dir1/dir2
    float[] c5 = new float[5] { 4.0f, 8.0f, 105.0f, 1.0f, 1.0f }; // l1, l2, theta, pan/tilt, dir1/dir2
    float[] c6 = new float[5] { 4.0f, 8.0f, 120.0f, 1.0f, 1.0f }; // l1, l2, theta, pan/tilt, dir1/dir2
    float[] c7 = new float[5] { 8.0f, 4.0f, 70.0f, 1.0f, 1.0f }; // l1, l2, theta, pan/tilt, dir1/dir2
    float[] c8 = new float[5] { 8.0f, 4.0f, 105.0f, 1.0f, 1.0f }; // l1, l2, theta, pan/tilt, dir1/dir2
    float[] c9 = new float[5] { 8.0f, 4.0f, 120.0f, 1.0f, 1.0f }; // l1, l2, theta, pan/tilt, dir1/dir2
    float[] c10 = new float[5] { 8.0f, 8.0f, 70.0f, 1.0f, 1.0f }; // l1, l2, theta, pan/tilt, dir1/dir2
    float[] c11 = new float[5] { 8.0f, 8.0f, 105.0f, 1.0f, 1.0f }; // l1, l2, theta, pan/tilt, dir1/dir2
    float[] c12 = new float[5] { 8.0f, 8.0f, 120.0f, 1.0f, 1.0f }; // l1, l2, theta, pan/tilt, dir1/dir2
    float[][] _conditions = new float[12][];
    
    /**
     * All of the non-scene texture objects are initially active (so they are easy to find). 
     * Get links to them, then make them inactive (except for the camera :-)
     **/
    private void GetGameObjects()
    {
        _adjustTarget = GameObject.Find("Target");
        _adjustTarget.SetActive(false);

        _reticle = GameObject.Find("Reticle");
        _reticle.SetActive(false);

        _camera = GameObject.Find("Camera Holder");

        _dialog = GameObject.Find("Dialog");
        _dialog.SetActive(false);

        _home = GameObject.Find("Homebase");
        _home.SetActive(false);

        _welcome = GameObject.Find("Welcome");
        _welcome.SetActive(false);
    }

    /**
     * Start is called once before the first execution of Update after the MonoBehaviour is created
     *
     **/
    void Start()
    {
        GetGameObjects();
        _adjustTarget.transform.localScale = new Vector3(SphereField.HALLWAY_RADIUS, SphereField.HALLWAY_RADIUS, SphereField.HALLWAY_RADIUS);

        Debug.Log("In WordCreator");
        _conditions[0] = c1;
        _conditions[1] = c2;
        _conditions[2] = c3;
        _conditions[3] = c4;
        _conditions[4] = c5;
        _conditions[5] = c6;
        _conditions[6] = c7;
        _conditions[7] = c8;
        _conditions[8] = c9;
        _conditions[9] = c10;
        _conditions[10] = c11;
        _conditions[11] = c12;



    }

    /**
     * This is called once every time tick. The entire experiment's UI is in one of a small
     * number of states. The work of the real experiment is done elsewhere
     **/
    void Update()
    {
        Dialog d = _dialog.GetComponent<Dialog>();
        Debug.Log("Update");

        switch (_uiState)
        {
            case UIState.Initialize:
                d.SetDialogTitle("Homebase");
                d.SetDialogElements("Choose Experiment", new string[] { "Adjust Target?, "Where Did I Go?" });
                       
                _dialog.SetActive(true);
                _uiState = UIState.WelcomeScreen;
                break;
            case UIState.WelcomeScreen:
                Debug.Log("Welcome screen");
                int response = d.GetResponse();
                Debug.Log(response);
                if (response >= 0)
                {
                    _experiment = response;
                    string[] confirmString = { "????", "Back" };
                    if (_experiment == 0)
                        confirmString[0] = "Do 'Adjust Target'";
                    else
                        confirmString[0] = "Do 'Where Did I Go'";
                    d.SetDialogElements("Confirm Choice", confirmString);
                    _uiState = UIState.ConfirmScreen;
                }
                break;
            case UIState.ConfirmScreen:
                int confirm = d.GetResponse();
                if (confirm >= 0)
                {
                    if (confirm == 0)
                    {
                        _uiState = UIState.DoingExperiment;
                        _dialog.SetActive(false);
                    } else // back
                    {
                        _uiState = UIState.Initialize;
                    }
                }
                break;
            case UIState.DoingExperiment:
                if (_experiment == 0)
                    DoAdjustTarget();
                else
                    DoWhereDidIGo();
                break;
            case UIState.ExperimentDone:
                break;
        }
    }

    private void DoAdjustTarget()
    {
    }

    private void DoWhereDidIGo()
    {
                float _distance, _angle;
        float x, y, z, tx, ty, tz;
        switch (experimentState)
        {
            case ExperimentState.BeforeMotion: // waiting befofre the first arm (of length _length1)
                Debug.Log("Before motion");
                if ((_cond > 0)||Input.GetKeyDown("z"))
                {
                    _length1 = _conditions[_cond][0];
                    _length2 = _conditions[_cond][1];
                    _turn = 180 - _conditions[_cond][2]; // angles measured the other way
                    _pitch = _conditions[_cond][3] > 0;
                    _turnRight = _conditions[_cond][4] > 0;

                    sf = new SphereField(12000, _length1, _length2, _turn, _turnRight, _pitch);
                    sf.EnableFirstHallway();
                    Debug.Log("STARTING CONDITION");
                    Debug.Log(_cond);


                    // set +ve spin direction depending on pitch and turnRight values
                    if (_pitch)
                    {
                        if (_turnRight)
                            _spinDir = -1.0f;
                        else
                            _spinDir = 1.0f;
                    }
                    else
                    {
                        if (_turnRight)
                            _spinDir = 1.0f;
                        else
                            _spinDir = -1.0f;
                    }



                    _welcome.SetActive(false);
                    _motion1Start = Time.time;
                    _distance = 0.0f;
                    _camera.transform.position = new Vector3(0, 0, _distance);
                    experimentState = ExperimentState.LegOne;
                }
                break;
            case ExperimentState.LegOne: // moving in direction (0,0,1) to _length1
                _distance = _velocity * (Time.time - _motion1Start);
                _camera.transform.position = new Vector3(0, 0, _distance);
                if (_distance >= _length1) // got there, do the adjust target task
                {
                    _distance = _length1;
                    _camera.transform.position = new Vector3(0, 0, _distance);
                    experimentState = ExperimentState.AdjustTarget1;
                    _targetDistance1 = UnityEngine.Random.Range(1.5f, 2.0f * _length1);
                    _adjustTarget.transform.position = new Vector3(0, 0, _targetDistance1 + _length1);
                    _adjustTarget.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                    _adjustTarget.SetActive(true);
                }
                break;
            case ExperimentState.AdjustTarget1: // first adjust target task (arrows to adjust, x to select)
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    _targetDistance1 = Mathf.Min(_targetDistance1 + _motionStep, 2.0f * _length1);
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    _targetDistance1 = Mathf.Max(_targetDistance1 - _motionStep, 0.1f * _length1);
                }
                _adjustTarget.transform.position = new Vector3(0, 0, _targetDistance1 + _length1);
                if (Input.GetKeyDown("x"))
                {
                    sf.EnableElbow();
                    experimentState = ExperimentState.Turning;
                    _turnStart = Time.time;
                    _adjustTarget.SetActive(false);
                }
                break;
            case ExperimentState.Turning: // rotate about the angle bewteen the two lengths (_turn an amplitude)
                _angle = _spinV * (Time.time - _turnStart);
                if (_angle >= _turn) // rotation is finished
                {
                    _angle = _turn;
                    experimentState = ExperimentState.AdjustOrientation;
                    if (_pitch)
                    {
                        _tilt = _spinDir * _angle;
                        Debug.Log("Creating recticle");
                        Debug.Log(_tilt);
                        tx = 0;
                        ty = -_reticleDistance * Mathf.Sin(3.1415f * _tilt / 180.0f);
                        tz = _reticleDistance * Mathf.Cos(3.1415f * _tilt / 180.0f) + _length1;
                        _reticle.transform.rotation = Quaternion.Euler(_tilt, 0.0f, 0.0f);
                        _reticle.transform.position = new Vector3(tx, ty, tz);
                    }
                    else
                    {
                        _pan = _spinDir * _angle;
                        tx = _reticleDistance * Mathf.Sin(3.1415f * _pan / 180.0f);
                        ty = 0;
                        tz = _reticleDistance * Mathf.Cos(3.1415f * _pan / 180.0f) + _length1;
                        _reticle.transform.rotation = Quaternion.Euler(0.0f, _pan, 0.0f);
                        _reticle.transform.position = new Vector3(tx, ty, tz);
                    }

                    _reticle.SetActive(true);
                    _turnAngle = 0.0f;
                    _sleepStart = Time.time;
                }
                else
                {
                    if (_pitch)
                    {
                        _pan = 0.0f;
                        _tilt = _spinDir * _angle;
                        _camera.transform.rotation = Quaternion.Euler(_tilt, _pan, 0.0f);
                    }
                    else
                    {
                        _pan = _spinDir * _angle;
                        _tilt = 0.0f;
                        _camera.transform.rotation = Quaternion.Euler(_tilt, _pan, 0.0f);
                    }
                }
                break;
            case ExperimentState.AdjustOrientation: // indicate orientation we just went through
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    _turnAngle = Mathf.Min(_turnAngle + _turnStep, 180.0f);
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    _turnAngle = Mathf.Max(_turnAngle - _turnStep, -180.0f);
                }
                if (Input.GetKeyDown("x")) // make the hall appear
                {
                    experimentState = ExperimentState.LegTwo;

                    _reticle.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                    _reticle.transform.position = new Vector3(0.0f, 0.0f, _reticleDistance + _length1);
                    _reticle.SetActive(false);
                    sf.CutHoleInElbow();
                    sf.RotateSecondHallway();
                    sf.EnableElbowAndHallway(); // now with view cut in it
                    _motion2Start = Time.time;
                }
                else
                {
                    if (_pitch)
                    {
                        _tilt = _spinDir * _turn;
                        tx = 0;
                        ty = -_reticleDistance * Mathf.Sin(3.1415f * (_tilt - _turnAngle) / 180.0f);
                        tz = _reticleDistance * Mathf.Cos(3.1415f * (_tilt - _turnAngle) / 180.0f) + _length1;
                        _reticle.transform.rotation = Quaternion.Euler(_tilt - _turnAngle, 0.0f, 0.0f);
                        _reticle.transform.position = new Vector3(tx, ty, tz);
                    }
                    else
                    {
                        _pan = _spinDir * _turn;
                        tx = _reticleDistance * Mathf.Sin(3.1415f * (_pan + _turnAngle) / 180.0f);
                        ty = 0;
                        tz = _reticleDistance * Mathf.Cos(3.1415f * (_pan + _turnAngle) / 180.0f) + _length1;
                        _reticle.transform.rotation = Quaternion.Euler(0.0f, _pan + _turnAngle, 0.0f);
                        _reticle.transform.position = new Vector3(tx, ty, tz);
                    }
                }
                break;
            case ExperimentState.LegTwo: // present stimulus for leg 2
                _distance = _velocity * (Time.time - _motion2Start);
                if (_distance >= _length2) // got to the stimulus distance for leg 2
                {
                    _distance = _length2;
                    experimentState = ExperimentState.AdjustTarget2;

                    _targetDistance2 = UnityEngine.Random.Range(1.5f, 2.0f * _length2);

                    if (_pitch)
                    {
                        tx = 0;
                        ty = -(_length2 + _targetDistance2) * Mathf.Sin(3.1415f * _spinDir * _turn / 180.0f);
                        tz = (_length2 + _targetDistance2) * Mathf.Cos(3.1415f * _spinDir * _turn / 180.0f) + _length1;
                        _adjustTarget.transform.rotation = Quaternion.Euler(_spinDir * _turn, 0.0f, 0.0f);
                    }
                    else
                    {
                        tx = (_length2 + _targetDistance2) * Mathf.Sin(3.1415f * _spinDir * _turn / 180.0f);
                        ty = 0;
                        tz = (_length2 + _targetDistance2) * Mathf.Cos(3.1415f * _spinDir * _turn / 180.0f) + _length1;
                        _adjustTarget.transform.rotation = Quaternion.Euler(0.0f, _spinDir * _turn, 0.0f);
                    }
                    _adjustTarget.transform.position = new Vector3(tx, ty, tz);
                    _adjustTarget.SetActive(true);
                    break;
                }

                if (_pitch)
                {
                    x = 0;
                    y = -_distance * Mathf.Sin(3.1415f * _spinDir * _turn / 180.0f);
                    z = _distance * Mathf.Cos(3.1415f * _spinDir * _turn / 180.0f) + _length1;
                }
                else
                {
                    x = _distance * Mathf.Sin(3.1415f * _spinDir * _turn / 180.0f);
                    y = 0;
                    z = _distance * Mathf.Cos(3.1415f * _spinDir * _turn / 180.0f) + _length1;
                }
                _camera.transform.position = new Vector3(x, y, z);
                break;

            case ExperimentState.AdjustTarget2: // do the adjust target task for leg 2
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    _targetDistance2 = Mathf.Min(_targetDistance2 + _motionStep, 2.0f * _length2);
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    _targetDistance2 = Mathf.Max(_targetDistance2 - _motionStep, 0.1f * _length2);
                }


                if (_pitch)
                {
                    tx = 0;
                    ty = -(_length2 + _targetDistance2) * Mathf.Sin(3.1415f * this._spinDir * this._turn / 180.0f);
                    tz = (_length2 + _targetDistance2) * Mathf.Cos(3.1415f * this._spinDir * this._turn / 180.0f) + _length1;
                }
                else
                {
                    tx = (_length2 + _targetDistance2) * Mathf.Sin(3.1415f * this._spinDir * this._turn / 180.0f);
                    ty = 0;
                    tz = (_length2 + _targetDistance2) * Mathf.Cos(3.1415f * this._spinDir * this._turn / 180.0f) + _length1;
                }
                _adjustTarget.transform.position = new Vector3(tx, ty, tz);

                if (Input.GetKeyDown("x")) // teleport to origin and set up for pointing task
                {
                    experimentState = ExperimentState.TargetDirection;
                    sf.EnableHomeBaseSphere();
                    sf.ResetSecondHallway();
                    _adjustTarget.SetActive(false);
                    _reticle.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                    _reticle.transform.position = new Vector3(0.0f, 0.0f, _reticleDistance);
                    _reticle.SetActive(false);
                    _reticle.SetActive(true);
                    _camera.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
                    _camera.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                    _directionAngle = 0.0f;
                }
                break;
            case ExperimentState.TargetDirection: // point in direction to goal location
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    _directionAngle = Mathf.Min(_directionAngle + _turnStep, 180.0f);
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    _directionAngle = Mathf.Max(_directionAngle - _turnStep, -180.0f);
                }
                if (Input.GetKeyDown("x")) // make the hallway appear (note, not necessarily in front of viewer)
                {
                    _reticle.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
                    _reticle.SetActive(false);
                    _adjustTarget.SetActive(true);
                    sf.CutHoleInHomeBaseSphere(_directionAngle);
                    sf.RotateHomeBaseDisplay(_directionAngle);
                    sf.EnableHomeBaseDisplay();
                    experimentState = ExperimentState.TargetDistance;
                    _directionDistance = UnityEngine.Random.Range(1.5f, (_length1 + _length2) * 2);

                    if (_pitch)
                    {
                        tx = 0;
                        ty = _directionDistance * Mathf.Sin(3.1415f * _directionAngle / 180.0f);
                        tz = _directionDistance * Mathf.Cos(3.1415f * _directionAngle / 180.0f);
                        _adjustTarget.transform.rotation = Quaternion.Euler(-_directionAngle, 0.0f, 0.0f);
                    }
                    else
                    {
                        tx = _directionDistance * Mathf.Sin(3.1415f * _directionAngle / 180.0f);
                        ty = 0;
                        tz = _directionDistance * Mathf.Cos(3.1415f * _directionAngle / 180.0f);
                        _adjustTarget.transform.rotation = Quaternion.Euler(0.0f, _directionAngle, 0.0f);
                    }
                    _adjustTarget.transform.position = new Vector3(tx, ty, tz);
                    _adjustTarget.SetActive(true);
                }
                else
                {
                    if (_pitch)
                    {
                        ty = _reticleDistance * Mathf.Sin(3.1415f * _directionAngle / 180.0f);
                        tz = _reticleDistance * Mathf.Cos(3.1415f * _directionAngle / 180.0f);
                        _reticle.transform.position = new Vector3(0.0f, ty, tz);
                        _reticle.transform.rotation = Quaternion.Euler(-_directionAngle, 0.0f, 0.0f);
                    }
                    else
                    {
                        tx = _reticleDistance * Mathf.Sin(3.1415f * _directionAngle / 180.0f);
                        tz = _reticleDistance * Mathf.Cos(3.1415f * _directionAngle / 180.0f);
                        _reticle.transform.position = new Vector3(tx, 0.0f, tz);
                        _reticle.transform.rotation = Quaternion.Euler(0.0f, _directionAngle, 0.0f);
                    }
                }
                break;
            case ExperimentState.TargetDistance:
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    _directionDistance = Mathf.Min(_directionDistance + _motionStep, 2.0f * (_length1 + _length2));
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    _directionDistance = Mathf.Max(_directionDistance - _motionStep, 1.5f);
                }

                if (_pitch)
                {
                    tx = 0;
                    ty = _directionDistance * Mathf.Sin(3.1415f * _directionAngle / 180.0f);
                    tz = _directionDistance * Mathf.Cos(3.1415f * _directionAngle / 180.0f);
                }
                else
                {
                    tx = _directionDistance * Mathf.Sin(3.1415f * _directionAngle / 180.0f);
                    ty = 0;
                    tz = _directionDistance * Mathf.Cos(3.1415f * _directionAngle / 180.0f);
                }
                _adjustTarget.transform.position = new Vector3(tx, ty, tz);
                //_adjustTarget.transform.rotation = Quaternion.Euler(0.0f, _directionAngle, 0.0f);

                if (Input.GetKeyDown("x")) // done
                {
                    experimentState = ExperimentState.Done;
                    _adjustTarget.SetActive(false);
                    sf.EnableAll(false);

                }
                break;
            case ExperimentState.Done:
                if (_cond < NCONDS-1)
                {
                    _cond = _cond + 1;
                    experimentState = ExperimentState.BeforeMotion;
                } else
                {
                    _welcome.SetActive(true);
                }
                break;
        }

    }

    void setTargetPosition(string item, float x, float y, float z)
    {
        GameObject target = GameObject.Find(item);
        target.transform.position = new Vector3(x, y, z);
    }

}
