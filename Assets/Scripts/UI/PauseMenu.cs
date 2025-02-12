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

  private bool inFlight = false;

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
    panel.transform.localPosition = offScreenPosition;

    // and disable it again after messing with it
    if (canvas != null)
    {
      canvas.gameObject.SetActive(false);
    }
  }

  void Update()
  {
    if (!inFlight && Input.GetKeyDown(KeyCode.Escape))
    {
      TogglePause();
    }
  }

  public void TogglePause()
  {
    // inFlight = true;

    // toggle pause in game manager for action runner
    if (gameManager != null)
    {
      gameManager.actionRunner.paused = !gameManager.actionRunner.paused;
    }

    List<IAction> actions = new List<IAction>();
    if (gameManager.actionRunner.paused) // if we are paused, now move onto the screen
    {
      canvas.gameObject.SetActive(true);
      actions.Add(new RotateAndMoveAction(panel, onScreenPosition, 0, 1));
    }
    else // if not paused, move stuff off screen
    {
      actions.Add(new RotateAndMoveAction(panel, offScreenPosition, 0, 1));
      // Toggle canvas on in a call back seperately to run later (Doesn't work until after the current batch but it moving offscreen too is fine)
      if (canvas != null)
      {
        var cb = new CallbackAction(() =>
        {
          canvas.gameObject.SetActive(false);
        });
        cb.bypassPausing = true;
        gameManager.actionBatchManager.AddBatch(new List<IAction> { cb });
      }
    }

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

    /* just doesn't work, don't care enough to fix something this small
    // no longer in flight after all the above batches run
    var f = new CallbackAction(() =>
    {
      inFlight = false;
    });
    f.bypassPausing = true;
    gameManager.actionBatchManager.AddBatch(new List<IAction> { f });
    */
  }

}
