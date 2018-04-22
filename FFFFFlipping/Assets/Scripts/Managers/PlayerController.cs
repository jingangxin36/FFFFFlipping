using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class PlayerController : Singleton<PlayerController> {

    /// <summary>
    /// 主角的跳跃速度
    /// </summary>
    public float speed;
    /// <summary>
    /// 主角每次跳跃的偏移量
    /// </summary>
    public float characterPositionOffset;
    /// <summary>
    /// 主角跳跃高度
    /// </summary>
    public float jumpHeight;
    /// <summary>
    /// 每次向后跳跃的间隔时间
    /// </summary>
    public float backableTime;

    public Button backButton;

    private bool mBackable;

    private bool mIsFirstTap;

    private bool mIsKilled;

    private bool mIsPressing;
    /// <summary>
    /// 主角当前位置X轴坐标
    /// </summary>
    private int mCurrentX;

    void Start() {

        mCurrentX = 1;
        mBackable = false;
        backButton.interactable = false;
        mIsFirstTap = true;
        Vector3 newVector3 = transform.position;
        newVector3.z = 0;
        transform.position = newVector3;
    }

    void Update() {

#if UNITY_EDITOR 
        if (!mIsKilled) {
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
            //如果停止按左右方向键
            if (Math.Abs(keyHorizontal) < 0.1) {
                CancelInvoke("LeftJump");
                CancelInvoke("RightJump");
            }

            //获得前后方向的偏移量, -1表示按下下键
            var keyVertical = Input.GetAxis("Vertical");
            if (mBackable && keyVertical < 0) {
                Invoke("JumpBack", 0);
            }
        }

#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        if (Input.touchCount != 1) {
            CancelInvoke("LeftJump");
            CancelInvoke("RightJump");
        }
#endif

    }


    public void SetPlayerKilled() {
        mIsKilled = true;
    }

    //public void LeftButtonOnClick() {
    //}

    //public void RightButtonOnClick() {
    //}

    /// <summary>
    /// 主角向左跳一次
    /// </summary>
    public void LeftJump() {
        ForwardJump(-1);
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
    /// <param name="direction">-1表示向左跳, 1表示向右跳</param>
    private void ForwardJump(int direction) {
        mCurrentX += direction;
        if (direction == -1) {
            //越界了
            if (mCurrentX < 0) {
                mCurrentX = 2;
                var newVector3 = transform.position;
                newVector3.x = 2 * characterPositionOffset;
                transform.position = newVector3;
            }
            transform.Translate(Vector3.left * characterPositionOffset);
        }
        if (direction == 1) {
            //越界了
            if (mCurrentX > 2) {
                mCurrentX = 0;
                var newVector3 = transform.position;
                newVector3.x = -2 * characterPositionOffset;
                transform.position = newVector3;
            }
            transform.Translate(Vector3.right * characterPositionOffset);
        }
        PlayerJump();
    }
    /// <summary>
    /// 向前跳
    /// </summary>
    public void PlayerJump() {
        if (mIsFirstTap) {
            AchievementManager.Instance.StartDecreaseXp();
            mIsFirstTap = false;
            mBackable = true;
            backButton.interactable = true;
        }
        transform.Translate(Vector3.forward * characterPositionOffset);
        transform.GetComponent<Rigidbody>().AddForce(new Vector3(0f, jumpHeight * 10, 0f));
    }
    /// <summary>
    /// 向后跳
    /// </summary>
    public void JumpBack() {
        mBackable = false;
        backButton.interactable = mBackable;
        transform.GetComponent<Rigidbody>().AddForce(new Vector3(0f, jumpHeight * 10, 0f));
        transform.Translate(Vector3.back * characterPositionOffset);
        StartCoroutine(WaitAndBackable());
    }
    /// <summary>
    /// 每次后跳间隔计时
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitAndBackable() {
        yield return new WaitForSeconds(backableTime);
        mBackable = true;
        backButton.interactable = mBackable;
    }
}
