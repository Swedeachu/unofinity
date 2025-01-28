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
      if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
      {
        // Debug.Log("Hit " + hit.collider.gameObject.name);
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

  private void PlayCardActionSequence(GameObject cardObj)
  {
    turnManager.playerAllowedToClick = false; // no longer allowed to click

    var actions = new System.Collections.Generic.List<IAction>();

    // 1) remove from player's pile
    actions.Add(new RemoveCardFromPileAction(turnManager.CurrentPlayer.CardPile, cardObj));

    // 2) move the card object to the middle pile
    actions.Add(new MoveCardToPileAction(cardObj, middlePile));

    // Score is increased for the player on playing a card
    turnManager.CurrentPlayer.score += 5;
    turnManager.CurrentPlayer.CardPile.UpdateText(turnManager.CurrentPlayer.score.ToString());

    // make text effect which fades in then kills its self
    var effectText = gameManager.MakeTextObject(middlePile.transform.position + new Vector3(45, 0, 0), "+5 Score!");
    actions.Add(new FadeInOutAction(effectText, 0.1f, 0.5f, 0.1f));

    gameManager.actionBatchManager.AddBatch(actions);

    gameManager.actionBatchManager.StartProcessing(() =>
    {
      if (effectText != null) GameObject.Destroy(effectText);
      turnManager.EndCurrentPlayerTurn();
    });

    // gameManager.actionBatchManager.StartProcessing(turnManager.EndCurrentPlayerTurn);
  }

}
