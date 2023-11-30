using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // Add this line for scene management.

public class PlayerManager : MonoBehaviour
{
    public bool gameOver = false;
    public GameObject gameOverPanel;
    public static bool isGameStarted;
    public GameObject startingText;
    public TextMeshProUGUI coinCount;
    public TextMeshProUGUI TimeLeft;
    // public Text coinsText;
    public static int numberofCoins;
    // public Text timerText;
    [SerializeField] public float duration, currentTime;

    void Start()
    {
        gameOver = false;
        Time.timeScale = 1;
        isGameStarted = false;
        numberofCoins = 0;
        currentTime=duration;
        // TimeLeft.text=currentTime.ToString();
    }
    IEnumerator TimeIEn(){
        while(currentTime>=0)
        {
            TimeLeft.text = currentTime.ToString();
            yield return new WaitForSeconds(1f);
            currentTime--;
        }
        gameOver = true;
    }
    void Update()
    {
        if (gameOver)
        {
            Time.timeScale = 0;
            gameOverPanel.SetActive(true);

        }
        coinCount.text = numberofCoins.ToString();

        // Check if AR scene is loaded and if the user taps the screen
        if (!isGameStarted && SwipeManager.tap)
        {
            isGameStarted = true;
            Destroy(startingText);
            coinCount.gameObject.SetActive(true);
            StartCoroutine(TimeIEn());
        }
    }

 
}
