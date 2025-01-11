using System;
using UnityEngine;

public class DelayAction : IAction
{

  private float delay;
  private Action onComplete;

  private float elapsedTime;
  private bool isComplete;

  public bool IsComplete => isComplete;

  public DelayAction(float delay)
  {
    this.delay = delay;
  }

  public void StartAction(Action onComplete)
  {
    this.onComplete = onComplete;
    elapsedTime = 0;
    isComplete = false;
  }

  public void UpdateAction()
  {
    if (isComplete) return;

    elapsedTime += Time.deltaTime;

    if (elapsedTime >= delay)
    {
      isComplete = true;
      onComplete?.Invoke();
    }
  }

}
