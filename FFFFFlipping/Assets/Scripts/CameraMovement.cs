using UnityEngine;

public class CameraMovement : MonoBehaviour {

    public GameObject followTarget;
    public float moveSpeed;

    void Update() {
        if (followTarget != null) {
            //相机位置Z值与目标点的Z值做插值, 实现相机前后跟随, 而目标点运动不影响
            var newZ = Mathf.Lerp(transform.position.z, followTarget.transform.position.z, Time.deltaTime * moveSpeed);
            var newVector3 = new Vector3(transform.position.x, transform.position.y, newZ);
            transform.position = newVector3;
        }
    }
}

