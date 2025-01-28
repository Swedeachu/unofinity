using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TransparencyAction : IAction
{
  public bool IsComplete => isComplete;
  public bool BypassPausing => false;

  private GameObject targetObject;
  private float targetTransparency;
  private float duration;
  private Action onComplete;

  private float elapsedTime;
  private bool isComplete;

  // Cache of components and their original colors
  private Renderer[] renderers;
  private Graphic[] uiElements;
  private TextMeshPro[] textMeshPros;
  private TextMeshProUGUI[] textMeshProUGUIs;
  private Color[] originalColors;

  public TransparencyAction(GameObject targetObject, float targetTransparency, float duration)
  {
    this.targetObject = targetObject;
    this.targetTransparency = Mathf.Clamp01(targetTransparency);
    this.duration = duration;

    // Collect all components and cache their original colors
    CollectComponents();
  }

  public void StartAction(Action onComplete)
  {
    this.onComplete = onComplete;
    elapsedTime = 0f;
    isComplete = false;
  }

  public void UpdateAction()
  {
    if (isComplete) return;

    elapsedTime += Time.deltaTime;
    float t = Mathf.Clamp01(elapsedTime / duration);

    // Lerp transparency for all cached components
    ApplyTransparency(t);

    if (t >= 1f)
    {
      isComplete = true;
      onComplete?.Invoke();
    }
  }

  private void CollectComponents()
  {
    // Collect renderers
    renderers = targetObject.GetComponentsInChildren<Renderer>();

    // Collect UI elements
    uiElements = targetObject.GetComponentsInChildren<Graphic>();

    // Collect TextMeshPro components
    textMeshPros = targetObject.GetComponentsInChildren<TextMeshPro>();
    textMeshProUGUIs = targetObject.GetComponentsInChildren<TextMeshProUGUI>();

    // Cache original colors
    int totalComponents = renderers.Length + uiElements.Length + textMeshPros.Length + textMeshProUGUIs.Length;
    originalColors = new Color[totalComponents];
    int index = 0;

    foreach (var renderer in renderers)
    {
      if (renderer.material.HasProperty("_Color"))
      {
        originalColors[index++] = renderer.material.color;
      }
    }

    foreach (var uiElement in uiElements)
    {
      originalColors[index++] = uiElement.color;
    }

    foreach (var tmp in textMeshPros)
    {
      originalColors[index++] = tmp.color;
    }

    foreach (var tmpUGUI in textMeshProUGUIs)
    {
      originalColors[index++] = tmpUGUI.color;
    }
  }

  private void ApplyTransparency(float t)
  {
    int index = 0;

    // Apply transparency to renderers
    foreach (var renderer in renderers)
    {
      if (renderer.material.HasProperty("_Color"))
      {
        Color originalColor = originalColors[index++];
        Color newColor = originalColor;
        newColor.a = Mathf.Lerp(originalColor.a, targetTransparency, t);
        renderer.material.color = newColor;
      }
    }

    // Apply transparency to UI elements
    foreach (var uiElement in uiElements)
    {
      Color originalColor = originalColors[index++];
      Color newColor = originalColor;
      newColor.a = Mathf.Lerp(originalColor.a, targetTransparency, t);
      uiElement.color = newColor;
    }

    // Apply transparency to TextMeshPro components
    foreach (var tmp in textMeshPros)
    {
      Color originalColor = originalColors[index++];
      Color newColor = originalColor;
      newColor.a = Mathf.Lerp(originalColor.a, targetTransparency, t);
      tmp.color = newColor;
    }

    foreach (var tmpUGUI in textMeshProUGUIs)
    {
      Color originalColor = originalColors[index++];
      Color newColor = originalColor;
      newColor.a = Mathf.Lerp(originalColor.a, targetTransparency, t);
      tmpUGUI.color = newColor;
    }
  }

}
