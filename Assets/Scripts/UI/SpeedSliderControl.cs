using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeedSliderControl : MonoBehaviour
{
  public Slider slider;
  public TextMeshProUGUI text;

  void Start()
  {
    // Get the Slider component
    slider = GetComponent<Slider>();

    // Subscribe to the onValueChanged event
    slider.onValueChanged.AddListener(Changed);

    slider.value = GameManager.speed;
  }

  public void Changed(float value)
  {
    // Round the value to 3 decimal places
    float roundedValue = Mathf.Round(value * 1000f) / 1000f;

    // Update UI text
    text.text = "GameSpeed: " + roundedValue;

    // Set game speed for real if not in auto mode to avoid changing while in chaos monkey mode calling this function back via slider.onValueChanged
    if (!GameManager.autoMode)
    {
      GameManager.speed = roundedValue;
    }
  }

  void OnDestroy()
  {
    // Unsubscribe to prevent memory leaks
    slider.onValueChanged.RemoveListener(Changed);
  }

}
