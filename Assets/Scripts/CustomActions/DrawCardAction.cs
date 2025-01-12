using System;
using UnityEngine;

// TODO: make this draw in proper spacing with the other cards in the target pile
public class DrawCardAction : IAction
{

  private GameManager gm;
  private CardPile targetPile; // The player's or AI's hand pile
  private GameObject cardObject;
  private Vector3 startPosition;
  private Vector3 endPosition;
  private float duration = 1.0f; // Time to complete the animation
  private float elapsedTime;
  private bool isComplete;
  private Action onComplete;

  public bool IsComplete => isComplete;

  public DrawCardAction(GameManager gm, CardPile pile)
  {
    this.gm = gm;
    this.targetPile = pile;
  }

  public void StartAction(Action onComplete)
  {
    isComplete = false;
    this.onComplete = onComplete;
    elapsedTime = 0f;

    // Draw from the deck
    Card drawnCard = gm.deck.Draw();
    if (drawnCard == null)
    {
      Debug.LogWarning("Deck is empty - need to handle reshuffle or skip??");
      isComplete = true;
      onComplete?.Invoke();
      return;
    }

    // Create the card object
    cardObject = gm.cardObjectBuilder.MakeCard(drawnCard);
    if (cardObject == null)
    {
      Debug.LogError("Failed to create card object!");
      isComplete = true;
      onComplete?.Invoke();
      return;
    }

    // Set start and end positions
    startPosition = gm.deckObject.transform.position;
    endPosition = CalculateTargetPosition();

    // Place the card at the start position initially
    cardObject.transform.position = startPosition;
  }

  public void UpdateAction()
  {
    if (isComplete || cardObject == null) return;

    // Animate the movement
    elapsedTime += Time.deltaTime;
    float t = Mathf.Clamp01(elapsedTime / duration);

    // Smooth animation using ease-out curve
    float easedT = EaseOutCubic(t);
    cardObject.transform.position = Vector3.Lerp(startPosition, endPosition, easedT);

    // If animation is complete
    if (t >= 1f)
    {
      // Add card to the player's pile
      targetPile.AddCard(cardObject);

      isComplete = true;
      onComplete?.Invoke();
    }
  }

  private Vector3 CalculateTargetPosition()
  {
    // Calculate the target position based on the pile's spread type
    Vector3 basePosition = targetPile.transform.position;

    if (targetPile.spreadType == SpreadType.LeftToRight)
    {
      // Calculate offset for the card's position in the hand
      int totalCards = targetPile.cards.Length + 1; // Include the new card
      float totalWidth = (totalCards - 1) * 1.2f; // Spacing of 1.2 units
      float startX = -totalWidth / 2; // Center the cards
      return basePosition + new Vector3(startX + (totalCards - 1) * 1.2f, 0, 0);
    }

    // Default to stack at the base position
    return basePosition;
  }

  private float EaseOutCubic(float t)
  {
    return 1 - Mathf.Pow(1 - t, 3);
  }

}
