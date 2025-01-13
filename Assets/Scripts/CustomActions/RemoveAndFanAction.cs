using System;
using UnityEngine;
using System.Collections.Generic;

public class RemoveAndFanAction : FanCardToPileAction
{

  private GameObject cardToRemove;

  public RemoveAndFanAction(CardPile pile, GameObject cardObj, float duration = 0.7f)
      : base(null, pile, duration) // Passing null as the card to move since we're removing
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

    // 2) Set up fan layout for the remaining cards
    pileCards = new List<GameObject>(targetPile.cards);
    SetupFanLayout();

    // If the pile is empty, finish immediately
    if (pileCards.Count == 0)
    {
      OnActionComplete();
    }
  }

}
