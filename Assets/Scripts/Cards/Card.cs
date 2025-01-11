public class Card
{

  // 0 to 9
  public int number;

  public enum CardColor
  {
    Red, Blue, Green, Yellow
  }

  public CardColor color;

  // Should be overridden for derived logic in custom cards like wild card, reverse, swap, etc.
  // By default, can play the card if color or number are equal.
  // The parameter card is the card to play ontop of in the middle pile.
  public virtual bool CanPlayOn(Card card)
  {
    return card.color == color || card.number == number;
  }

}
