using System;
using UnityEngine;
using System.Collections.Generic;

// moves a group of cards at once to a pile
public class RemoveCardFromPileAction : IAction
{

  private CardPile pile;
  private GameObject cardToRemove;
  private bool isComplete;
  private Action onComplete;

  public bool IsComplete => isComplete;

  public RemoveCardFromPileAction(CardPile pile, GameObject cardObj)
  {
    this.pile = pile;
    this.cardToRemove = cardObj;
  }

  public void StartAction(Action onComplete)
  {
    isComplete = false;
    this.onComplete = onComplete;
    // Remove the card
    RemoveCardFromPile();
    // Mark done
    isComplete = true;
    onComplete?.Invoke();
  }

  public void UpdateAction() { /* not used */ }

  private void RemoveCardFromPile()
  {
    var cardList = new List<GameObject>(pile.cards);
    if (cardList.Contains(cardToRemove))
    {
      cardList.Remove(cardToRemove);
      pile.cards = cardList.ToArray();
    }
  }

}
