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
  public List<string> playerOrder; // Names of the piles in the desired order which is configurable in the editor

  public static float speed = 1f;
  public bool autoMode = false;

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

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.A))
    {
      autoMode = !autoMode;
      if (autoMode) speed = 5f; else speed = 1f;
    }
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

    // After assigning the piles, sort the playerList based on the configured order
    if (playerOrder != null && playerOrder.Count > 0)
    {
      Debug.Log("Player Order Configured in Inspector: " + string.Join(", ", playerOrder));

      playerList.Sort((p1, p2) =>
      {
        // Normalize names by trimming and converting to lowercase
        string name1 = p1.CardPile.gameObject.name.Trim().ToLower();
        string name2 = p2.CardPile.gameObject.name.Trim().ToLower();

        Debug.Log($"Comparing {name1} and {name2}");

        // Normalize the playerOrder list for comparison
        int index1 = playerOrder.FindIndex(o => o.Trim().ToLower() == name1);
        int index2 = playerOrder.FindIndex(o => o.Trim().ToLower() == name2);

        Debug.Log($"Index in Player Order - {name1}: {index1}, {name2}: {index2}");

        // Handle names not found in the list
        index1 = index1 == -1 ? int.MaxValue : index1;
        index2 = index2 == -1 ? int.MaxValue : index2;

        return index1.CompareTo(index2);
      });
    }

    // Log the sorted list
    string plrs = "";
    for (int i = 0; i < playerList.Count; i++)
    {
      Player p = playerList[i];
      plrs += $"{p.CardPile.gameObject.name} | Index: {i} | IsHuman: {p.IsHuman}\n";
    }
    Debug.Log("Sorted Player List:\n" + plrs);

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
          Debug.LogWarning("Deck is empty!"); // maybe not an error, just trigger some sort of reshuffle/refill action
          return;
        }

        // should make a debug setting later on to make cards always face up no matter what
        bool faceUp = pile.pileType == PileType.Player_Pile;

        // Draw a card from the deck and create its GameObject
        Card card = deck.Draw();
        GameObject cardObject = cardObjectBuilder.MakeCard(card, faceUp);
        cardObject.transform.position = deckObject.transform.position;

        // Add the card to the pile's list (internally for tracking)
        pile.AddCard(cardObject);

        // Add the card to the list of cards to move
        cardsToAdd.Add(cardObject);
      }

      // Create a MoveToPile action for all cards at once
      if (pile.pileType != PileType.Player_Pile)
      {
        var moveToPileAction = new FanCardToPileAction(cardsToAdd[0], pile);
        actionBatchManager.AddBatch(new List<IAction> { moveToPileAction });
      }
      else
      {
        var moveToPileAction = new MoveCardToPileAction(cardsToAdd[0], pile);
        actionBatchManager.AddBatch(new List<IAction> { moveToPileAction });
      }
    }

    // update
    deckTextComponent.text = deck.Count.ToString() + " Cards";

    // Start processing all action batches, and calls back the turn manager on finishing
    actionBatchManager.StartProcessing(turnManager.OnAllInitialDealsComplete);
  }

  public void Restart()
  {
    deck = new CardCollection(); // replace completely
    InitializeDeck();

    actionRunner.GetActiveActions().Clear();
    actionBatchManager.GetBatches().Clear();
    actionBatchManager.isProcessing = false;

    playerList.Clear();

    // empty each array of game objects stored in the piles
    foreach (var pile in nonMiddlePiles)
    {
      pile.GetComponent<CardPile>().cards = new List<GameObject>().ToArray();
    }
    nonMiddlePiles.Clear();

    // empty the array as well too
    middlePile.GetComponent<CardPile>().cards = new List<GameObject>().ToArray();

    playerPile = null; // clear reference
    middlePile = null; // clear reference

    AssignPiles(); // reassign

    // destroy all cards in the scene
    var cards = GameObject.FindObjectsOfType<CardData>();
    foreach (CardData card in cards)
    {
      GameObject.Destroy(card.gameObject);
    }

    StartGame();
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
