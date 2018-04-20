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
        mXp += 10;
        killEnemyCount.text = mKillEnemyCount + " ";
        RefreshXpInfo();
    }

    private void CoinOnPickUp() {
        mCoinCount++;
        coinCount.text = mCoinCount + " ";
    }

    private void XpDecrease() {
        mXp -= xpDecreaseSpeed;
        RefreshXpInfo();
    }

    private void RefreshXpInfo() {
        if (mXp <= 0) {
            xpSlider.value = 0;
            PlayerOnKilled();
            return;
        }
        if (mXp >= 100) {
            mXp = 100;
        }
        xp.text = mXp + " ";
        xpSlider.value = (float)mXp / 100;
    }

    private void PlayerOnKilled() {
        GameController.Instance.GameOver();
    }

}
