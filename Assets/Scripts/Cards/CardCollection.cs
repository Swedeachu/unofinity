using System.Collections.Generic;

public class CardCollection
{

  private List<Card> cards = new List<Card>();

  public int Count => cards.Count;

  // Add a card to the collection
  public void Add(Card card)
  {
    cards.Add(card);
  }

  // Remove a card from the collection
  public bool Remove(Card card)
  {
    return cards.Remove(card);
  }

  // Draw and remove the top card from the collection
  // You should check if this returns null!
  public Card Draw()
  {
    if (cards.Count == 0)
    {
      return null; 
    }
    Card topCard = cards[cards.Count - 1];
    cards.RemoveAt(cards.Count - 1);
    return topCard;
  }

  // Get the top card without removing it
  // You should check if this returns null!
  public Card GetTopCard()
  {
    if (cards.Count == 0)
    {
      return null; // Or throw an exception
    }
    return cards[cards.Count - 1];
  }

  public void Shuffle()
  {
    System.Random rng = new System.Random();
    int n = cards.Count;
    while (n > 1)
    {
      n--;
      int k = rng.Next(n + 1);
      Card value = cards[k];
      cards[k] = cards[n];
      cards[n] = value;
    }
  }

}
