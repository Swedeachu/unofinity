using System.Linq;
using UnityEngine;

public class PlayerInputRaycaster : MonoBehaviour
{

  private GameManager gameManager;
  private TurnManager turnManager;
  private CardPile middlePile;
  private Camera mainCamera;

  private void Start()
  {
    gameManager = FindObjectOfType<GameManager>();
    if (gameManager != null)
    {
      turnManager = gameManager.GetTurnManager();
    }

    if (mainCamera == null)
      mainCamera = Camera.main;
  }

  private void Update()
  {
    // If it's not the human player's turn, do nothing
    if (turnManager == null || !turnManager.playerAllowedToClick || !turnManager.CurrentPlayer.IsHuman) return;

    // Left click
    if (Input.GetMouseButtonDown(0))
    {
      Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out RaycastHit hit, 10000f))
      {
        CardData cardData = hit.collider.GetComponentInParent<CardData>();
        if (cardData != null)
        {
          AttemptPlayCard(cardData.gameObject);
        }
      }
    }
  }

  private void AttemptPlayCard(GameObject cardObj)
  {
    Player currentPlayer = turnManager.CurrentPlayer;

    if (!currentPlayer.CardPile.cards.Contains(cardObj))
    {
      Debug.Log("You can't click on that card!");
      return;
    }

    // lazy get it
    if (middlePile == null) middlePile = gameManager.GetMiddlePile().GetComponent<CardPile>();

    // If middle pile is empty, any card can be played
    if (middlePile.cards.Length == 0)
    {
      PlayCardActionSequence(cardObj);
    }
    else
    {
      // Check top card
      GameObject topCardObj = middlePile.GetTopCard();
      Card topCard = topCardObj.GetComponent<CardData>().card;

      Card cardToPlay = cardObj.GetComponent<CardData>().card;
      if (cardToPlay.CanPlayOn(topCard))
      {
        PlayCardActionSequence(cardObj);
      }
      else
      {
        Debug.Log("Card cannot be played on the current top card.");
      }
    }
  }

  /// <summary>
  /// Builds the action list for removing from the player hand,
  /// moving to the middle pile, then ends the turn.
  /// </summary>
  private void PlayCardActionSequence(GameObject cardObj)
  {
    turnManager.playerAllowedToClick = false; // no longer allowed to click

    var actions = new System.Collections.Generic.List<IAction>();

    // 1) remove from player's pile
    actions.Add(new RemoveCardFromPileAction(turnManager.CurrentPlayer.CardPile, cardObj));

    // 2) move the card object to the middle pile
    actions.Add(new MoveCardToPileAction(cardObj, middlePile));

    gameManager.actionBatchManager.AddBatch(actions);
    gameManager.actionBatchManager.StartProcessing(turnManager.EndCurrentPlayerTurn);
  }

}
