public class Player
{

  public int ID { get; private set; }

  public bool IsHuman { get; private set; }

  public string Name { get; private set; }

  public int score = 0;

  // the card pile essentially works as the hand
  public CardPile CardPile { get; private set; }

  public Player(int id, bool isHuman, string name, CardPile cardPile)
  {
    ID = id;
    IsHuman = isHuman;
    Name = name;
    CardPile = cardPile;  
  }

}
