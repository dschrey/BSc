using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSVUtils
{
    private readonly object _fileLock = new();

    private readonly Dictionary<string, FileWriteContext> _fileContexts = new();

    private class FileWriteContext
    {
        public Queue<string> Buffer { get; } = new();
        public string FileHeader { get; set; }
    }

    public void WriteDataToFile(string dataToWrite, string outputFilePath, string outputFileHeader)
    {
        lock (_fileLock)
        {
            if (!_fileContexts.TryGetValue(outputFilePath, out var context))
            {
                context = new FileWriteContext { FileHeader = outputFileHeader };
                _fileContexts[outputFilePath] = context;
            }

            try
            {
                bool fileExists = File.Exists(outputFilePath);
                using StreamWriter sw = new(outputFilePath, true);
                if (!fileExists)
                {
                    sw.WriteLine(context.FileHeader);
                }
                sw.WriteLine(dataToWrite);

            }
            catch (IOException ex)
            {
                Debug.LogWarning($"Failed to write to {outputFilePath}: {ex.Message}");
                context.Buffer.Enqueue(dataToWrite);
            }
            // Debug.Log($"Exported data to {outputFilePath}");
        }
    }

    public void FlushAllBuffers()
    {
        lock (_fileLock)
        {
            foreach (var kvp in _fileContexts)
            {
                string outputFilePath = kvp.Key;
                FileWriteContext fileContext = kvp.Value;
                // Debug.Log($"{outputFilePath}");
                try
                {
                    bool fileExists = File.Exists(outputFilePath);
                    if (!fileExists)
                    {
                        using StreamWriter sw = new(outputFilePath, false);
                        sw.WriteLine(fileContext.FileHeader);
                    }

                    while (fileContext.Buffer.Count > 0)
                    {
                        string data = fileContext.Buffer.Peek();
                        try
                        {
                            using (StreamWriter sw = new(outputFilePath, true))
                            {
                                sw.WriteLine(data);
                            }
                            fileContext.Buffer.Dequeue();
                        }
                        catch (IOException ex)
                        {
                            Debug.LogWarning($"Failed to write buffered data to {outputFilePath}: {ex.Message}");
                            break;
                        }
                        // Debug.Log($"Exported data to {outputFilePath}");
                    }
                }
                catch (IOException ex)
                {
                    Debug.LogWarning($"Failed to flush buffer for {outputFilePath}: {ex.Message}");
                }
            }
        }
    }

    public List<string> ReadDataFromFile(string inputFilePath)
    {
        lock (_fileLock)
        {
            try
            {
                if (!File.Exists(inputFilePath))
                {
                    Debug.LogWarning($"File {inputFilePath} does not exist.");
                    return new List<string>();
                }

                List<string> lines = new();
                using StreamReader sr = new(inputFilePath);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    lines.Add(line);
                }
                return lines;
            }
            catch (IOException ex)
            {
                Debug.LogWarning($"Failed to read from {inputFilePath}: {ex.Message}");
                return new List<string>();
            }
        }
    }
}