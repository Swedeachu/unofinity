using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class TransparencyUtils
{

  /// <summary>
  /// Sets the transparency of all compatible components (Renderers, UI elements, TextMeshPro) in a GameObject and its children.
  /// </summary>
  /// <param name="target">The GameObject to update.</param>
  /// <param name="transparency">The target transparency (0 to 1).</param>
  public static void SetTransparency(GameObject target, float transparency)
  {
    // Clamp transparency to a valid range
    transparency = Mathf.Clamp01(transparency);

    // Iterate through Renderers
    Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
    foreach (Renderer renderer in renderers)
    {
      if (renderer.material.HasProperty("_Color"))
      {
        Color color = renderer.material.color;
        color.a = transparency;
        renderer.material.color = color;
      }
    }

    // Iterate through UI elements (e.g., Image, RawImage, Text)
    Graphic[] uiElements = target.GetComponentsInChildren<Graphic>();
    foreach (Graphic uiElement in uiElements)
    {
      Color color = uiElement.color;
      color.a = transparency;
      uiElement.color = color;
    }

    // Iterate through TextMeshPro components
    TextMeshPro[] textMeshPros = target.GetComponentsInChildren<TextMeshPro>();
    foreach (TextMeshPro tmp in textMeshPros)
    {
      Color color = tmp.color;
      color.a = transparency;
      tmp.color = color;
    }

    TextMeshProUGUI[] textMeshProUGUIs = target.GetComponentsInChildren<TextMeshProUGUI>();
    foreach (TextMeshProUGUI tmpUGUI in textMeshProUGUIs)
    {
      Color color = tmpUGUI.color;
      color.a = transparency;
      tmpUGUI.color = color;
    }
  }

}
