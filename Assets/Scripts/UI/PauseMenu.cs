using System.Collections.Generic;
using Unity.Mathematics;
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

  // automode chaos monkey buissness
  private AddHand addHandButton;
  private CardCountSlider cardCountSlider;
  private SpeedSliderControl speedSliderControl;

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

      // Find the stuff we need too for chaose monkey mode
      speedSliderControl = FindAnyObjectByType<SpeedSliderControl>();
      cardCountSlider = FindAnyObjectByType<CardCountSlider>();
      addHandButton = FindAnyObjectByType<AddHand>();

      if (speedSliderControl == null)
      {
        Debug.LogWarning("speed slider is null!");
      }

      if (cardCountSlider == null)
      {
        Debug.LogWarning("Card count slider is null!");
      }

      if (addHandButton == null)
      {
        Debug.LogWarning("Add hand button is null!");
      }

      canvas.gameObject.SetActive(false);
    }
  }

  void Update()
  {
    if (!inFlight && !GameManager.autoMode && Input.GetKeyDown(KeyCode.Escape))
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

  // Picks one of the following and either simulates its click action, or changes its value to something random within bounds if it is a slider, and logs it in telemetry manager.
  // SpeedSliderControl: we don't actually want to do this because this would screw with auto mode speed, maybe we should do the slider to change it visually than back right after 
  // CardCountSlider
  // AddHand button
  //
  // Afterwards TogglePause is called again (except for AddHand that already happens after clicking it)
  // We don't do exit button or resume button, because we obviously don't want to exit the app,
  // and resume is always called afterwards so no point calling it randomly or manually afterwards.
  public void ChaosMonkey()
  {
    // TODO: telemetry logging in a TelemetryManager object

    TogglePause();

    int randomValue = UnityEngine.Random.Range(1, 4);

    bool runUnpauseAction = true;

    var cleanUpActions = new List<IAction>();

    // wait one second afterwards of doing the thing (5 seconds to make it 1 second long due to auto mode being 5x fast)
    var delay = new DelayAction(5);
    delay.bypassPausing = true;
    // do it first thing in its own batch
    gameManager.actionBatchManager.AddBatch(new List<IAction>() { delay });

    if (randomValue == 1)
    {
      Debug.Log("Chaos Monkey: Add Hand Button");

      // hit the add hand button 
      var action = new CallbackAction(() =>
      {
        addHandButton.OnButtonClicked();
        runUnpauseAction = false; // not needed because the return below but whatever
      });

      action.bypassPausing = true;

      gameManager.actionBatchManager.AddBatch(new List<IAction>() { action });

      return; // because add hand button calls TogglePause() on its own
    }
    else if (randomValue == 2)
    {
      Debug.Log("Chaos Monkey: Speed Slider");

      // tweak the game speed slider (since we are in auto mode this won't actually have any real impact beyond visual in the UI, as auto mode is always 5x)
      float before = speedSliderControl.slider.value;

      var slideAction = new CallbackAction(() =>
      {
        int random = UnityEngine.Random.Range(1, 6);
        speedSliderControl.slider.value = random;
      });

      slideAction.bypassPausing = true;

      gameManager.actionBatchManager.AddBatch(new List<IAction>() { slideAction });

      var cleanUp = new CallbackAction(() =>
      {
        speedSliderControl.slider.value = before;
      });

      cleanUp.bypassPausing = true;
    }
    else if (randomValue == 3)
    {
      Debug.Log("Chaos Monkey: Card count");

      // tweak the card count slider

      var action = new CallbackAction(() =>
      {
        int random = UnityEngine.Random.Range(1, 7);
        cardCountSlider.slider.value = random;
      });

      action.bypassPausing = true;

      gameManager.actionBatchManager.AddBatch(new List<IAction>() { action });
    }

    // except for add hand button since it already does that, we need to unpause afterwards
    if (runUnpauseAction)
    {
      var action = new CallbackAction(() =>
      {
        TogglePause();
      });
      action.bypassPausing = true;

      gameManager.actionBatchManager.AddBatch(new List<IAction>() { action });
    }

    gameManager.actionBatchManager.AddBatch(cleanUpActions);
  }

}
