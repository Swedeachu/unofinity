using UnityEngine;

public class ResumeButton : MonoBehaviour
{

  public void OnButtonClicked()
  {
    var gameManager = FindObjectOfType<GameManager>();
    if (gameManager != null)
    {
      // return if we are not paused (we only want to control stuff if we are paused)
      if (!gameManager.actionRunner.paused) return;
    }

    // Get the parent Canvas with the PauseMenu component
    PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();
    if (pauseMenu != null)
    {
      if (!GameManager.autoMode) // we don't want to pause during auto mode chaos monkey tests
      {
        pauseMenu.TogglePause();
      }
    }
    else
    {
      Debug.LogError("PauseMenu component not found on parent Canvas.");
    }
  }

}
