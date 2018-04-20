using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Pool<T> {
    protected List<T> objects;
    protected Func<T> provider;
    /// <summary>
    /// 池子里面item的数量
    /// </summary>
    public int Count => objects.Count;

    public Pool(Func<T> provider, int prefill = 0) {
        if (provider == null) {
            throw new ArgumentNullException($"Pool provider function");
        }
        objects = new List<T>();
        this.provider = provider;
        for (int i = 0; i < prefill; i++) {
            Release(provider());
        }
    }
    /// <summary>
    /// 清理池子
    /// </summary>
    public void Clear() {
        objects.Clear();
    }
    /// <summary>
    /// 池子里面是否存在item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(T item) => objects.Contains(item);
    /// <summary>
    /// 放回到池子里面
    /// </summary>
    /// <param name="item"></param>
    public void Release(T item) {
        objects.Add(item);
    }

    /// <summary>
    /// 从池子里面取出, random默认为false
    /// </summary>
    /// <param name="random"></param>
    /// <returns></returns>
    public T Take(bool random = false) {
        if (objects.Count > 0) {
            int index = objects.Count - 1;
            if (random) {
                index = UnityEngine.Random.Range(0, objects.Count);
            }
            T local = objects[index];
            objects.RemoveAt(index);
            return local;
        }
        return this.provider();
    }
}

