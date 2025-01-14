using System;

public class CallbackAction : IAction
{

  private Action callback;
  private bool isComplete;
  private Action onComplete;

  private bool bypassPausing = false;
  public bool IsComplete => isComplete;
  public bool BypassPausing => bypassPausing;

  public CallbackAction(Action callback)
  {
    this.callback = callback;
  }

  public void StartAction(Action onComplete)
  {
    isComplete = false;
    this.onComplete = onComplete;
  }

  public void UpdateAction()
  {
    if (isComplete) return;

    // Immediately call the callback, then complete
    callback?.Invoke();

    isComplete = true;
    onComplete?.Invoke();
  }

}
