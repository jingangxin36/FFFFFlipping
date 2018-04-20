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
    }
}
