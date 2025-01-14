using System.Collections.Generic;
using UnityEngine;

public class ActionRunner : MonoBehaviour
{

  private List<IAction> activeActions = new List<IAction>();
  public bool paused { get; set; } = false;

  public void RunActions(List<IAction> actions)
  {
    foreach (var action in actions)
    {
      action.StartAction(() => RemoveAction(action));
      activeActions.Add(action);
    }
  }

  private void Update()
  {
    for (int i = activeActions.Count - 1; i >= 0; i--)
    {
      if (i < 0 || i > activeActions.Count - 1)
      {
        Debug.Log("Breaking out of action runner!");
        return; // super safety for weird stuff in the action runner got interuppted mid update logic
      }

      // if we are not paused or the actions works regardless of pausing, then call update on it each frame
      if (!paused || activeActions[i].BypassPausing)
      {
        activeActions[i].UpdateAction();
      }
    }
  }

  private void RemoveAction(IAction action)
  {
    activeActions.Remove(action);
  }

  public List<IAction> GetActiveActions()
  {
    // return new List<IAction>(activeActions); // Return a copy to prevent modification
    return activeActions;
  }

}
