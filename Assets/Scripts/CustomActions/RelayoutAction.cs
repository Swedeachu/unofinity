using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class RelayoutAction : IAction
{

  protected CardPile targetPile;
  protected bool isComplete;
  protected Action onComplete;
  public bool IsComplete => isComplete;

  // Duration for the entire re-layout animation
  protected float duration;
  protected float elapsedTime;

  // We’ll gather all cards in the pile, 
  // then store their start & end positions for lerping.
  protected List<GameObject> pileCards;
  protected Vector3[] startPositions;
  protected Vector3[] endPositions;

  public RelayoutAction(CardPile targetPile, float duration = 0.7f)
  {
    this.targetPile = targetPile;
    this.duration = duration;
  }

  public abstract void StartAction(Action onComplete);

  public virtual void UpdateAction()
  {
    if (isComplete) return;

    elapsedTime += Time.deltaTime;
    float t = Mathf.Clamp01(elapsedTime / duration);

    // Lerp every card in the pile from start to end
    for (int i = 0; i < pileCards.Count; i++)
    {
      if (pileCards[i] == null) continue;
      pileCards[i].transform.position = Vector3.Lerp(startPositions[i], endPositions[i], t);
    }

    // Completed animation?
    if (t >= 1f)
    {
      OnActionComplete();
    }
  }

  protected virtual void OnActionComplete()
  {
    isComplete = true;
    onComplete?.Invoke();
  }

  /// <summary>
  /// After modifying targetPile.cards (adding/removing), 
  /// call this to gather the current positions & compute new positions 
  /// so we can animate everything at once.
  /// </summary>
  protected void SetupPileForRelayout()
  {
    // 1) Gather up all cards currently in the pile
    pileCards = new List<GameObject>(targetPile.cards);

    // 2) Create arrays for start/end positions
    startPositions = new Vector3[pileCards.Count];
    endPositions = new Vector3[pileCards.Count];

    // 3) Record each card’s current position
    for (int i = 0; i < pileCards.Count; i++)
    {
      startPositions[i] = pileCards[i].transform.position;
    }

    // 4) Compute the final layout positions for each card
    for (int i = 0; i < pileCards.Count; i++)
    {
      endPositions[i] = ComputeCardEndPosition(i, pileCards.Count, targetPile);
    }

    // Reset elapsed time so we start animating from 0
    elapsedTime = 0f;
  }

  /// <summary>
  /// Returns where card i out of total should end up for the given pile’s spread.
  /// </summary>
  protected virtual Vector3 ComputeCardEndPosition(int i, int total, CardPile pile)
  {
    Vector3 basePos = pile.transform.position;

    switch (pile.spreadType)
    {
      case SpreadType.LeftToRight:
        float spacing = 1.2f;
        float totalWidth = (total - 1) * spacing;
        float startX = -totalWidth / 2f;
        return basePos + new Vector3(startX + i * spacing, 0f, 0f);

      case SpreadType.Top:
        // Example: stack them with a tiny offset in Y so we can see each card
        return basePos /*+ new Vector3(0f, i * 0.02f, 0f)*/;

      default:
        // If more modes exist, handle them here
        return basePos;
    }
  }

}
