using System;
using UnityEngine;

public class RotateAction : IAction
{

  private GameObject gameObject;
  private float targetRotation;
  private float duration;
  private Action onComplete;

  private float startRotation;
  private float elapsedTime;
  private bool isComplete;
  public bool bypassPausing = false;
  public bool IsComplete => isComplete;
  public bool BypassPausing => bypassPausing;

  public RotateAction(GameObject gameObject,float targetRotation, float duration)
  {
    this.gameObject = gameObject;
    this.targetRotation = targetRotation;
    this.duration = duration;
    this.duration /= GameManager.speed;

    startRotation = gameObject.transform.eulerAngles.y; // Only rotating around Y-axis
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
    float t = Mathf.Clamp01(elapsedTime / duration);

    // Apply an ease-out curve to t (cubic easing for smooth deceleration)
    float easedT = RelayoutAction.EaseOutCubic(t);

    // Interpolate rotation around Y-axis with eased time
    float currentRotation = Mathf.LerpAngle(startRotation, targetRotation, easedT);
    gameObject.transform.eulerAngles = new Vector3(0, currentRotation, 0);

    if (t >= 1f)
    {
      isComplete = true;
      onComplete?.Invoke();
    }
  }

}
