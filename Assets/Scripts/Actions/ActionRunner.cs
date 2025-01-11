using System.Collections.Generic;
using UnityEngine;

public class ActionRunner : MonoBehaviour
{

  private List<IAction> activeActions = new List<IAction>();

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
      activeActions[i].UpdateAction();
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
