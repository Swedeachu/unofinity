using System;
using UnityEngine;

public class RotateAndMoveAction : IAction
{

  private GameObject cardObject;
  private Vector3 targetPosition;
  private float targetRotation;
  private float duration;
  private Action onComplete;

  private Vector3 startPosition;
  private float startRotation;
  private float elapsedTime;
  private bool isComplete;

  public bool IsComplete => isComplete;

  public RotateAndMoveAction(GameObject cardObject, Vector3 targetPosition, float targetRotation, float duration)
  {
    this.cardObject = cardObject;
    this.targetPosition = targetPosition;
    this.targetRotation = targetRotation;
    this.duration = duration;

    startPosition = cardObject.transform.position;
    startRotation = cardObject.transform.eulerAngles.y; // Only rotating around Y-axis
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
    float easedT = EaseOutCubic(t);

    // Interpolate position with eased time
    cardObject.transform.position = Vector3.Lerp(startPosition, targetPosition, easedT);

    // Interpolate rotation around Y-axis with eased time
    float currentRotation = Mathf.LerpAngle(startRotation, targetRotation, easedT);
    cardObject.transform.eulerAngles = new Vector3(0, currentRotation, 0);

    if (t >= 1f)
    {
      isComplete = true;
      onComplete?.Invoke();
    }
  }

  private float EaseOutCubic(float t)
  {
    return 1 - Mathf.Pow(1 - t, 3);
  }

}
