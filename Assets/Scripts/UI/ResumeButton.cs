using UnityEngine;

public class ResumeButton : MonoBehaviour
{

  public void OnButtonClicked()
  {
    // Get the parent Canvas with the PauseMenu component
    PauseMenu pauseMenu = FindObjectOfType<PauseMenu>();
    if (pauseMenu != null)
    {
      pauseMenu.TogglePause();
    }
    else
    {
      Debug.LogError("PauseMenu component not found on parent Canvas.");
    }
  }

}
