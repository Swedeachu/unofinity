public enum PileType
{
  Player_Pile,
  AI_Pile,
  Middle_Pile
}

public enum SpreadType
{
  Top, // just place cards on top of each other directly (like the deck or middle pile)
  LeftToRight, // first card in the pile is leftmost placed, the hand piles follow this for each player
}