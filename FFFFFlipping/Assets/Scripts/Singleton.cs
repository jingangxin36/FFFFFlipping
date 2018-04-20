using System;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
    protected static T instance;

    protected virtual void Awake() {
        if (Instance == null) {
            instance = GetComponent<T>();
        }
        else {
            Debug.LogError("Something went wrong.  There should never be more than one instance of " + typeof(T));
        }
    }

    public static T Instance => instance;

    public static bool InstanceExists => instance != null;

    public virtual void ReleaseIntoPool(GameObject go) {
        
    }
}

