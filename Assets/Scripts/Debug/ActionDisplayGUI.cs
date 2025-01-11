using UnityEngine;
using ImGuiNET;
using System.Collections.Generic;

public class ActionDisplayGUI : MonoBehaviour
{

  private bool showGUI = false; // Toggle for the GUI
  private ActionBatchManager actionBatchManager;
  private ActionRunner actionRunner;

  private void Start()
  {
    // Scan the scene for the GameManager object
    var gameManager = FindObjectOfType<GameManager>();

    // bonkers hack to use reflection to force strip the field's address off the class instance
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

    // Begin the GUI window
    ImGui.Begin("Action Display");

    if (actionBatchManager == null || actionRunner == null)
    {
      ImGui.TextColored(new Vector4(1f, 0f, 0f, 1f), "GameManager or action components not found!");
      ImGui.End();
      return;
    }

    // Display Action Batch Manager Info
    ImGui.Text("Action Batch Manager:");
    DisplayActionBatchManager();

    ImGui.Text("\n");

    // Display Action Runner Info
    ImGui.Text("Action Runner:");
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
      ImGui.TextColored(new Vector4(0f, 1f, 1f, 1f), $"Batch {batchIndex}");
      foreach (var action in batch)
      {
        ImGui.Text($"- {action.GetType().Name}");
      }
      batchIndex++;
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

    foreach (var action in activeActions)
    {
      ImGui.TextColored(new Vector4(0f, 1f, 0f, 1f), $"{action.GetType().Name} - In Progress");
    }
  }

}
