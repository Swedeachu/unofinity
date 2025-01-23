using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A multi-cycle riffle shuffle that:
///  1) Splits the deck into two piles,
///  2) Riffles them near the center with random offsets,
///  3) Stacks them back together AT the center, 
///     smoothly restoring each card's original rotation,
///  4) Repeats for the specified number of cycles.
///
/// Each cycle is subdivided into SPLIT, RIFFLE, and STACK sub-phases.
/// We also stagger each card's motion by a small offset so they don't all move in unison.
/// </summary>
public class ShuffleCardsAction : IAction
{
  // Required by IAction
  public bool IsComplete => isComplete;
  public bool BypassPausing => false;

  // Callback when action fully completes
  private Action onComplete;
  private bool isComplete;

  // Cards to shuffle
  private List<GameObject> cards;

  // How many times to repeat the entire split→riffle→stack sequence
  private int cycleCount;

  // The "base" duration (in seconds) for each cycle, before we apply speed scaling.
  private float baseCycleDuration;

  // We’ll subdivide each cycle into phases by these fractions of baseCycleDuration:
  private float splitFraction = 0.40f;
  private float riffleFraction = 0.25f;
  // The remaining 35% is for the final stack

  // For per-card staggering (each card starts slightly after the previous one):
  private float perCardOffset = 0.02f;

  // We'll internally compute a "fullCycleTime" that includes:
  //    (baseCycleDuration / speed) + enough extra time so the LAST card can finish.
  private float fullCycleTime;      // total time each cycle actually gets
  private float splitEndTime;       // time in seconds at which SPLIT ends
  private float riffleEndTime;      // time in seconds at which RIFFLE ends
  private float stackEndTime;       // time in seconds at which STACK ends (= fullCycleTime)

  // Track progress: which cycle we’re on, how long (sec) we’ve been in the current cycle
  private int currentCycle;
  private float timeInCurrentCycle;

  // Precomputed positions/rotations for each sub-phase:
  private Vector3 stackCenter;            // Where the deck gathers
  private Vector3[] originalPositions;    // Each card’s starting pos (if you ever need it)
  private float[] originalRotationsY;    // Each card’s original Y-rotation

  private Vector3[] splitPositions;
  private float[] splitRotationsY;

  private Vector3[] rifflePositions;
  private float[] riffleRotationsY;

  public ShuffleCardsAction(List<GameObject> cards, int cycleCount = 2, float cycleDuration = 3.0f)
  {
    this.cards = cards;
    this.cycleCount = Mathf.Max(1, cycleCount);
    // We'll store the base cycle duration (pre-speed) and apply speed in StartAction
    this.baseCycleDuration = cycleDuration;
  }

  public void StartAction(Action onComplete)
  {
    this.onComplete = onComplete;
    isComplete = false;

    if (cards == null || cards.Count == 0)
    {
      isComplete = true;
      onComplete?.Invoke();
      return;
    }

    int count = cards.Count;

    // 1) Compute the "center" by averaging card positions
    Vector3 sum = Vector3.zero;
    for (int i = 0; i < count; i++)
    {
      sum += cards[i].transform.position;
    }
    stackCenter = sum / count;

    // 2) We'll store each card's original position/rotation
    originalPositions = new Vector3[count];
    originalRotationsY = new float[count];

    for (int i = 0; i < count; i++)
    {
      originalPositions[i] = cards[i].transform.position;
      originalRotationsY[i] = cards[i].transform.eulerAngles.y;
    }

    // 3) We define the final sub-phase as returning each card to (stackCenter, originalRotY).
    //    The "split" and "riffle" sub-phases need target positions/rotations:
    splitPositions = new Vector3[count];
    splitRotationsY = new float[count];
    rifflePositions = new Vector3[count];
    riffleRotationsY = new float[count];

    // We'll split the deck into two piles: left & right
    float pileOffset = 1.2f;
    Vector3 leftCenter = stackCenter + new Vector3(-pileOffset, 0f, 0f);
    Vector3 rightCenter = stackCenter + new Vector3(+pileOffset, 0f, 0f);
    float leftTilt = -15f;
    float rightTilt = +15f;

    // Riffle: random scatter around center
    float riffleRadius = 0.5f;

    for (int i = 0; i < count; i++)
    {
      bool leftSide = (i < count / 2);

      // Split
      splitPositions[i] = leftSide ? leftCenter : rightCenter;
      splitRotationsY[i] = leftSide ? leftTilt : rightTilt;

      // Riffle
      Vector2 rnd = UnityEngine.Random.insideUnitCircle * riffleRadius;
      rifflePositions[i] = stackCenter + new Vector3(rnd.x, 0f, rnd.y);
      riffleRotationsY[i] = UnityEngine.Random.Range(0f, 360f);
    }

    // 4) Now we compute the actual times for each cycle, including speed + staggering:
    //    baseCycleDuration is what the user wants for the main shuffle animation,
    //    but the last card also might start late, so let’s ensure we add enough time
    //    for the last card to finish properly.
    float adjustedBase = baseCycleDuration / GameManager.speed;
    // Because each successive card can start up to (count-1)*perCardOffset seconds later,
    // let’s add that to ensure the last card also has a full sub-phase:
    fullCycleTime = adjustedBase + (count - 1) * perCardOffset;

    // Next, define the sub-phase boundaries (in seconds) within that single cycle:
    // We'll do them as fractions of adjustedBase (not fullCycleTime),
    // so that the main animation phases keep correct relative lengths.
    // (The extra time is effectively a buffer to accommodate the late-starting cards.)
    float splitDuration = adjustedBase * splitFraction;
    float riffleDuration = adjustedBase * riffleFraction;
    float stackDuration = adjustedBase * (1f - splitFraction - riffleFraction);

    splitEndTime = splitDuration;
    riffleEndTime = splitDuration + riffleDuration;
    stackEndTime = splitDuration + riffleDuration + stackDuration;
    // stackEndTime should be <= adjustedBase
    // The leftover from adjustedBase..fullCycleTime is the “buffer” for the final card’s motion.

    // 5) Initialize cycle tracking
    currentCycle = 0;
    timeInCurrentCycle = 0f;
  }

