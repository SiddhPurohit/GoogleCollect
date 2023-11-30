using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Events : MonoBehaviour
{
   public PlayerController pc;
   public GameObject SettingsUI;
   public void ReplayGame()
   {
      Debug.Log("Replay is called");
      SceneManager.LoadScene("Level");
      pc.InitUDP();
    
   }
   public void QuitGame()
   {
      Debug.Log("Quit is called");
    Application .Quit();
    
   }

   public void SettingsMenu()
   {
      SettingsUI.SetActive(true);
      print("enter settings menu");
   }
}
