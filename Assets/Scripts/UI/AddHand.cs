using System.Collections.Generic;
using UnityEngine;

public class AddHand : MonoBehaviour
{

  private GameManager manager;

  private void Start()
  {
    manager = FindObjectOfType<GameManager>();
    if(manager == null )
    {
      Debug.LogWarning("Could not find game manager in AddHand button!");
    }
  }

  public void OnButtonClicked()
  {
    var gameManager = FindObjectOfType<GameManager>();
    if (gameManager != null)
    {
      
      // return if we are not paused (we only want to control stuff if we are paused)
      if (!gameManager.actionRunner.paused) return;

      // Move up a bit
      Vector3 target = Camera.main.transform.localPosition + new Vector3(0, 0.6f, 0);
      manager.actionBatchManager.AddBatch(new List<IAction>() { new RotateAndMoveAction(Camera.main.gameObject, target, 0, 0.5f, false) });

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
