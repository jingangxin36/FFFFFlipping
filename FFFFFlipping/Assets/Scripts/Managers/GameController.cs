using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController :Singleton<GameController> {

    public GameObject menuView;
    /// <summary>
    /// 死亡时相关的UI响应
    /// </summary>
    public void GameOver() {
        //todo 显示成就, 功能按钮,
        Time.timeScale = 0;
        menuView.SetActive(true);
    }

    public void ReLoadGame() {
        Time.timeScale = 1;
        SceneManager.LoadScene("Demo");
    }
}
