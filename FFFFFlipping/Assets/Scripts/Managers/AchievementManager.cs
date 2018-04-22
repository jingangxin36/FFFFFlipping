using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//todo  菜单

public class AchievementManager : Singleton<AchievementManager> {

    public Text killEnemyCount;
    public Text coinCount;
    public Text xp;
    public Slider xpSlider;
    /// <summary>
    /// 血条消耗的速度
    /// </summary>
    public int xpDecreaseSpeed;

    private int mKillEnemyCount;
    private int mXp = 100;
    private int mCoinCount;

    public int lowBloodLimit;

    void Start () {
        RefreshUI();
    }

    public void StartDecreaseXp() {
        InvokeRepeating("XpDecrease", 0f,  1f);
    }

    public void PickUp(CellType type) {
        switch (type) {
            case CellType.COIN:
                CoinOnPickUp();
                break;
            case CellType.ENEMY:
                EnemyOnKill();
                break;
            case CellType.TRAP:
                PlayerOnKilled();
                break;
        }
    }

    private void RefreshUI() {
        coinCount.text = mCoinCount + " ";
        killEnemyCount.text = mKillEnemyCount + " ";
        RefreshXpInfo();

    }

    private void EnemyOnKill() {
        mKillEnemyCount++;
        killEnemyCount.text = mKillEnemyCount + " ";
        ChangeXp(1);
    }

    private void CoinOnPickUp() {
        mCoinCount++;
        coinCount.text = mCoinCount + " ";
    }

    private void XpDecrease() {
        ChangeXp(-1);
    }


    private void ChangeXp(int direction) {
        if (transform.gameObject.activeSelf) {
            if (direction == -1) {
                //是否提示低血
                if (mXp <= lowBloodLimit + xpDecreaseSpeed && mXp >= lowBloodLimit) {
                    GameController.Instance.LowBlood();
                }
            }

            if (direction == 1) {
                //是否恢复正常血
                if (mXp >= lowBloodLimit + xpDecreaseSpeed && mXp <= lowBloodLimit + 2 * xpDecreaseSpeed) {
                    GameController.Instance.ResumeNormalBlood();
                }
            }
            mXp += xpDecreaseSpeed * direction;
            RefreshXpInfo();
        }

    }


    private void RefreshXpInfo() {
        if (mXp <= 0) {
            xpSlider.value = 0;
            PlayerOnKilled();
            return;
        }
        else if (mXp >= 100) {
            mXp = 100;
        }
        xp.text = mXp + " ";
        xpSlider.value = (float)mXp / 100;
    }

    private void PlayerOnKilled() {
        GameController.Instance.GameOver();
    }

}
