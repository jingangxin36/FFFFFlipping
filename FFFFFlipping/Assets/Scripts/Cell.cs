using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {
    private float mProbability;
    private CellType mType;
    public Cell(CellType type, float probability = 1f) {
        Type = type;
        Probability = probability;
    }


    public override string ToString() =>
        Type.ToString();

    public float Probability { get; set; }

    public CellType Type { get; set; }


    public virtual void ReleaseIntoPool() {

    }

    public virtual void OnPickUp() {
        
    }
}
