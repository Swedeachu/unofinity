using System;
using UnityEngine;

public class RotateAndMoveAction : IAction
{

  private GameObject gameObject;
  private Vector3 targetPosition;
  private float targetRotation;
  private float duration;
  private Action onComplete;

  private Vector3 startPosition;
  private float startRotation;
  private float elapsedTime;
  private bool isComplete;
  public bool bypassPausing = false;
  public bool IsComplete => isComplete;
  public bool BypassPausing => bypassPausing;

  public RotateAndMoveAction(GameObject gameObject, Vector3 targetPosition, float targetRotation, float duration)
  {
    this.gameObject = gameObject;
    this.targetPosition = targetPosition;
    this.targetRotation = targetRotation;
    this.duration = duration;
    this.duration /= GameManager.speed;

    startPosition = gameObject.transform.localPosition;
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

    // Interpolate position with eased time
    gameObject.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, easedT);

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
