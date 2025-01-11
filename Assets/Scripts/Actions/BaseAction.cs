using System;

public abstract class BaseAction : IAction
{

  private Action _onComplete;
  private bool _isComplete;

  public bool IsComplete => _isComplete;

  public void StartAction(Action onComplete)
  {
    _onComplete = onComplete;
    _isComplete = false;
    OnStart();
  }

  public void UpdateAction()
  {
    if (_isComplete) return;

    OnUpdate();

    if (_isComplete)
    {
      _onComplete?.Invoke();
    }
  }

  protected void CompleteAction()
  {
    _isComplete = true;
  }

  // To be implemented by derived classes
  protected abstract void OnStart();
  protected abstract void OnUpdate();

}
