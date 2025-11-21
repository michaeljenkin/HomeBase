/**
 * This maintains static information required over the experiments.
 * This must be initialized before being used
 * 
 * @author Michael Jenkin
 * @copyright 2025
 * 
 * Version History
 * 
 * V1.0 - based on V8.0 of the ISS software
 **/

using UnityEngine;
public static class PersistentData  {
    public static string VectionFile;
    public static string JSONFile;
    public static int SubjectNumber;

    /*
     * Find a child by name. Returns null if none.
     */
    public static GameObject GetChildByName(GameObject obj, string s)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            if (obj.transform.GetChild(i).gameObject.name.Equals(s))
                return obj.transform.GetChild(i).gameObject;
        }
        Debug.Log("There was no child object " + s + " on object " + obj.name);
        return null;
    }
}
