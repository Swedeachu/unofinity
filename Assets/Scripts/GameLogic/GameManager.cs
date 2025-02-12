using System;
using System.Collections.Generic;
using TMPro;
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
  private List<GameObject> textLabels = new List<GameObject>();

  private Canvas canvas;

  // Set me in the editor!
  public GameObject textPrefab;
  public GameObject pilePrefab;

  public static float speed = 1f;
  public bool autoMode = false;
  public static int startingCards = 7; // can be changed via pause menu, this is how many cards to deal out at the start

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

    // Find the Canvas in the scene we care about
    var canvases = FindObjectsOfType<Canvas>();
    foreach (Canvas c in canvases)
    {
      if (c.name == "MainUI")
      {
        canvas = c;
        break;
      }
    }

    if (canvas != null)
    {
      // Collect all child GameObjects
      CollectChildObjects(canvas.gameObject);
    }
    else
    {
      Debug.LogWarning("No Canvas found in the scene.");
    }

    // Fill the deck up
    InitializeDeck();

    // Find and assign all piles in the scene
    AssignPiles();

    StartGame();
  }

  private void CollectChildObjects(GameObject parent)
  {
    // Iterate through all child Transforms
    foreach (Transform child in parent.transform)
    {
      // Add the child GameObject to the list
      textLabels.Add(child.gameObject);

      // Recursively collect children of this child
      CollectChildObjects(child.gameObject);
    }
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.A))
    {
      autoMode = !autoMode;
      if (autoMode) speed = 5f; else speed = 1f;
    }
  }

  public void AddPlayer()
  {
    Debug.Log("Adding player");

    // Determine the best position for the new pile (we do this kinda ass)
    Vector3 newPilePosition = Vector3.zero;

    if (nonMiddlePiles.Count > 0)
    {
      // Calculate an appropriate position by expanding outward from existing piles
      float spacing = 2.5f; // Adjust spacing as needed
      int rowSize = Mathf.CeilToInt(Mathf.Sqrt(nonMiddlePiles.Count + 1)); // Spread them in a grid
      int row = nonMiddlePiles.Count / rowSize;
      int col = nonMiddlePiles.Count % rowSize;

      newPilePosition = new Vector3(col * spacing, 0, row * spacing);
    }

    // Instantiate new player pile
    GameObject p = GameObject.Instantiate(pilePrefab, newPilePosition, Quaternion.identity);

    p.GetComponent<MeshRenderer>().enabled = false;

    var pile = p.GetComponent<CardPile>();
    if (pile != null)
    {
      // Add to lists
      nonMiddlePiles.Add(p);
      playerList.Add(new Player(playerList.Count, false, "Player #" + playerList.Count, pile));

      // Add actions to make them draw cards (starting hand amount in a new batch)
      for (int i = 0; i < startingCards; i++)
      {
        DrawCardToPile(pile);
      }

      // Start processing all action batches if not already running
      if (!actionBatchManager.isProcessing)
      {
        actionBatchManager.StartProcessing();
      }
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
          // Also needs to be added to the non-middle piles list
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

    // Sort the playerList based on playerOrder
    if (playerOrder != null && playerOrder.Count > 0)
    {
      Debug.Log("Player Order Configured in Inspector: " + string.Join(", ", playerOrder));

      playerList.Sort((p1, p2) =>
      {
        string name1 = p1.CardPile.gameObject.name.Trim().ToLower();
        string name2 = p2.CardPile.gameObject.name.Trim().ToLower();

        int index1 = playerOrder.FindIndex(o => o.Trim().ToLower() == name1);
        int index2 = playerOrder.FindIndex(o => o.Trim().ToLower() == name2);

        index1 = index1 == -1 ? int.MaxValue : index1;
        index2 = index2 == -1 ? int.MaxValue : index2;

        return index1.CompareTo(index2);
      });
    }

    // Sort nonMiddlePiles to match the sorted playerList
    nonMiddlePiles = new List<GameObject>(playerList.ConvertAll(player => player.CardPile.gameObject));

    // Log the sorted lists
    Debug.Log($"Found {nonMiddlePiles.Count} Non-Middle Piles.");
    Debug.Log("Sorted Non-Middle Piles: " + string.Join(", ", nonMiddlePiles.ConvertAll(p => p.name)));
    Debug.Log("Sorted Player List: " + string.Join(", ", playerList.ConvertAll(p => p.CardPile.gameObject.name)));
  }

  // only virtual so MessAroundGameManager can override this for messing stuff up
  public virtual void StartGame()
  {
    // hide the UI labels
    foreach (var obj in textLabels)
    {
      TransparencyUtils.SetTransparency(obj, 0);
    }

    // First thing to do is wait
    actionBatchManager.AddBatch(new List<IAction>() { new DelayAction(1f) });

    // We do a shuffle animation action first
    var activeCards = new List<GameObject>();
    var cardData = new List<Card>();

    // Draw and initialize the cards
    for (int i = 0; i < 60; i++)
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
      cardData.Add(card);
    }

    // Run the animation
    actionBatchManager.AddBatch(new List<IAction>() { new ShuffleCardsAction(activeCards, 2, 0.8f) });

    // Destroy all the card game objects and put the card data back into the deck card collection
    // We do this because we were just using them temporarily as objects for the shuffle action animation
    actionBatchManager.AddBatch(new List<IAction>() { new CallbackAction(() =>
    {
      foreach(var card in activeCards)
      {
        if (card != null)
        {
          Destroy(card);
        }
      }

      foreach(var card in cardData)
      {
        deck.Add(card);
      }
    })});

    // Then make the UI fade in
    var transparencyActions = new List<IAction>();
    foreach (var obj in textLabels)
    {
      transparencyActions.Add(new TransparencyAction(obj, 1, 0.5f));
    }
    actionBatchManager.AddBatch(transparencyActions);

    // After shuffling we deal the cards and start the game
    actionBatchManager.StartProcessing(DealAndStart);
  }

  private void DealAndStart()
  {
    for (int i = 0; i < startingCards; i++) // Loop to deal one card at a time to each pile
    {
      foreach (GameObject pileObject in nonMiddlePiles)
      {
        // Get the CardPile component from the pile object
        CardPile pile = pileObject.GetComponent<CardPile>();
        if (pile == null)
        {
          Debug.LogError("Pile does not have a CardPile component!");
          continue;
        }

        // Add a CallbackAction for dealing a card to this pile
        DrawCardToPile(pile);
      }
    }

    // Start processing all action batches, and calls back the turn manager on finishing
    actionBatchManager.StartProcessing(turnManager.OnAllInitialDealsComplete);
  }

  // this is an insane amount of callbacks 
  private void DrawCardToPile(CardPile pile)
  {
    // Add a CallbackAction for dealing a card to this pile
    actionBatchManager.AddBatch(new List<IAction>
    {
        new CallbackAction(() =>
        {
            if (deck.Count == 0)
            {
                Debug.Log("Deck is empty, restoring from middle pile!");
                RestoreDeck(); // remakes the deck if needed
            }

            // callback action to draw a card after the deck is remade
            actionBatchManager.AddBatch(new List<IAction>
            {
                new CallbackAction(() =>
                {
                    // Determine if the card should be face up
                    bool faceUp = pile.pileType == PileType.Player_Pile;

                    // Draw a card from the deck and create its GameObject
                    Card card = deck.Draw();
                    GameObject cardObject = cardObjectBuilder.MakeCard(card, faceUp);
                    cardObject.transform.position = deckObject.transform.position;

                    // Update the deck text component
                    deckTextComponent.text = deck.Count.ToString() + " Cards";

                    // Create an action to move the card to the pile
                    IAction moveToPileAction;

                    if (pile.pileType != PileType.Player_Pile)
                    {
                        moveToPileAction = new FanCardToPileAction(cardObject, pile, 0.3f);
                    }
                    else
                    {
                        moveToPileAction = new MoveCardToPileAction(cardObject, pile, 0.3f);
                    }

                    // Add the move action in a new batch to execute after the card creation
                    actionBatchManager.AddBatch(new List<IAction> { moveToPileAction });
                })
            });
        })
    });
  }

  // Moves the middle pile back into the deck
  public void RestoreDeck()
  {
    Debug.Log("Refilling deck from middle!");
    List<IAction> moves = new List<IAction>();
    List<IAction> destroys = new List<IAction>();

    var middlePile = GetMiddlePile().GetComponent<CardPile>();

    foreach (var gameObject in middlePile.cards)
    {
      var cd = gameObject.GetComponent<CardData>();
      if (cd != null)
      {
        deck.Add(cd.card);
        moves.Add(new RotateAndMoveAction(gameObject, deckObject.transform.position, 0, 0.5f));
        destroys.Add(new CallbackAction(() =>
        {
          GameObject.Destroy(gameObject);
        }));
      }
    }
    // clear internal data of the middle pile first
    middlePile.cards = new List<GameObject>().ToArray();
    actionBatchManager.AddBatch(moves);
    actionBatchManager.AddBatch(destroys);
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

  public GameObject MakeTextObject(Vector3 spawnPos, string text)
  {
    if (textPrefab != null && canvas != null)
    {
      // Instantiate the prefab
      GameObject instance = Instantiate(textPrefab);

      // Set the instantiated prefab as a child of the Canvas
      instance.transform.SetParent(canvas.transform, false);
      instance.transform.localPosition = spawnPos;

      TextMeshProUGUI textComp = instance.GetComponent<TextMeshProUGUI>();
      if (textComp != null)
      {
        textComp.text = text;
      }
      else
      {
        Debug.LogWarning("no text component found on game object");
      }

      return instance;
    }
    else
    {
      Debug.LogError("Prefab or Canvas is not assigned in the Inspector.");
    }

    return null;
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
