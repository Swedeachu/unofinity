using System;

public interface IAction
{
  void StartAction(Action onComplete);
  void UpdateAction();
  bool IsComplete { get; }
  bool BypassPausing { get; }
}
