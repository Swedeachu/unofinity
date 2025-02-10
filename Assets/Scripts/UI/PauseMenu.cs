using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{

  private GameManager gameManager;

  // must be set in editor since its disabled by default and finding it automatically would be really jank
  public GameObject panel;
  public Canvas canvas;

  // must set in editor:
  public Vector3 offScreenPosition;
  public Vector3 onScreenPosition;

  void Start()
  {
    if (canvas == null)
    {
      Debug.LogError("Pause menu canvas not found");
    }
    else
    {
      Debug.Log("Found PauseMenu canvas");
      canvas.gameObject.SetActive(true); // set it to active so we can mess with it
    }

    // Find the GameManager
    gameManager = FindObjectOfType<GameManager>();
    if (gameManager == null)
    {
      Debug.LogError("GameManager not found");
    }
    else
    {
      Debug.Log("Found GameManager");
    }

    // starts off screen
    panel.transform.position = offScreenPosition; 

    // and disable it again after messing with it
    if (canvas != null)
    {
      canvas.gameObject.SetActive(false);
    }
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Escape))
    {
      TogglePause();
    }
  }

  public void TogglePause()
  {
    // toggle pause in game manager for action runner
    if (gameManager != null)
    {
      gameManager.actionRunner.paused = !gameManager.actionRunner.paused;
    }

    List<IAction> actions = new List<IAction>();
    if (gameManager.actionRunner.paused) // if we are paused, now move onto the screen
    {
      actions.Add(new RotateAndMoveAction(panel, onScreenPosition, 0, 1));
    }
    else // if not paused, move stuff off screen
    {
      // this won't work because canvas will be disabled, we would need to defer it being disabled but also make it not interactable anymore
      // actions.Add(new RotateAndMoveAction(resumeButton.gameObject, resumeButtonOffScreenPosition, 0, 1));
      // instead I just auto set the position | WEIRD: positions are seemingly being set randomly
      panel.transform.localPosition = offScreenPosition;
    }

    // Toggle canvas on
    if (canvas != null)
    {
      canvas.gameObject.SetActive(!canvas.gameObject.activeSelf);
    }

    if (actions.Count == 0) return;

    // force the rotate and move actions to bypass pausing
    foreach (IAction action in actions)
    {
      var rma = action as RotateAndMoveAction;
      if (rma != null)
      {
        rma.bypassPausing = true;
      }
    }

    // run it immediately right in the current action set
    gameManager.actionRunner.RunActions(actions);
  }

}
