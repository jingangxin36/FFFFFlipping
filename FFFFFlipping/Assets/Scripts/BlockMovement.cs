using UnityEngine;

public class BlockMovement : MonoBehaviour {

    void OnTriggerExit(Collider other) {
        //当相机看不见某一行block时, 将该block移到最后一排, 实现无限地图
        if (other.gameObject.tag == "Floor") {
            BlockManager.instance.ChangeBlockPosition(other.transform);
        }
    }
}
