using System;
using UnityEngine;

// moves a single card at once to a pile
public class MoveCardToPileAction : IAction
{

  private GameObject card;
  private CardPile targetPile;
  private bool isComplete;
  private Action onComplete;

  // You could param-ify the movement speed/duration. For simplicity:
  private float duration = 0.7f;
  private float elapsedTime = 0f;

  private Vector3 startPos;
  private Vector3 endPos;

  public bool IsComplete => isComplete;

  public MoveCardToPileAction(GameObject card, CardPile target)
  {
    this.card = card;
    this.targetPile = target;
  }

  public void StartAction(Action onComplete)
  {
    this.onComplete = onComplete;
    isComplete = false;
    elapsedTime = 0f;

    startPos = card.transform.position;
    endPos = CalculateTargetPosition();

    // We can also directly update the target pile's array now or at the end 
    // so the card is recognized as belonging to that pile. Let's do it at the end:
  }

  public void UpdateAction()
  {
    if (isComplete) return;

    elapsedTime += Time.deltaTime;
    float t = Mathf.Clamp01(elapsedTime / duration);

    card.transform.position = Vector3.Lerp(startPos, endPos, t);

    if (t >= 1f)
    {
      // Add the card to the target pile
      targetPile.AddCard(card);

      isComplete = true;
      onComplete?.Invoke();
    }
  }

  private Vector3 CalculateTargetPosition()
  {
    // This can be a simplified version or reuse your existing Spread logic
    // For a quick approach, use target pile's transform position:
    return targetPile.transform.position;
  }

}
