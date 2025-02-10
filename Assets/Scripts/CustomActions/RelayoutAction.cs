using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class RelayoutAction : IAction
{

  protected CardPile targetPile;
  protected bool isComplete;
  protected Action onComplete;
  public bool IsComplete => isComplete;
  private bool bypassPausing = false;
  public bool BypassPausing => bypassPausing;

  // unused by maybe needed later
  public bool randomizeOffsets = false;
  public float rotationOffsetEulerAngleMin = 0;
  public float rotationOffsetEulerAngleMax = 0;
  public Vector3 positionOffsetMax = Vector3.zero;
  public Vector3 positionOffsetMin = Vector3.zero;

  // Duration for the entire re-layout animation
  protected float duration;
  protected float elapsedTime;

  // We'll gather all cards in the pile,
  // then store their start & end states for lerping.
  protected List<GameObject> pileCards;
  protected Vector3[] startPositions;
  protected Vector3[] endPositions;

  // For rotation lerping:
  protected Quaternion[] startRotations;
  protected Quaternion[] endRotations;

  public RelayoutAction(CardPile targetPile, float duration = 0.7f)
  {
    this.targetPile = targetPile;
    this.duration = duration;
    this.duration /= GameManager.speed;
  }

  public abstract void StartAction(Action onComplete);

  public virtual void UpdateAction()
  {
    if (isComplete) return;

    elapsedTime += Time.deltaTime;
    float t = Mathf.Clamp01(elapsedTime / duration);

    // Apply easing for smooth deceleration
    float easedT = EaseOutCubic(t);

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

    // 2) Create arrays for start/end positions and rotations
    startPositions = new Vector3[pileCards.Count];
    endPositions = new Vector3[pileCards.Count];

    startRotations = new Quaternion[pileCards.Count];
    endRotations = new Quaternion[pileCards.Count];

    // 3) Record each card’s current position/rotation (start)
    for (int i = 0; i < pileCards.Count; i++)
    {
      startPositions[i] = pileCards[i].transform.position;
      startRotations[i] = pileCards[i].transform.rotation;
    }

    // 4) Compute the final layout positions & rotations for each card
    for (int i = 0; i < pileCards.Count; i++)
    {
      // final position is the local offset -> world space
      // final rotation is the same as the pile's rotation (or you could offset further)
      endPositions[i] = ComputeCardEndPosition(i, pileCards.Count, targetPile);
      endRotations[i] = targetPile.transform.rotation;
    }

    // Reset elapsed time so we start animating from 0
    elapsedTime = 0f;
  }

  /// <summary>
  /// Returns where card i out of total should end up in world space,
  /// based on the pile's transform/rotation/spread.
  /// </summary>
  protected virtual Vector3 ComputeCardEndPosition(int i, int total, CardPile pile)
  {
    // We'll compute a local-space offset, then transform it by the pile's transform
    Vector3 localOffset = Vector3.zero;

    switch (pile.spreadType)
    {
      case SpreadType.LeftToRight:
      {
        // Spread along local X
        float spacing = 1.2f;
        float totalWidth = (total - 1) * spacing;
        float startX = -totalWidth / 2f;

        localOffset = new Vector3(startX + i * spacing, 0f, 0f);
        // Convert local offset to world space
        return pile.transform.TransformPoint(localOffset);
      }

      /*
      case SpreadType.Top:
      {
        // Stack cards with a small offset
        float stackOffset = 0.02f; // how far behind each card is
        localOffset = new Vector3(0f, 0f, -i * stackOffset);
        return pile.transform.TransformPoint(localOffset);
      }
      */

      default:
        return pile.transform.position;
    }
  }

  /// <summary>
  /// Smooth deceleration using cubic easing.
  /// </summary>
  public static float EaseOutCubic(float t)
  {
    return 1 - Mathf.Pow(1 - t, 3);
  }

}
