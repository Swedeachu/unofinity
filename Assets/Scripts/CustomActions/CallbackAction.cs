using System;
using System.Diagnostics;

public class CallbackAction : IAction
{

  private Action callback;
  private bool isComplete;
  private Action onComplete;

  public bool bypassPausing = false;
  public bool IsComplete => isComplete;
  public bool BypassPausing => bypassPausing;

  private readonly string origin;

  public CallbackAction(Action callback)
  {
    this.callback = callback;

    // Capture the calling method, file, and line number for more information of where the callback lambda is defined
    // This would actually be a really large performance hit in prod, this should be a debug only feature
    var stackTrace = new StackTrace(true);
    if (stackTrace.FrameCount > 1)
    {
      var frame = stackTrace.GetFrame(1); // Get the calling frame
      var method = frame.GetMethod();
      var file = frame.GetFileName();
      var line = frame.GetFileLineNumber();

      // Format origin like "TurnManager#43"
      if (!string.IsNullOrEmpty(file))
      {
        file = System.IO.Path.GetFileName(file); // Extract just the filename
        origin = $"{file}#{line}";
      }
      else
      {
        origin = method.DeclaringType?.Name ?? "Unknown";
      }
    }
    else
    {
      origin = "Unknown";
    }
  }

  public void StartAction(Action onComplete)
  {
    isComplete = false;
    this.onComplete = onComplete;
  }

  public void UpdateAction()
  {
    if (isComplete) return;

    // Immediately call the callback, then complete
    callback?.Invoke();

    isComplete = true;
    onComplete?.Invoke();
  }

  public override string ToString()
  {
    return $"CallbackAction ({origin})";
  }

}
