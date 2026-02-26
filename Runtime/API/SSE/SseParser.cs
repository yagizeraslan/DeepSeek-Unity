using System;
using System.Text;
using UnityEngine;

namespace YagizEraslan.DeepSeek.Unity
{
    public class SseParser
    {
        private readonly StringBuilder buffer = new StringBuilder();
        private readonly Action<string> onData;
        private readonly Action onComplete;
        private readonly Action<string> onError;

        public SseParser(Action<string> onData, Action onComplete, Action<string> onError = null)
        {
            this.onData = onData ?? throw new ArgumentNullException(nameof(onData));
            this.onComplete = onComplete ?? throw new ArgumentNullException(nameof(onComplete));
            this.onError = onError;
        }

        public void ProcessChunk(string chunk)
        {
            if (string.IsNullOrEmpty(chunk)) return;

            buffer.Append(chunk);
            string bufferContent = buffer.ToString();
            string[] lines = bufferContent.Split('\n');

            if (!bufferContent.EndsWith("\n"))
            {
                buffer.Clear();
                buffer.Append(lines[lines.Length - 1]);
                for (int i = 0; i < lines.Length - 1; i++)
                    ProcessLine(lines[i]);
            }
            else
            {
                buffer.Clear();
                foreach (string line in lines)
                    ProcessLine(line);
            }
        }

        private void ProcessLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return;

            if (line.StartsWith(ApiConstants.SSE_DATA_PREFIX))
            {
                string jsonData = line.Substring(ApiConstants.SSE_DATA_PREFIX.Length).Trim();

                if (jsonData == ApiConstants.SSE_DONE_MARKER)
                {
                    onComplete?.Invoke();
                    return;
                }

                try
                {
                    StreamingResponse response = JsonUtility.FromJson<StreamingResponse>(jsonData);
                    if (response?.choices != null && response.choices.Length > 0)
                    {
                        string content = response.choices[0].delta?.content;
                        if (!string.IsNullOrEmpty(content))
                            onData?.Invoke(content);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"{ApiConstants.LOG_PREFIX_STREAMING} Failed to parse stream chunk: {e.Message}");
                }
            }
        }

        public void ProcessRemainingBuffer()
        {
            if (buffer.Length > 0)
            {
                ProcessLine(buffer.ToString());
            }
        }

        public void Reset()
        {
            buffer.Clear();
        }

        [Serializable]
        private class StreamingResponse
        {
            public string id;
            public StreamingChoice[] choices;
        }

        [Serializable]
        private class StreamingChoice
        {
            public StreamingDelta delta;
        }

        [Serializable]
        private class StreamingDelta
        {
            public string content;
        }
    }
}
