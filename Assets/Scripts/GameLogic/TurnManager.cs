using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class TurnManager
{

  private GameManager gameManager;
  private CardPile middlePile;
  private int currentPlayerIndex = 0;
  private int turnCount = 0;

  public bool playerAllowedToClick = false;

  public Player CurrentPlayer => gameManager.GetPlayers()[currentPlayerIndex];

  public TurnManager(GameManager gameManager)
  {
    this.gameManager = gameManager;
  }

  public void OnAllInitialDealsComplete()
  {
    Debug.Log("All initial deals complete. Starting turns...");
    currentPlayerIndex = math.max((gameManager.GetPlayers().Count - 1) / 2, 0);

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
    turnCount++;

    Debug.Log($"[TurnManager] Next Turn: {p.Name} (ID: {p.ID}, IsHuman: {p.IsHuman})");

    // Skip them if their hand is empty (game restarts if all hands are empty after turn 1)
    if (p.CardPile.cards.Length <= 0 && turnCount > 1)
    {
      Debug.Log($"[TurnManager] {p.Name} hand is empty");

      bool good = false;

      // check if all players hands are empty
      foreach (Player player in gameManager.GetPlayers())
      {
        if (player.CardPile.cards.Length > 0)
        {
          good = true;
          break;
        }
      }

      if (good)
      {
        NextTurn();
        return;
      }
      else
      {
        Debug.Log($"[TurnManager] RESTARTING!");
        gameManager.Restart();
        turnCount = 0;
        return;
      }
    }

    if (p.IsHuman && !gameManager.autoMode)
    {
      // First, check if they can play at least one card.
      // If not, do the auto draw-play loop and end turn.
      GameObject topCardObject = middlePile.GetTopCard();
      Card topCard = topCardObject ? topCardObject.GetComponent<CardData>().card : null;
      List<GameObject> playableCards = p.CardPile.GetPlayableCardsOn(topCard);

      if (playableCards.Count > 0)
      {
        // They can play, so let them click in the UI
        playerAllowedToClick = true;
      }
      else
      {
        // They cannot play anything, so do the auto-draw-then-play logic
        AttemptPlayOrDraw(p, EndCurrentPlayerTurn);
      }
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
    Card topCard = topCardObject?.GetComponent<CardData>().card;

    if (topCard == null)
    {
      // If no card in the middle, just play the first card from hand
      GameObject firstCard = aiPlayer.CardPile.GetTopCard();
      PlayCardToMiddlePile(firstCard);
    }
    else
    {
      List<GameObject> playableCards = aiPlayer.CardPile.GetPlayableCardsOn(topCard);

      if (playableCards.Count > 0)
      {
        // Play the first possible card
        GameObject firstCard = playableCards[0];
        PlayCardToMiddlePile(firstCard);
      }
      else
      {
        // If no playable card, keep drawing until we get one, then auto-play
        AttemptPlayOrDraw(aiPlayer, EndCurrentPlayerTurn);
      }
    }
  }

  private void AttemptPlayOrDraw(Player p, Action onComplete)
  {
    // Check the top card (could be null if middle pile is empty)
    GameObject topCardObj = middlePile.GetTopCard();
    Card topCard = topCardObj ? topCardObj.GetComponent<CardData>().card : null;

    List<GameObject> playableCards = p.CardPile.GetPlayableCardsOn(topCard);

    if (playableCards.Count > 0)
    {
      // Auto-play the first valid card
      PlayCardToMiddlePile(playableCards[0], onComplete);
    }
    else
    {
      // If no cards drawable, move all the card data from the middle pile into the deck data structure,
      // and delete all the game objects that were in the middle pile

      if (gameManager.deck.Count <= 0)
      {
        gameManager.RestoreDeck(); 
      }

      // No playable card, so we must draw.
      if (p.IsHuman)
      {
        var drawAction = new DrawCardAction(gameManager, p.CardPile);
        gameManager.actionBatchManager.AddBatch(new List<IAction> { drawAction });
      }
      else // AI player's hands are fanned
      {
        var drawAction = new DrawCardAndFanAction(gameManager, p.CardPile);
        gameManager.actionBatchManager.AddBatch(new List<IAction> { drawAction });
      }

      // After drawing completes, try again
      gameManager.actionBatchManager.StartProcessing(() =>
      {
        AttemptPlayOrDraw(p, onComplete);
      });
    }
  }

  // this ends the current players turn 
  private void PlayCardToMiddlePile(GameObject cardObj, Action onComplete = null)
  {
    var actions = new List<IAction>();

    // need to make the card appear face up as it is now played
    var cardData = cardObj.GetComponent<CardData>();
    if (cardData != null)
    {
      gameManager.cardObjectBuilder.SetCardPropertiesFaceUp(cardObj, cardData.card);
    }
    else
    {
      Debug.LogError("Could not find card data component when making card face up");
    }

    // 1) remove from player's pile depending on if they are a player or not
    if (CurrentPlayer.IsHuman)
    {
      actions.Add(new RemoveCardFromPileAction(CurrentPlayer.CardPile, cardObj));
    }
    else
    {
      actions.Add(new RemoveAndFanAction(CurrentPlayer.CardPile, cardObj)); // AI player piles are fanned
    }

    // Score is increased for the player on playing a card
    CurrentPlayer.score += 5;
    CurrentPlayer.CardPile.UpdateText(CurrentPlayer.score.ToString());
    var effectText = gameManager.MakeTextObject(middlePile.transform.position + new Vector3(45, 0, 0), "+5 Score!");

    // then make it fade in and then kill its self
    actions.Add(new FadeInOutAction(effectText, 0.1f, 0.5f, 0.1f));

    // 2) move the card object to the middle pile
    actions.Add(new MoveCardToPileAction(cardObj, middlePile));

    gameManager.actionBatchManager.AddBatch(actions);

    // If onComplete is null, default to ending the turn
    gameManager.actionBatchManager.StartProcessing(() =>
    {
      if (effectText != null) GameObject.Destroy(effectText); // remove this too now once all actions finish
      if (onComplete != null) onComplete();
      else EndCurrentPlayerTurn();
    });
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