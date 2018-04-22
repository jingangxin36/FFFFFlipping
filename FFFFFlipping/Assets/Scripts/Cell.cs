using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {
    private float mProbability;
    private CellType mType;
    public CellType Type { get; set; }

    public GameObject Player { get; private set; }

    public float Probability { get; set; }

    public Cell(CellType type, float probability = 1f) {
        Type = type;
        Probability = probability;
    }


    public override string ToString() =>
        Type.ToString();

    protected virtual void Start() {
        if (Player == null) {
            Player = GameObject.FindGameObjectWithTag("Player");
        }
    }


    public virtual void ReleaseIntoPool() {

    }

    public virtual void OnPickUp() {

    }
}
