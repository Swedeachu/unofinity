using UnityEngine;
using ImGuiNET;
using System.Collections.Generic;

public class ActionDisplayGUI : MonoBehaviour
{

  [Header("GUI Settings")]
  public bool showGUI = false; // Toggle for the GUI (editable default in the Inspector)
  public float windowWidth = 400f; // Preset window width
  public float windowHeight = 600f; // Preset window height

  private ActionBatchManager actionBatchManager;
  private ActionRunner actionRunner;

  private void Start()
  {
    // Locate the GameManager in the scene
    var gameManager = FindObjectOfType<GameManager>();

    // Use reflection to access private fields
    if (gameManager != null)
    {
      actionBatchManager = gameManager.GetType()
          .GetField("actionBatchManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
          ?.GetValue(gameManager) as ActionBatchManager;

      actionRunner = gameManager.GetType()
          .GetField("actionRunner", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
          ?.GetValue(gameManager) as ActionRunner;
    }
    else
    {
      Debug.LogError("GameManager not found in the scene!");
    }
  }

  private void Update()
  {
    // Toggle GUI with the D key
    if (Input.GetKeyDown(KeyCode.D))
    {
      showGUI = !showGUI;
    }
  }

  private void OnEnable()
  {
    ImGuiUn.Layout += OnLayout;
  }

  private void OnDisable()
  {
    ImGuiUn.Layout -= OnLayout;
  }

  private void OnLayout()
  {
    if (!showGUI) return;

    // Set the window size using public fields
    ImGui.SetNextWindowSize(new Vector2(windowWidth, windowHeight), ImGuiCond.FirstUseEver);

    // Begin the GUI window
    ImGui.Begin("Action List", ImGuiWindowFlags.NoCollapse);

    if (actionBatchManager == null || actionRunner == null)
    {
      ImGui.TextColored(new Vector4(1f, 0f, 0f, 1f), "GameManager or action components not found!");
      ImGui.End();
      return;
    }

    // Static section for Action Batch Manager
    ImGui.TextColored(new Vector4(0f, 1f, 1f, 1f), "Action Batch Manager");
    DisplayActionBatchManager();

    // Separator between sections
    ImGui.Separator();

    // Static section for Action Runner
    ImGui.TextColored(new Vector4(0f, 1f, 0f, 1f), "Action Runner");
    DisplayActionRunner();

    ImGui.End();
  }

  private void DisplayActionBatchManager()
  {
    Queue<List<IAction>> batches = actionBatchManager.GetBatches();

    if (batches.Count == 0)
    {
      ImGui.TextColored(new Vector4(1f, 1f, 0f, 1f), "No batches in the queue.");
      return;
    }

    int batchIndex = 0;
    foreach (var batch in batches)
    {
      // Add a section label for each batch
      ImGui.Text($"Batch {batchIndex}");

      int actionIndex = 1;
      foreach (var action in batch)
      {
        ImGui.BulletText($"{actionIndex}. {action.GetType().Name}");
        actionIndex++;
      }
      batchIndex++;

      // Add spacing between batches
      ImGui.Spacing();
    }
  }

  private void DisplayActionRunner()
  {
    List<IAction> activeActions = actionRunner.GetActiveActions();

    if (activeActions.Count == 0)
    {
      ImGui.TextColored(new Vector4(1f, 1f, 0f, 1f), "No active actions.");
      return;
    }

    int actionIndex = 1;
    foreach (var action in activeActions)
    {
      ImGui.BulletText($"{actionIndex}. {action.GetType().Name}");
      actionIndex++;
    }
  }

}
