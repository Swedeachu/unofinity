using System;
using System.Collections.Generic;
using UnityEngine;

public class DrawCardAndFanAction : FanCardToPileAction
{

  private GameManager gm;

  public DrawCardAndFanAction(GameManager gm, CardPile pile, float duration = 1.0f)
      : base(null, pile, duration) // Passing null as the cardToMove since we're drawing
  {
    this.gm = gm;
  }

  public override void StartAction(Action onComplete)
  {
    this.onComplete = onComplete;
    isComplete = false;

    // 1) Draw a card from the deck
    Card drawnCard = gm.deck.Draw();
    if (drawnCard == null)
    {
      Debug.LogWarning("Deck is empty - maybe handle reshuffle or skip");
      isComplete = true;
      onComplete?.Invoke();
      return;
    }

    // Update deck text
    gm.deckTextComponent.text = gm.deck.Count.ToString() + " Cards";

    // Determine if the card should be face-up (for player piles)
    bool faceUp = targetPile.pileType == PileType.Player_Pile;

    // 2) Create the new card GameObject & place it at the deck's position
    GameObject newCard = gm.cardObjectBuilder.MakeCard(drawnCard, faceUp);
    if (newCard == null)
    {
      Debug.LogError("Failed to create card object!");
      isComplete = true;
      onComplete?.Invoke();
      return;
    }

    newCard.transform.position = gm.deckObject.transform.position;

    // 3) Add the new card to the target pile
    targetPile.AddCard(newCard);

    // 4) Set up the fan layout with the new card included
    pileCards = new List<GameObject>(targetPile.cards);
    SetupFanLayout();

    // If the pile is empty, finish immediately
    if (pileCards.Count == 0)
    {
      OnActionComplete();
    }
  }

}
