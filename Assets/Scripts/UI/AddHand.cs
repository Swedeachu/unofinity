using UnityEngine;

public class AddHand : MonoBehaviour
{

  public void OnButtonClicked()
  {
    var gameManager = FindObjectOfType<GameManager>();
    if (gameManager != null)
    {
      
      // return if we are not paused (we only want to control stuff if we are paused)
      if (!gameManager.actionRunner.paused) return;

      // Unpause right after (do we actually want to do this?)
      PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();
      if (pauseMenu != null)
      {
        pauseMenu.TogglePause();
      }

      gameManager.AddPlayer();
    }
  }

}