  public void UpdateAction()
  {
    if (isComplete)
    {
      return;
    }

    // If we’ve finished all cycles, end
    if (currentCycle >= cycleCount)
    {
      // Already did everything
      EndShuffle();
      return;
    }

    // Update time in *this cycle*
    timeInCurrentCycle += Time.deltaTime;

    // If we've exceeded the full cycle time,
    // we forcibly finalize this cycle and move to the next:
    if (timeInCurrentCycle >= fullCycleTime)
    {
      // By design, the final sub-phase should have 
      // already placed cards at center + original rotation.
      // We do a quick “SnapAllToOriginalRotation” just for safety.
      SnapAllToOriginalRotation();

      currentCycle++;
      timeInCurrentCycle = 0f;

      // If we just finished the last cycle, we end:
      if (currentCycle >= cycleCount)
      {
        EndShuffle();
      }
      return;
    }

    // Otherwise, we are within the current cycle
    // We'll figure out each card's position based on sub-phase + per-card offset
    int count = cards.Count;

    for (int i = 0; i < count; i++)
    {
      GameObject card = cards[i];

      // Each card is offset in time
      float localTime = timeInCurrentCycle - i * perCardOffset;

      // If this card hasn't started moving yet, skip
      if (localTime < 0f)
      {
        // It's still waiting to begin this cycle's motion.
        // So it should remain in place from the end of the *previous* cycle.
        continue;
      }

      // If localTime surpasses the main animation window (stackEndTime),
      // there's a little leftover buffer, so the card is done moving.
      // By that point it *should* be at final position for this cycle anyway.
      if (localTime >= stackEndTime)
      {
        // The card should already be at center + original rotation 
        // (the end of the final sub-phase). But we can clamp:
        card.transform.position = stackCenter;
        float finalY = originalRotationsY[i];
        card.transform.eulerAngles = new Vector3(0f, finalY, 0f);
        continue;
      }

      // Figure out which sub-phase:
      if (localTime < splitEndTime)
      {
        // SPLIT PHASE
        float phaseRatio = Mathf.InverseLerp(0f, splitEndTime, localTime);
        float eased = EaseOutCubic(phaseRatio);

        // Start: (center, 0°)
        Vector3 startPos = stackCenter;
        float startRot = 0f;

        // End: (splitPositions[i], splitRotationsY[i])
        Vector3 endPos = splitPositions[i];
        float endRot = splitRotationsY[i];

        Vector3 curPos = Vector3.Lerp(startPos, endPos, eased);
        float curY = Mathf.LerpAngle(startRot, endRot, eased);

        card.transform.position = curPos;
        card.transform.eulerAngles = new Vector3(0f, curY, 0f);
      }
      else if (localTime < riffleEndTime)
      {
        // RIFFLE PHASE
        float phaseRatio = Mathf.InverseLerp(splitEndTime, riffleEndTime, localTime);
        float eased = EaseOutCubic(phaseRatio);

        // Start: split positions
        Vector3 startPos = splitPositions[i];
        float startRot = splitRotationsY[i];

        // End: riffle positions
        Vector3 endPos = rifflePositions[i];
        float endRot = riffleRotationsY[i];

        Vector3 curPos = Vector3.Lerp(startPos, endPos, eased);
        float curY = Mathf.LerpAngle(startRot, endRot, eased);

        card.transform.position = curPos;
        card.transform.eulerAngles = new Vector3(0f, curY, 0f);
      }
      else
      {
        // STACK PHASE (final sub-phase)
        // The card should move from the riffle positions/rotations
        // smoothly back to center + original rotation
        float phaseRatio = Mathf.InverseLerp(riffleEndTime, stackEndTime, localTime);
        float eased = EaseOutCubic(phaseRatio);

        // Start: riffle positions
        Vector3 startPos = rifflePositions[i];
        float startRot = riffleRotationsY[i];

        // End: (center, original rotation for this card)
        Vector3 endPos = stackCenter;
        float endRot = originalRotationsY[i];

        Vector3 curPos = Vector3.Lerp(startPos, endPos, eased);
        float curY = Mathf.LerpAngle(startRot, endRot, eased);

        card.transform.position = curPos;
        card.transform.eulerAngles = new Vector3(0f, curY, 0f);
      }
    }
  }

  private void EndShuffle()
  {
    // By the time we get here, we've done all cycles.
    // We'll do one final snap just to ensure no floating-point weirdness:
    SnapAllToOriginalRotation();
    isComplete = true;
    onComplete?.Invoke();
  }

  /// <summary>
  /// Places all cards exactly at the center with their original Y rotation.
  /// This won't cause a visible "jump" if they've already arrived naturally.
  /// </summary>
  private void SnapAllToOriginalRotation()
  {
    for (int i = 0; i < cards.Count; i++)
    {
      cards[i].transform.position = stackCenter;
      float y = originalRotationsY[i];
      cards[i].transform.eulerAngles = new Vector3(0f, y, 0f);
    }
  }

  /// <summary>
  /// Standard "ease-out cubic": 1 - (1 - t)^3
  /// </summary>
  private float EaseOutCubic(float t)
  {
    float inv = 1f - t;
    return 1f - (inv * inv * inv);
  }

}
