using System.Collections.Generic;
using UnityEngine;

public class MessAroundGameManager : GameManager
{

  [Header("Mess Around Config")]
  public int numCardsToDraw = 30; // Number of cards to draw initially
  public float tossRadius = 5f;  // Radius of the surface area where cards are tossed
  public float tossDuration = 1.5f; // Duration of the card toss animation

  private List<GameObject> activeCards = new List<GameObject>();

  public override void StartGame()
  {
    // Draw and initialize the cards
    for (int i = 0; i < numCardsToDraw; i++)
    {
      if (deck.Count == 0)
      {
        Debug.LogError("Deck is empty!");
        return;
      }

      // Draw a card from the deck
      Card card = deck.Draw();
      GameObject cardObject = cardObjectBuilder.MakeCard(card, true);
      cardObject.transform.position = deckObject.transform.position;

      // Add the card to the active cards list
      activeCards.Add(cardObject);
    }

    // Update text on the deck
    deckTextComponent.text = deck.Count.ToString() + " Cards";

    // Start the first cycle of actions
    StartTossCycle();
  }

  private void StartTossCycle()
  {
    List<IAction> tossActions = new List<IAction>();

    // Assign a new RotateAndMoveAction to each card
    foreach (GameObject card in activeCards)
    {
      Vector3 randomPosition = GetRandomPosition();
      float randomRotation = Random.Range(0f, 360f);

      tossActions.Add(new RotateAndMoveAction(card, randomPosition, randomRotation, tossDuration));
      tossActions.Add(new RotateAction(card, randomRotation, tossDuration)); // just for check offs to show that I have a rotate action seperate
    }

    // Add a delay action before starting the next cycle
    tossActions.Add(new DelayAction(2f));

    // Add the batch to the ActionBatchManager
    actionBatchManager.AddBatch(tossActions);

    // Set up the callback to start the next cycle after the batch completes
    actionBatchManager.StartProcessing(OnTossCycleComplete);
  }

  private void OnTossCycleComplete()
  {
    // Once all actions complete, start a new cycle
    StartTossCycle();
  }

  private Vector3 GetRandomPosition()
  {
    // Generate a random position within a circle on the XZ plane
    Vector2 randomCircle = Random.insideUnitCircle * tossRadius;
    return new Vector3(randomCircle.x, 0, randomCircle.y); // Maintain Y=0 (or original Y)
  }

}
