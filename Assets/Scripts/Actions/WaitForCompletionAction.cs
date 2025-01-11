using System;
using System.Collections.Generic;

public class WaitForCompletionAction : IAction
{

  private List<IAction> actionsToWaitFor;
  private Action onComplete;
  private bool isComplete;

  public bool IsComplete => isComplete;

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

