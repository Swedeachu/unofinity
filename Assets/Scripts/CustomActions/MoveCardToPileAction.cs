using System;
using UnityEngine;

public class MoveCardToPileAction : RelayoutAction
{

  private GameObject cardToMove;

  public MoveCardToPileAction(GameObject card, CardPile target, float duration = 0.7f)
      : base(target, duration)
  {
    this.cardToMove = card;
  }

  public override void StartAction(Action onComplete)
  {
    this.onComplete = onComplete;
    isComplete = false;

    // 1) Immediately add this card to the pile so the final layout includes it

    // actually sometimes we don't need to add the card to the pile, so I made the CardPile class do a contains check internall when Add is called
    targetPile.AddCard(cardToMove); 

    // 2) Setup the re-layout so every card in targetPile shifts accordingly
    SetupPileForRelayout();
  }

}
