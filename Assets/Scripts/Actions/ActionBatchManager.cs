using System;
using System.Collections.Generic;

public class ActionBatchManager
{

  private Queue<List<IAction>> actionBatches = new Queue<List<IAction>>();
  private ActionRunner actionRunner;
  private bool isProcessing;

  private Action onAllBatchesComplete; // Callback for when all batches are processed

  public ActionBatchManager(ActionRunner runner)
  {
    actionRunner = runner;
  }

  public void AddBatch(List<IAction> actions)
  {
    actionBatches.Enqueue(actions);
  }

  public void StartProcessing(Action onComplete = null)
  {
    // Store the callback for when all batches are complete
    onAllBatchesComplete = onComplete;

    if (!isProcessing)
    {
      ProcessNextBatch();
    }
  }

  private void ProcessNextBatch()
  {
    if (actionBatches.Count == 0)
    {
      isProcessing = false;

      // Invoke the global callback when all batches are complete
      onAllBatchesComplete?.Invoke();
      return;
    }

    isProcessing = true;
    var currentBatch = actionBatches.Dequeue();
    actionRunner.RunActions(currentBatch);

    // Wait for all actions to complete in the batch before moving to the next
    actionRunner.RunActions(new List<IAction>
    {
      new WaitForCompletionAction(currentBatch, ProcessNextBatch)
    });
  }

  public Queue<List<IAction>> GetBatches()
  {
    return new Queue<List<IAction>>(actionBatches); // Return a copy to prevent modification
  }


}
