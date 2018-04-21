using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController :Singleton<GameController> {
    public GameObject gameView;
    public GameObject gameOverView;
    public Text distance;
    public Text killCount;
    public Text coinCount;
    public Text score;
    public Transform player;


    public bool isDead;
    /// <summary>
    /// 死亡时相关的UI响应
    /// </summary>
    public void GameOver() {
        Debug.Log(player.position.z);

        PlayerController.Instance.SetPlayerKilled();
        //todo 显示成就, 功能按钮,
        var achievementInstance = AchievementManager.Instance;
        distance.text = (Convert.ToInt32(player.position.z)) + "";
        killCount.text = achievementInstance.killEnemyCount.text + "";
        coinCount.text = achievementInstance.coinCount.text + "";
        score.text = (Convert.ToInt32(killCount.text) + Convert.ToInt32(distance.text)).ToString();

        //Time.timeScale = 0;
        gameView.SetActive(false);
        gameOverView.SetActive(true);
    }

    public void ReLoadGame() {
        //Time.timeScale = 1;
        SceneManager.LoadScene("Demo");
    }
}
