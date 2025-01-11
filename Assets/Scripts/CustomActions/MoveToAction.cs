using System;
using UnityEngine;

public class MoveToAction : IAction
{

  private GameObject cardObject;
  private Vector3 targetPosition;
  private float duration;
  private Action onComplete;

  private Vector3 startPosition;
  private float elapsedTime;
  private bool isComplete;

  public bool IsComplete => isComplete;

  public MoveToAction(GameObject cardObject, Vector3 targetPosition, float duration)
  {
    this.cardObject = cardObject;
    this.targetPosition = targetPosition;
    this.duration = duration;
  }

  public void StartAction(Action onComplete)
  {
    this.onComplete = onComplete;
    startPosition = cardObject.transform.position;
    elapsedTime = 0;
    isComplete = false;
  }

  public void UpdateAction()
  {
    if (isComplete) return;

    elapsedTime += Time.deltaTime;
    float t = Mathf.Clamp01(elapsedTime / duration);

    cardObject.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

    if (t >= 1f)
    {
      isComplete = true;
      onComplete?.Invoke();
    }
  }

}
