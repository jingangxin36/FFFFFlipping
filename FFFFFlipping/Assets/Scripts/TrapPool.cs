using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class TrapPool : Singleton<TrapPool> {
    public CellType cellType;
    public float probability;
    public GameObject[] prefabs;

    public int Count =>
        mPool.Count;

    private int mPreCache;
    private Pool<GameObject> mPool;

    protected override void Awake() {

        base.Awake();
        mPool = new Pool<GameObject>(NewGameObject, 0);
    }

    private void Start() {
        mPreCache = (int)(BlockManager.Instance.maxSizeInSight * 6 * probability);
        for (int i = 0; i < mPreCache; i++) {
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

