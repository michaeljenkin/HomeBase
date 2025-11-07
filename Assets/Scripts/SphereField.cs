/**
 * Create a field of spheres (all of one type at present). This has been modified somewhat to
 * reflect changes in the planned way in which the experiment takes place.
 *
 * We reflect and rotate this for differnet conditions. A pan to the right is assumed by default
 * We now generate a solid volume of randomly seeded objects and then cut out the space we need. 
 * The constructor now just creates N objects in a horizontal volume
 *
 * 
 * Copyright Michael Jenkin 2025
 * Version History
 * 
 * V3.0 - yet anothr substantive re-write of how to generate the random field.
 * V2.0 - substantive update based on revised experimental structure
 * V1.0 - based on the OSC software
 **/
using UnityEngine;
using System;

public class SphereField {
    public const float SPHERE_RADIUS = 0.8f;
    public const float HALLWAY_RADIUS = (3.3f / 2.0f); // hallway radius
    private const float OUTER_HALLWAY_MULTIPLIER = 5.0f; //how thick to make the hallway walls
    private const float MAX_HALLWAY_LENGTH = 30.0f;

    private const int N_SPHERE_TYPES = 1; // how many types of spheres are there. Currently 1


    public const int LEG1 = 0;
    public const int ELBOW = 1;
    public const int LEG2 = 2;
    public const int HOMESPHERE = 3;
    public const int NSEGS = 4;


    private class Blob {
        public float X;
        public float Z;
        public float Y;
        public GameObject Obj;
        public bool[] IsIn;
    }


    private Blob[] _blobs;

    private int _n;
    private float _firstLength, _secondLength, _turnAngle;
    private bool _turnRight, _pitch;

    /**
     * Create N spheres in the volume defined above. We will now create n balls ...
     */
    public SphereField(int n, float firstLength, float secondLength, float turnAngle, bool turnRight, bool pitch)
    {
        _n = n;
        _firstLength = firstLength;
        _secondLength = secondLength;
        _turnAngle = turnAngle;
        _turnRight = turnRight;
        _pitch = pitch;
        _blobs = new Blob[n];
        UnityEngine.Object sphere = Resources.Load("TexturedSphere01");
        for (int i = 0; i < n;)
        {
            // all spheres are in a long rectangular solid
            float x = UnityEngine.Random.Range(-HALLWAY_RADIUS * OUTER_HALLWAY_MULTIPLIER, HALLWAY_RADIUS * OUTER_HALLWAY_MULTIPLIER);
            float y = UnityEngine.Random.Range(-HALLWAY_RADIUS * OUTER_HALLWAY_MULTIPLIER, HALLWAY_RADIUS * OUTER_HALLWAY_MULTIPLIER);
            float z = UnityEngine.Random.Range(-HALLWAY_RADIUS * OUTER_HALLWAY_MULTIPLIER, MAX_HALLWAY_LENGTH);

            // and inside a large cylinder
            if (x * x + y * y > HALLWAY_RADIUS * OUTER_HALLWAY_MULTIPLIER * HALLWAY_RADIUS * OUTER_HALLWAY_MULTIPLIER)
                continue;

            _blobs[i] = new Blob();
            _blobs[i].IsIn = new bool[NSEGS];
            _blobs[i].IsIn[0] = InLineRegion(x, y, z, 0, 0, 0, 0, 0, 1, OUTER_HALLWAY_MULTIPLIER * HALLWAY_RADIUS) &
                                !InLineRegion(x, y, z, 0, 0, 0, 0, 0, 1, HALLWAY_RADIUS);
            _blobs[i].IsIn[1] = InSphereRegion(x, y, z, 0, 0, _firstLength, OUTER_HALLWAY_MULTIPLIER * HALLWAY_RADIUS) &
                                !InSphereRegion(x, y, z, 0, 0, _firstLength, HALLWAY_RADIUS);
            _blobs[i].IsIn[2] = InSecondLineRegion(x, y, z, 0, 0, _firstLength, 0, 0, 1, HALLWAY_RADIUS * OUTER_HALLWAY_MULTIPLIER) &
                                !InSecondLineRegion(x, y, z, 0, 0, _firstLength, 0, 0, 1, HALLWAY_RADIUS);
            _blobs[i].IsIn[3] = InSphereRegion(x, y, z, 0, 0, 0, OUTER_HALLWAY_MULTIPLIER * HALLWAY_RADIUS) &
                                !InSphereRegion(x, y, z, 0, 0, 0, HALLWAY_RADIUS);

            bool keeper = false;
            for (int j = 0; j < NSEGS; j++)
                keeper = keeper || _blobs[i].IsIn[j];

            if (keeper)
            {
                _blobs[i].Obj = (GameObject)UnityEngine.Object.Instantiate(sphere);
                _blobs[i].X = x;
                _blobs[i].Y = y;
                _blobs[i].Z = z;
                _blobs[i].Obj.transform.position = new Vector3(_blobs[i].X, _blobs[i].Y, _blobs[i].Z);
                _blobs[i].Obj.transform.localScale = new Vector3(SPHERE_RADIUS, SPHERE_RADIUS, SPHERE_RADIUS);
                _blobs[i].Obj.SetActive(false);
                i++;
            }

        }
        this.EnableAll(false);
        Debug.Log("Blobs created");
    }

