using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

  public CardCollection deck;
  public TMPro.TextMeshProUGUI deckTextComponent;
  public GameObject deckObject;

  private List<GameObject> nonMiddlePiles = new List<GameObject>(); // All piles except for the middle pile
  private GameObject playerPile; // The special case pile controlled by the player
  private GameObject middlePile; // The pile all the cards go to

  public ActionBatchManager actionBatchManager; // Manages batches of actions
  public ActionRunner actionRunner; // Executes individual actions

  public CardObjectBuilder cardObjectBuilder;

  private TurnManager turnManager;

  private List<Player> playerList = new List<Player>();

  private void Awake()
  {
    actionRunner = gameObject.AddComponent<ActionRunner>();
    actionBatchManager = new ActionBatchManager(actionRunner);

    deck = new CardCollection();

    turnManager = new TurnManager(this);
  }

  private void Start()
  {
    // Find the deck object in the scene
    deckObject = GameObject.Find("Deck");
    if (deckObject != null)
    {
      Debug.Log("Found Deck Object");
      deckTextComponent = deckObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
      if (deckTextComponent != null)
      {
        Debug.Log("Found Deck Text");
      }
      else
      {
        Debug.LogError("Could not find Deck Text!");
      }
    }
    else
    {
      Debug.LogError("Could not find deck object in scene!");
    }

    // Fill the deck up
    InitializeDeck();

    // Find and assign all piles in the scene
    AssignPiles();

    StartGame();
  }

  private void InitializeDeck()
  {
    // Add 1 "0" card per color
    foreach (Card.CardColor color in Enum.GetValues(typeof(Card.CardColor)))
    {
      deck.Add(new Card { number = 0, color = color });
    }

    // Add 2 "1-9" cards per color
    foreach (Card.CardColor color in Enum.GetValues(typeof(Card.CardColor)))
    {
      for (int number = 1; number <= 9; number++)
      {
        deck.Add(new Card { number = number, color = color });
        deck.Add(new Card { number = number, color = color });
      }
    }

    deckTextComponent.text = deck.Count.ToString() + " Cards";

    // Shuffle the deck after initialization
    deck.Shuffle();
  }

  private void AssignPiles()
  {
    // Find all GameObjects with a CardPile component
    CardPile[] allPiles = FindObjectsOfType<CardPile>();

    int ids = -1;

    foreach (var pile in allPiles)
    {
      switch (pile.pileType)
      {
        case PileType.Player_Pile:

          if (playerPile == null)
          {
            playerPile = pile.gameObject;
            playerList.Add(new Player(++ids, true, "Player #" + ids, pile));
            Debug.Log("Assigned Player Pile");
          }
          else
          {
            Debug.LogError("Multiple Player_Pile detected in the scene!");
          }
          // also needs to be added to the non middle piles list
          nonMiddlePiles.Add(pile.gameObject);
          break;

        case PileType.Middle_Pile:

          if (middlePile == null)
          {
            middlePile = pile.gameObject;
            Debug.Log("Assigned Middle Pile");
          }
          else
          {
            Debug.LogError("Multiple Middle_Pile detected in the scene!");
          }
          break;

        default:

          playerList.Add(new Player(++ids, false, "Player #" + ids, pile));
          nonMiddlePiles.Add(pile.gameObject);
          Debug.Log("Assigned Non-Middle Pile");
          break;

      }
    }

    // Validation checks
    if (playerPile == null)
    {
      Debug.LogError("No Player_Pile found in the scene!");
    }

    if (middlePile == null)
    {
      Debug.LogError("No Middle_Pile found in the scene!");
    }

    Debug.Log($"Found {nonMiddlePiles.Count} Non-Middle Piles.");
  }

  // only virtual so MessAroundGameManager can override this for messing stuff up
  public virtual void StartGame()
  {
    // first thing to do is wait
    var initialActions = new List<IAction>();
    initialActions.Add(new DelayAction(1f));

    actionBatchManager.AddBatch(initialActions);

    foreach (GameObject pileObject in nonMiddlePiles)
    {
      // Get the CardPile component from the pile object
      CardPile pile = pileObject.GetComponent<CardPile>();
      if (pile == null)
      {
        Debug.LogError("Pile does not have a CardPile component!");
        continue;
      }

      // Prepare cards for the pile
      List<GameObject> cardsToAdd = new List<GameObject>();
      for (int i = 0; i < 7; i++)
      {
        if (deck.Count == 0)
        {
          Debug.LogError("Deck is empty!"); // maybe not an error, just trigger some sort of reshuffle/refill action
          return;
        }

        // Draw a card from the deck and create its GameObject
        Card card = deck.Draw();
        GameObject cardObject = cardObjectBuilder.MakeCard(card);
        cardObject.transform.position = deckObject.transform.position;

        // Add the card to the pile's list (internally for tracking)
        pile.AddCard(cardObject);

        // Add the card to the list of cards to move
        cardsToAdd.Add(cardObject);
      }

      // Create a MoveToPile action for all cards at once
      var moveToPileAction = new MoveMultipleCardsToPileAction(cardsToAdd, pile);
      actionBatchManager.AddBatch(new List<IAction> { moveToPileAction });
    }

    // update
    deckTextComponent.text = deck.Count.ToString() + " Cards";

    // Start processing all action batches, and calls back the turn manager on finishing
    actionBatchManager.StartProcessing(turnManager.OnAllInitialDealsComplete);
  }

  public GameObject GetMiddlePile()
  {
    return middlePile;
  }

  public List<Player> GetPlayers()
  {
    return playerList;
  }

  public TurnManager GetTurnManager()
  {
    return turnManager;
  }

}
