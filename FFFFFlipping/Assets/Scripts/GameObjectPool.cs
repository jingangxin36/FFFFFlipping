using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class GameObjectPool : MonoBehaviour {
    public int preCache;
    public GameObject[] prefabs;

    public int Count =>
        mPool.Count;

    private Pool<GameObject> mPool;

    private void Awake() {
        mPool = new Pool<GameObject>(NewGameObject, 0);
    }

    private void Start() {
        for (int i = 0; i < preCache; i++) {
            GameObject go = NewGameObject();
            go.transform.parent = transform;
            Release(go, true);
        }
    }

    protected virtual GameObject NewGameObject() =>
        Instantiate(prefabs[UnityEngine.Random.Range(0, prefabs.Length)]);

    public virtual void Release(GameObject go, bool deactivate = true) {
        if (deactivate) {
            go.SetActive(false);
        }
        mPool.Release(go);
    }

    public virtual GameObject Take(bool random = true, bool activate = true) {
        GameObject obj2 = mPool.Take(true);
        if (activate) {
            obj2.SetActive(true);
        }
        return obj2;
    }
}

