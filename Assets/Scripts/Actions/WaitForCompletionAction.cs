using System;
using System.Collections.Generic;

public class WaitForCompletionAction : IAction
{

  private List<IAction> actionsToWaitFor;
  private Action onComplete;

  private bool isComplete;
  private bool bypassPausing = true; // to avoid self caused dead locks in a now empty set of actions during a paused state, we have this as true
                                     // this is to fix a rare problem during switching from paused to non paused states
  public bool IsComplete => isComplete;
  public bool BypassPausing => bypassPausing;

  public WaitForCompletionAction(List<IAction> actions, Action onComplete)
  {
    actionsToWaitFor = actions;
    this.onComplete = onComplete;
  }

  public void StartAction(Action onComplete)
  {
    this.onComplete += onComplete; 
    isComplete = false;
  }

  public void UpdateAction()
  {
    if (isComplete) return;

    isComplete = actionsToWaitFor.TrueForAll(action => action.IsComplete);
    if (isComplete)
    {
      onComplete?.Invoke();
    }
  }

}

