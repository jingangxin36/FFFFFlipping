using System;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    /// <summary>
    /// 主角的跳跃速度
    /// </summary>
    public float speed;
    /// <summary>
    /// 主角每次跳跃的偏移量
    /// </summary>
    public float characterPositionOffset;
    /// <summary>
    /// 主角位置的X轴坐标
    /// </summary>
    private float[] mCharacterPositionXs;
    /// <summary>
    /// 主角当前位于哪个位置的索引
    /// </summary>
    private int mCurrentX;

    void Start() {
        //初始化主角位置
        mCharacterPositionXs = new[] { characterPositionOffset * -1, 0, characterPositionOffset };
        mCurrentX = 1;
    }

    void Update() {
        //获得左右方向的偏移量, -1表示按下左键, 1表示按下右键
        var keyHorizontal = Input.GetAxis("Horizontal");
        if (keyHorizontal > 0) {
            if (!IsInvoking("RightJump")) {
                InvokeRepeating("RightJump", 0f, speed / 0.1f);
            }

        }
        else if (keyHorizontal < 0) {
            if (!IsInvoking("LeftJump")) {
                InvokeRepeating("LeftJump", 0f, speed / 0.1f);
            }
        }
        //如果停止按方向键
        if (Math.Abs(keyHorizontal) < 0.1) {
            CancelInvoke("LeftJump");
            CancelInvoke("RightJump");
        }
    }

    /// <summary>
    /// 主角向左跳一次
    /// </summary>
    public void LeftJump() {
        ForwardJump(2);
    }
    /// <summary>
    /// 主角向右跳一次
    /// </summary>
    public void RightJump() {
        ForwardJump(1);
    }
    /// <summary>
    /// 左右跳时, 主角也向前跳
    /// </summary>
    /// <param name="direction">2表示向左跳, 1表示向右跳</param>
    private void ForwardJump(int direction) {
        var newVector3 = transform.position;
        newVector3.z = characterPositionOffset + transform.position.z;
        //得到当前操作后的主角位置坐标索引
        mCurrentX = (mCurrentX + direction) % 3;
        newVector3.x = mCharacterPositionXs[mCurrentX];
        transform.position = newVector3;
    }
}
