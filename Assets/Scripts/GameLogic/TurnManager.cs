using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TurnManager
{

  private GameManager gameManager;
  private CardPile middlePile;
  private int currentPlayerIndex = 0;

  public bool playerAllowedToClick = false;

  public Player CurrentPlayer => gameManager.GetPlayers()[currentPlayerIndex];

  public TurnManager(GameManager gameManager)
  {
    this.gameManager = gameManager;
  }

  public void OnAllInitialDealsComplete()
  {
    Debug.Log("All initial deals complete. Starting turns...");
    currentPlayerIndex = math.max(gameManager.GetPlayers().Count - 1, 0); // should make things wrap around back to 0 for the human

    middlePile = gameManager.GetMiddlePile().GetComponent<CardPile>();

    StartTurnCycle();
  }

  private void StartTurnCycle()
  {
    NextTurn();
  }

  public void NextTurn()
  {
    currentPlayerIndex = (currentPlayerIndex + 1) % gameManager.GetPlayers().Count;
    Player p = CurrentPlayer;

    Debug.Log($"[TurnManager] Next Turn: {p.Name} (ID: {p.ID}, IsHuman: {p.IsHuman})");

    // If human, we rely on the PlayerInputRaycaster to handle playing or drawing
    // The TurnManager just sets this flag so the UI code knows it's permissible.
    if (p.IsHuman)
    {
      playerAllowedToClick = true;
    }
    else
    {
      // AI Turn
      HandleAIPlay(p);
    }
  }

  private void HandleAIPlay(Player aiPlayer)
  {
    GameObject topCardObject = middlePile.GetTopCard();
    Card topCard = topCardObject.GetComponent<CardData>().card;

    if (topCard == null)
    {
      // if no card, we can play whatever card we want in our hand, so we just do the first (this isn't very strategic) 
      GameObject firstCard = aiPlayer.CardPile.GetTopCard();
      PlayCardToMiddlePile(firstCard);
    }
    else
    {
      // get the possible cards in our hand we can play
      List<GameObject> playableCards = aiPlayer.CardPile.GetPlayableCardsOn(topCard);

      if (playableCards.Count > 0)
      {
        // play the first possible card in the hand onto the middle pile (also not very strategic) 
        GameObject firstCard = playableCards[0];
        PlayCardToMiddlePile(firstCard);
      }
      else
      {
        // draw cards until we get a possible card and then automatically play it (TODO with actions later)
      }
    }
  }

  // this ends the current players turn 
  private void PlayCardToMiddlePile(GameObject cardObj)
  {
    var actions = new List<IAction>();

    // 1) remove from player's pile
    actions.Add(new RemoveCardFromPileAction(CurrentPlayer.CardPile, cardObj));

    // 2) move the card object to the middle pile
    actions.Add(new MoveCardToPileAction(cardObj, middlePile));

    gameManager.actionBatchManager.AddBatch(actions);
    gameManager.actionBatchManager.StartProcessing(EndCurrentPlayerTurn);
  }

  public void EndCurrentPlayerTurn()
  {
    Debug.Log($"[TurnManager] Ending turn for {CurrentPlayer.Name}");

    var middlePile = gameManager.GetMiddlePile().GetComponent<CardPile>();

    UpdateMiddlePileVisibility();

    NextTurn();
  }

  /// <summary>
  /// Only enable the topmost card's renderers.
  /// </summary>
  private void UpdateMiddlePileVisibility()
  {
    for (int i = 0; i < middlePile.cards.Length; i++)
    {
      bool isTopCard = (i == middlePile.cards.Length - 1);
      GameObject card = middlePile.cards[i];

      // For any kind of renderer (MeshRenderer, SpriteRenderer, etc.)
      foreach (var renderer in card.GetComponentsInChildren<Renderer>())
      {
        renderer.enabled = isTopCard;
      }

      // For TextMeshPro
      foreach (var textRenderer in card.GetComponentsInChildren<TMPro.TextMeshProUGUI>())
      {
        textRenderer.enabled = isTopCard;
      }
    }
  }

}
