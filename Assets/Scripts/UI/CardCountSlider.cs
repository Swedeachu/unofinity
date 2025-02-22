using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardCountSlider : MonoBehaviour
{

  public Slider slider;
  public TextMeshProUGUI text;

  void Start()
  {
    // Get the Slider component
    slider = GetComponent<Slider>();

    // Subscribe to the onValueChanged event
    slider.onValueChanged.AddListener(Changed);

    slider.value = GameManager.startingCards;
  }

  public void Changed(float value)
  {
    int roundedValue = (int)value;

    // Update UI text
    text.text = "Starting hand size: " + roundedValue;

    // Set 
    GameManager.startingCards = roundedValue;
  }

  void OnDestroy()
  {
    // Unsubscribe to prevent memory leaks
    slider.onValueChanged.RemoveListener(Changed);
  }

}
