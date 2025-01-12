using System;
using UnityEngine;
using System.Collections.Generic;

public class RemoveCardFromPileAction : RelayoutAction
{

  private GameObject cardToRemove;

  public RemoveCardFromPileAction(CardPile pile, GameObject cardObj, float duration = 0.7f)
      : base(pile, duration)
  {
    this.cardToRemove = cardObj;
  }

  public override void StartAction(Action onComplete)
  {
    this.onComplete = onComplete;
    isComplete = false;

    // 1) Remove the card from the pile's array
    var cardList = new List<GameObject>(targetPile.cards);
    if (cardList.Contains(cardToRemove))
    {
      cardList.Remove(cardToRemove);
      targetPile.cards = cardList.ToArray();
    }

    // 2) Now that the card is removed, set up re-layout for the pile
    SetupPileForRelayout();

    // If the pile is empty, there's nothing to animate. Just finish immediately.
    if (pileCards.Count == 0)
    {
      OnActionComplete();
    }
  }

}
