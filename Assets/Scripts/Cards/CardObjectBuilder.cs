using UnityEngine;

[CreateAssetMenu(fileName = "CardObjectBuilder", menuName = "Card Builder")]
public class CardObjectBuilder : ScriptableObject
{

  [Header("The Card Prefab to clone")]
  public GameObject cardPrefab; 

  /// <summary>
  /// Creates a GameObject representation of the card.
  /// </summary>
  /// <param name="card">The card data to represent.</param>
  /// <returns>A GameObject configured with the card's properties.</returns>
  public GameObject MakeCard(Card card)
  {
    if (cardPrefab == null)
    {
      Debug.LogError("Card prefab is not assigned!");
      return null;
    }

    // Clone the prefab
    GameObject cardObject = GameObject.Instantiate(cardPrefab);

    // Set card properties on the GameObject (e.g., visuals, text, color)
    SetCardProperties(cardObject, card);

    return cardObject;
  }

  /// <summary>
  /// Configures the GameObject based on the card data.
  /// </summary>
  /// <param name="cardObject">The GameObject representing the card.</param>
  /// <param name="card">The card data to apply.</param>
  private void SetCardProperties(GameObject cardObject, Card card)
  {
    // Set the card number text
    var textComponent = cardObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
    if (textComponent != null)
    {
      textComponent.text = card.number.ToString();
    }

    // Set the card color
    var renderer = cardObject.GetComponentInChildren<Renderer>();
    if (renderer != null)
    {
      renderer.material.color = GetColorFromCard(card.color);
    }

    // Assign the card data to a component for later reference
    CardData data = cardObject.AddComponent<CardData>();
    data.SetCardData(card);
  }

  /// <summary>
  /// Maps the CardColor enum to a Unity color.
  /// </summary>
  /// <param name="cardColor">The color of the card.</param>
  /// <returns>A Unity Color corresponding to the card color.</returns>
  private Color GetColorFromCard(Card.CardColor cardColor)
  {
    switch (cardColor)
    {
      case Card.CardColor.Red:
      return Color.red;
      case Card.CardColor.Blue:
      return Color.blue;
      case Card.CardColor.Green:
      return Color.green;
      case Card.CardColor.Yellow:
      return Color.yellow;
      default:
      return Color.white;
    }
  }

}
