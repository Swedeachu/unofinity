using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveMultipleCardsToPileAction : IAction
{

  private List<GameObject> cards;
  private CardPile targetPile;
  private float duration;
  private Action onComplete;

  private float elapsedTime;
  private bool isComplete;
  private List<Vector3> targetPositions;

  private float spacing = 1.2f; // Distance between cards when doing left to right

  public bool IsComplete => isComplete;

  public MoveMultipleCardsToPileAction(List<GameObject> cards, CardPile targetPile, float duration = 1f)
  {
    this.cards = cards;
    this.targetPile = targetPile;
    this.duration = duration;

    // Calculate target positions based on the pile's spread type
    targetPositions = CalculateTargetPositions();
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

    // Move each card to its target position
    for (int i = 0; i < cards.Count; i++)
    {
      if (cards[i] != null)
      {
        cards[i].transform.position = Vector3.Lerp(
            cards[i].transform.position,
            targetPositions[i],
            t
        );
      }
    }

    if (t >= 1f)
    {
      isComplete = true;
      onComplete?.Invoke();
    }
  }

  private List<Vector3> CalculateTargetPositions()
  {
    List<Vector3> positions = new List<Vector3>();
    Vector3 basePosition = targetPile.transform.position;

    switch (targetPile.spreadType)
    {
      case SpreadType.Top:
      // All cards stack at the base position (TODO: Y increase by a tiny bit maybe so they stack on top of each other?)
      foreach (var _ in cards)
      {
        positions.Add(basePosition);
      }
      break;

      case SpreadType.LeftToRight:
      // Center the cards with respect to the pile
      int totalCards = cards.Count;
      float totalWidth = (totalCards - 1) * spacing; // Total width occupied by the cards
      float startX = -totalWidth / 2; // Start position for centering cards

      for (int i = 0; i < totalCards; i++)
      {
        positions.Add(basePosition + new Vector3(startX + i * spacing, 0, 0));
      }
      break;
    }

    return positions;
  }
}