    public void DestroyGameObjects()
    {
        EnableAll(false);
        for (int j = 0; j < NSEGS; j++)
        {
            UnityEngine.Object.Destroy(_blobs[j].Obj);
        }
    }


    /**
     * Compute T value for point on a line starting at (px,py,pz) with direction vector
     * (dx,dy,dz). (dx,dy,dz) is a unit vector
     */
    private float ClosestPointT(float x, float y, float z, float px, float py, float pz,
                         float dx, float dy, float dz) {

        //t = p - x
        float tx = x - px;
        float ty = y - py;
        float tz = z - pz;

        // d = tx dot d 
        return (tx * dx + ty * dy + tz * dz);
    }

    /**
     * Is the point (x,y,z) within the one-ended tube starting at (px, py, pz) pointing in (dx,dy,dz)
     */
    private bool InLineRegion(float x, float y, float z, float px, float py, float pz,
                         float dx, float dy, float dz, float radius)
    {

        float t = ClosestPointT(x, y, z, px, py, pz, dx, dy, dz);


        float dist;
        if (t > 0)
        {
            float cx = px + t * dx;
            float cy = py + t * dy;
            float cz = pz + t * dz;
            dist = Mathf.Sqrt((cx - x) * (cx - x) + (cy - y) * (cy - y) + (cz - z) * (cz - z));
            if (dist > radius)
                return (false);
            return (true);
        }
        dist = (float)Math.Sqrt((px - x) * (px - x) + (py - y) * (py - y) + (pz - z) * (pz - z));
        return (dist <= radius);
    }

    /**
     * Is the point (x,y,z) within the open-ended tube starting at (px, py, pz) pointing in (dx,dy,dz)
     * but with a flat bottom to the cylinder
     */
    private bool InSecondLineRegion(float x, float y, float z, float px, float py, float pz,
                         float dx, float dy, float dz, float radius) {

        float t = ClosestPointT(x, y, z, px, py, pz, dx, dy, dz);
        if (t < 0)
            return (false);

        float dist;

        float cx = px + t * dx;
        float cy = py + t * dy;
        float cz = pz + t * dz;
        dist = Mathf.Sqrt((cx - x) * (cx - x) + (cy - y) * (cy - y) + (cz - z) * (cz - z));
        return (dist <= radius);
    }

    /**
     * Is the point (x,y,z) within the sphere at (px, py, pz) with radius radius
     */
    private bool InSphereRegion(float x, float y, float z, float px, float py, float pz, float radius)
    {
        float dist = Mathf.Sqrt((x - px) * (x - px) + (y - py) * (y - py) + (z - pz) * (z - pz));

        return (dist <= radius);
    }

    public void EnableAll(bool state)
    {
        for (int i = 0; i < _n; i++)
            _blobs[i].Obj.SetActive(state);
    }

    /**
     * First Hallway. The participant is at (0,0,0) facing in the z direction
     **/
    public void EnableFirstHallway()
    {
        for (int i = 0; i < _n; i++)
            _blobs[i].Obj.SetActive(_blobs[i].IsIn[0]);
    }

    /**
     * Enable the elbow
     **/
    public void EnableElbow()
    {
        for (int i = 0; i < _n; i++)
            _blobs[i].Obj.SetActive(_blobs[i].IsIn[1]);
    }

    /**
     * Enable the elbow and hallway
     **/
    public void EnableElbowAndHallway()
    {
        for (int i = 0; i < _n; i++)
            _blobs[i].Obj.SetActive(_blobs[i].IsIn[1] || _blobs[i].IsIn[2]);
    }

