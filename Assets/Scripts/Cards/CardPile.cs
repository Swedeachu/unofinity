using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class CardPile : MonoBehaviour
{

  [Header("Card Pile Configuration")]
  [Tooltip("The type of this card pile.")]
  public PileType pileType;
  [Tooltip("The spread type of this card pile.")]
  public SpreadType spreadType;

  [Header("Cards in the Pile")]
  [Tooltip("List of cards in this pile.")]
  public GameObject[] cards;

  public void AddCard(GameObject card)
  {
    if (ContainsCard(card)) return; // major safety check or things shit the bed

    var cardsList = new List<GameObject>(cards) { card };
    cards = cardsList.ToArray();
    // Debug.Log($"Added a card to {pileType}. Total cards: {cards.Length}");
  }

  public GameObject RemoveCard()
  {
    if (cards.Length == 0) return null;

    var cardsList = new List<GameObject>(cards);
    var card = cardsList[0];
    cardsList.RemoveAt(0);
    cards = cardsList.ToArray();
    // Debug.Log($"Removed a card from {pileType}. Total cards: {cards.Length}");

    return card;
  }

  public bool ContainsCard(GameObject card) { return cards.Contains(card); }

  public GameObject GetTopCard()
  {
    if (cards.Length == 0) return null;

    return cards[cards.Length - 1];
  }

  public List<GameObject> GetPlayableCardsOn(Card card)
  {
    // if no card, it's all of them we can play onto the empty pile as any of them work
    if (card == null) return cards.ToList<GameObject>();

    List<GameObject> cs = new List<GameObject>();

    foreach (GameObject c in cards)
    {
      var cd = c.GetComponent<CardData>();
      if (cd != null)
        if (cd.card.CanPlayOn(card)) cs.Add(c);
    }

    return cs;
  }

}
