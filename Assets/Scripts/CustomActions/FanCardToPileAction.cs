using System;
using System.Collections.Generic;
using UnityEngine;

public class FanCardToPileAction : IAction
{

  protected CardPile targetPile;
  protected List<GameObject> pileCards;
  private GameObject cardToMove;

  private Vector3[] startPositions;
  private Vector3[] endPositions;

  private Quaternion[] startRotations;
  private Quaternion[] endRotations;

  private float duration;
  private float elapsedTime;
  protected bool isComplete;
  protected Action onComplete;

  public bool IsComplete => isComplete;

  public FanCardToPileAction(GameObject card, CardPile target, float duration = 0.7f)
  {
    this.cardToMove = card;
    this.targetPile = target;
    this.duration = duration;
  }

  public virtual void StartAction(Action onComplete)
  {
    this.onComplete = onComplete;
    isComplete = false;

    // Add the card to the pile (if not already there)
    targetPile.AddCard(cardToMove);

    // Gather all cards in the pile
    pileCards = new List<GameObject>(targetPile.cards);

    // Setup the start and end states for the animation
    SetupFanLayout();
  }

  public void UpdateAction()
  {
    if (isComplete) return;

    elapsedTime += Time.deltaTime;
    float t = Mathf.Clamp01(elapsedTime / duration);

    // Apply easing for smooth deceleration
    float easedT = RelayoutAction.EaseOutCubic(t);

    // Lerp every card in the pile from start to end position AND rotation
    for (int i = 0; i < pileCards.Count; i++)
    {
      if (pileCards[i] == null) continue;

      // Position with eased interpolation
      pileCards[i].transform.position =
          Vector3.Lerp(startPositions[i], endPositions[i], easedT);

      // Rotation with eased interpolation
      pileCards[i].transform.rotation =
          Quaternion.Slerp(startRotations[i], endRotations[i], easedT);
    }

    // Completed animation?
    if (t >= 1f)
    {
      OnActionComplete();
    }
  }

  protected void SetupFanLayout()
  {
    int totalCards = pileCards.Count;

    startPositions = new Vector3[totalCards];
    endPositions = new Vector3[totalCards];

    startRotations = new Quaternion[totalCards];
    endRotations = new Quaternion[totalCards];

    // Record current states (start)
    for (int i = 0; i < totalCards; i++)
    {
      startPositions[i] = pileCards[i].transform.position;
      startRotations[i] = pileCards[i].transform.rotation;
    }

    // Calculate final positions and rotations (end)
    for (int i = 0; i < totalCards; i++)
    {
      endPositions[i] = ComputeFanEndPosition(i, totalCards, targetPile);
      endRotations[i] = ComputeFanEndRotation(i, totalCards, targetPile);
    }

    elapsedTime = 0f;
  }

  private Vector3 ComputeFanEndPosition(int index, int total, CardPile pile)
  {
    // Fan out in a local arc relative to the pile's rotation
    float arcWidth = 0.3f; // Adjust spacing between cards
    float centerOffset = -(total - 1) * arcWidth / 2f;

    // Local offset along the pile's local X-axis
    Vector3 localOffset = new Vector3(centerOffset + index * arcWidth, 0f, 0f);

    // Convert local offset to world space
    return pile.transform.TransformPoint(localOffset);
  }

  private Quaternion ComputeFanEndRotation(int index, int total, CardPile pile)
  {
    // Fan rotation: Adjust rotation angle per card
    float maxAngle = 30f; // Total fan angle in degrees
    float startAngle = -maxAngle / 2f; // Starting angle
    float angleStep = maxAngle / Mathf.Max(1, total - 1); // Angle per card

    // Calculate card rotation in local space
    float cardAngle = startAngle + index * angleStep;

    // Combine pile's rotation with the local fan angle
    Quaternion pileRotation = pile.transform.rotation;
    Quaternion localRotation = Quaternion.Euler(0f, cardAngle, 0f);

    return pileRotation * localRotation;
  }

  protected void OnActionComplete()
  {
    isComplete = true;
    onComplete?.Invoke();
  }

}