    /**
     * Cut the hole in the elbow based on angle, left and pitch
     **/
    public void CutHoleInElbow()
    {
        float spinDir = 1.0f;
        float turn = _turnAngle;
        if (_turnRight)
            spinDir = 1.0f;
        else
            spinDir = -1.0f;

        for (int i = 0; i < _n; i++)
        {
            bool kill = false;
            if (_blobs[i].IsIn[1])
            {
                if (_pitch)
                    kill = InSecondLineRegion(_blobs[i].X, _blobs[i].Y, _blobs[i].Z, 0, 0, _firstLength, 0, Mathf.Sin(3.1415f * turn * spinDir / 180.0f), Mathf.Cos(3.1415f * spinDir * turn / 180.0f), HALLWAY_RADIUS);
                else
                    kill = InSecondLineRegion(_blobs[i].X, _blobs[i].Y, _blobs[i].Z, 0, 0, _firstLength, Mathf.Sin(3.1415f * turn * spinDir / 180.0f), 0, Mathf.Cos(3.1415f * spinDir * turn / 180.0f), HALLWAY_RADIUS);
                _blobs[i].IsIn[1] = !kill;
            }

        }
    }

    /**
     * Rotate the second hallway
     */

    public void RotateSecondHallway()
    {
        float spinDir;
        float turn = _turnAngle;
        if (_turnRight)
            spinDir = 1.0f;
        else
            spinDir = -1.0f;
        Vector3 p = new Vector3(0.0f, 0.0f, _firstLength);
        Vector3 right = new Vector3(-1.0f, 0.0f, 0.0f);
        Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
        for (int i = 0; i < _n; i++)
        {
            if (!_blobs[i].IsIn[1] && _blobs[i].IsIn[2])
            {
                if (_pitch)
                    _blobs[i].Obj.transform.RotateAround(p, right, turn * spinDir);
                else
                    _blobs[i].Obj.transform.RotateAround(p, up, turn * spinDir);
            }
        }
    }


    /**
     * reset second hallway so that it is not rotated
     **/
    public void ResetSecondHallway()
    {
        for (int i = 0; i < _n; i++)
        {
            if (_blobs[i].IsIn[2])
            {
                _blobs[i].Obj.transform.position = new Vector3(_blobs[i].X, _blobs[i].Y, _blobs[i].Z);
                _blobs[i].Obj.transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
            }
        }
    }

    /**
     * Display the second hallway
     **/
    public void EnableSecondHallway()
    {
        for (int i = 0; i < _n; i++)
            _blobs[i].Obj.SetActive(_blobs[i].IsIn[2]);
    }

    /**
     * Enable a sphere field at home base
     **/
    public void EnableHomeBaseSphere()
    {
        for (int i = 0; i < _n; i++)
            _blobs[i].Obj.SetActive(_blobs[i].IsIn[3]);
    }

    /**
     * Cut the hole in the sphere at the origin based on angle, left and pitch
     **/
    public void CutHoleInHomeBaseSphere(float turn)
    {
        for (int i = 0; i < _n; i++)
        {
            bool kill = false;
            if (_blobs[i].IsIn[3])
            {
                if (_pitch)
                    kill = InSecondLineRegion(_blobs[i].X, _blobs[i].Y, _blobs[i].Z, 0, 0, 0, 0, Mathf.Sin(3.1415f * turn / 180.0f), Mathf.Cos(3.1415f * turn / 180.0f), HALLWAY_RADIUS);
                else
                    kill = InSecondLineRegion(_blobs[i].X, _blobs[i].Y, _blobs[i].Z, 0, 0, 0, Mathf.Sin(3.1415f * turn / 180.0f), 0, Mathf.Cos(3.1415f * turn / 180.0f), HALLWAY_RADIUS);
                _blobs[i].IsIn[3] = !kill;
            }

        }
    }

    /**
     * Rotate the hallway for home base display
     */
    public void RotateHomeBaseDisplay(float angle)
    {
        Vector3 p = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 right = new Vector3(-1.0f, 0.0f, 0.0f);
        Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
        for (int i = 0; i < _n; i++)
        {
            if (!_blobs[i].IsIn[3] && _blobs[i].IsIn[0])
            {
                if (_pitch)
                    _blobs[i].Obj.transform.RotateAround(p, right, angle);
                else
                    _blobs[i].Obj.transform.RotateAround(p, up, angle);
            }
        }
    }
    
    /**
     * Enable the distance from home base display
     */
    public void EnableHomeBaseDisplay()
    {
        for (int i = 0; i < _n; i++)
        {
            if (_blobs[i].IsIn[0] && _blobs[i].Z < 0)
                continue;
            _blobs[i].Obj.SetActive(_blobs[i].IsIn[0] || _blobs[i].IsIn[2] || _blobs[i].IsIn[3]);
        }
    }


}
