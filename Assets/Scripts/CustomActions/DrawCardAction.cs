using System;
using UnityEngine;

public class DrawCardAction : RelayoutAction
{

  private GameManager gm;

  public DrawCardAction(GameManager gm, CardPile pile, float duration = 1.0f)
      : base(pile, duration)
  {
    this.gm = gm;
  }

  public override void StartAction(Action onComplete)
  {
    this.onComplete = onComplete;
    isComplete = false;

    // 1) Draw from the deck
    Card drawnCard = gm.deck.Draw();
    if (drawnCard == null)
    {
      Debug.LogWarning("Deck is empty - maybe handle reshuffle or skip");
      isComplete = true;
      onComplete?.Invoke();
      return;
    }
    // Update text too
    gm.deckTextComponent.text = gm.deck.Count.ToString() + " Cards";

    // 2) Create the card gameobject & place it at deck position
    GameObject newCard = gm.cardObjectBuilder.MakeCard(drawnCard);
    if (newCard == null)
    {
      Debug.LogError("Failed to create card object!");
      isComplete = true;
      onComplete?.Invoke();
      return;
    }

    newCard.transform.position = gm.deckObject.transform.position;

    // 3) Add that new card to the pile so the final layout includes it
    targetPile.AddCard(newCard);

    // 4) Setup re-layout (the new card is in the list, 
    //    so we can animate the entire pile including the newly drawn card)
    SetupPileForRelayout();
  }

}
