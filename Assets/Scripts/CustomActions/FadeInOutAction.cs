using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FadeInOutAction : IAction
{

  public bool IsComplete => isComplete;
  public bool BypassPausing => false;

  private GameObject targetObject;
  private float fadeInDuration;
  private float stayDuration;
  private float fadeOutDuration;
  private Action onComplete;

  private float elapsedTime;
  private bool isComplete;

  private Renderer[] renderers;
  private Graphic[] uiElements;
  private TextMeshPro[] textMeshPros;
  private TextMeshProUGUI[] textMeshProUGUIs;
  private Color[] originalColors;

  public FadeInOutAction(GameObject targetObject, float fadeInDuration, float stayDuration, float fadeOutDuration)
  {
    this.targetObject = targetObject;
    this.fadeInDuration = fadeInDuration /= GameManager.speed;
    this.stayDuration = stayDuration /= GameManager.speed;
    this.fadeOutDuration = fadeOutDuration /= GameManager.speed;

    // Collect all components and cache their original colors
    CollectComponents();
  }

  public void StartAction(Action onComplete)
  {
    this.onComplete = onComplete;
    elapsedTime = 0f;
    isComplete = false;

    // Set initial transparency to 0
    SetTransparency(0f);
  }

  public void UpdateAction()
  {
    if (isComplete) return;

    elapsedTime += Time.deltaTime;

    // Determine which phase we're in
    if (elapsedTime < fadeInDuration)
    {
      // Fade In phase
      float t = elapsedTime / fadeInDuration;
      SetTransparency(t);
    }
    else if (elapsedTime < fadeInDuration + stayDuration)
    {
      // Stay phase
      SetTransparency(1f);
    }
    else if (elapsedTime < fadeInDuration + stayDuration + fadeOutDuration)
    {
      // Fade Out phase
      float t = (elapsedTime - fadeInDuration - stayDuration) / fadeOutDuration;
      SetTransparency(1f - t);
    }
    else
    {
      // Action is complete
      SetTransparency(0f);
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

  private void SetTransparency(float transparency)
  {
    int index = 0;

    // Apply transparency to renderers
    foreach (var renderer in renderers)
    {
      if (renderer.material.HasProperty("_Color"))
      {
        Color originalColor = originalColors[index++];
        Color newColor = originalColor;
        newColor.a = transparency;
        renderer.material.color = newColor;
      }
    }

    // Apply transparency to UI elements
    foreach (var uiElement in uiElements)
    {
      Color originalColor = originalColors[index++];
      Color newColor = originalColor;
      newColor.a = transparency;
      uiElement.color = newColor;
    }

    // Apply transparency to TextMeshPro components
    foreach (var tmp in textMeshPros)
    {
      Color originalColor = originalColors[index++];
      Color newColor = originalColor;
      newColor.a = transparency;
      tmp.color = newColor;
    }

    foreach (var tmpUGUI in textMeshProUGUIs)
    {
      Color originalColor = originalColors[index++];
      Color newColor = originalColor;
      newColor.a = transparency;
      tmpUGUI.color = newColor;
    }
  }

}
