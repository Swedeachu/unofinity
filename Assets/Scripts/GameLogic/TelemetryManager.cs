using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TelemetryManager : MonoBehaviour
{

  private Dictionary<string, int> valuePairs = new Dictionary<string, int>();

  private void Update()
  {
    if (Input.GetKeyUp(KeyCode.S))
    {
      Save();
      Clear();  
    }
  }

  public void Add(string key, int value = 1)
  {
    if (!GameManager.autoMode) return;

    if (valuePairs.ContainsKey(key))
    {
      valuePairs[key] += value;
      Debug.Log(key + ": " + valuePairs[key]);
    }
    else
    {
      valuePairs.Add(key, value);
      Debug.Log(key + ": " + value);
    }
  }

  // writes everything to app data in a simple csv
  private void Save()
  {
    string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    string fileName = $"telemetry_{timestamp}.csv";
    string filePath = Path.Combine(Application.persistentDataPath, fileName);

    using (StreamWriter writer = new StreamWriter(filePath))
    {
      writer.WriteLine("Key,Value"); // CSV Header
      foreach (var pair in valuePairs)
      {
        writer.WriteLine($"{pair.Key},{pair.Value}");
      }
    }

    Debug.Log($"Telemetry saved to {filePath}");
  }

  private void Clear()
  {
    valuePairs.Clear();
  }

}
