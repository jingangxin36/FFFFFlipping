using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour {
    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "PoolItem") {
            var poolItem = other.transform.GetComponent<Cell>();
            //Debug.Log("OnPickUp " + poolItem);
            poolItem.OnPickUp();
        }
        else {
            if (other.transform.GetComponent<Cell>() != null) {
                Debug.LogError("不明确Cell: " + other.name+"未设置Tag!!");
            }
        }
    }
}
