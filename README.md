<p align="center">
  <!-- Top small clean badges -->
  <a href="https://unity.com/releases/editor/whats-new/2020.3.0">
    <img alt="Unity 2020.3+" src="https://img.shields.io/badge/Unity-2020.3%2B-green?logo=unity&logoColor=white">
  </a>
  <img alt="UPM Compatible" src="https://img.shields.io/badge/UPM-Compatible-brightgreen">
  <a href="https://opensource.org/licenses/MIT">
    <img alt="License: MIT" src="https://img.shields.io/badge/License-MIT-blue.svg">
  </a>
  <a href="https://github.com/sponsors/yagizeraslan">
    <img alt="Sponsor" src="https://img.shields.io/badge/Sponsor-❤-ff69b4">
  </a>
  <a href="https://github.com/yagizeraslan/DeepSeek-Unity/commits/main">
    <img alt="Last Commit" src="https://img.shields.io/github/last-commit/yagizeraslan/DeepSeek-Unity">
  </a>
  <a href="https://github.com/yagizeraslan/DeepSeek-Unity">
    <img alt="Code Size" src="https://img.shields.io/github/languages/code-size/yagizeraslan/DeepSeek-Unity">
  </a>
  <br/>
  
  <!-- Bottom bigger social + platform badges -->
  <img alt="Maintenance" src="https://img.shields.io/badge/Maintained-Yes-brightgreen">
  <a href="https://github.com/yagizeraslan/DeepSeek-Unity/stargazers">
    <img alt="GitHub stars" src="https://img.shields.io/github/stars/yagizeraslan/DeepSeek-Unity?style=social">
  </a>
  <a href="https://github.com/yagizeraslan/DeepSeek-Unity/network/members">
    <img alt="GitHub forks" src="https://img.shields.io/github/forks/yagizeraslan/DeepSeek-Unity?style=social">
  </a>
  <a href="https://github.com/yagizeraslan/DeepSeek-Unity/issues">
    <img alt="GitHub issues" src="https://img.shields.io/github/issues/yagizeraslan/DeepSeek-Unity?style=social">
  </a>
  <img alt="Windows" src="https://img.shields.io/badge/Platform-Windows-blue">
  <img alt="WebGL" src="https://img.shields.io/badge/Platform-WebGL-orange">
  <img alt="Android" src="https://img.shields.io/badge/Platform-Android-green">
</p>

# 🐳 DeepSeek API for Unity

> 💬 A clean, modular Unity integration for DeepSeek's powerful LLMs — chat, reasoning, and task automation made easy.
> 

⚠️ **Note**: This is an unofficial integration not affiliated with or endorsed by DeepSeek.

---

## ✨ Features

- ✅ Clean, reusable SDK for DeepSeek API
- 🔄 Supports true SSE-based streaming and non-streaming chat completions
- 🛡️ Robust error handling and automatic resource management
- 🧠 Compatible with multiple models (DeepSeek Chat, Reasoner)
- 🎨 Modular & customizable UI chat component
- 🔐 Secure API key storage (runtime-safe)
- ⚙️ Built with Unity Package Manager (UPM)
- 🧪 Includes sample scene & prefabs

---

### 🧩 Supported Platforms & Unity Versions

| Platform | Unity 2020.3 | Unity 2021 | Unity 2022 | Unity 6 | Notes |
| --- | --- | --- | --- | --- | --- |
| **Windows** | ✅ | ✅ | ✅ | ✅ | Fully supported (tested with IL2CPP & Mono) |
| **Android** | ✅ | ✅ | ✅ | ✅ | Requires internet permission in manifest |
| **WebGL** | ⚠️ *Partial* | ⚠️ *Partial* | ✅ | ✅ | Streaming unsupported; add CORS headers on server |
| **Linux** | ❓ | ❓ | ❓ | ❓ | Likely works, but not yet tested |
| **macOS** | ❓ | ❓ | ❓ | ❓ | Not tested, expected to work |
| **iOS** | ❓ | ❓ | ❓ | ❓ | Not tested, expected to work (HTTPS required) |
| **Consoles** | ❌ | ❌ | ❌ | ❌ | Not supported (Unity license + network limitations) |

> ❓ = Not tested yet — expected to work but needs verification
> 
> 
> ⚠️ = Partial support (some limitations)
>

---

## 🧰 Requirements

- Unity 2020.3 LTS or newer
- TextMeshPro (via Package Manager)
- DeepSeek API Key from [platform.deepseek.com](https://platform.deepseek.com/)

---

## 📦 Installation

### Option 1: Via Git URL (Unity Package Manager)

1. Open your Unity project
2. Go to **Window > Package Manager**
3. Click `+` → **Add package from Git URL**
4. Paste:
    
    ```csharp
    https://github.com/yagizeraslan/DeepSeek-Unity.git
    
    ```
    
5. ✅ Done

---

## 🚀 Getting Started

### 🔧 Setup

1. After installation, download Sample scene from Package Manager
2. Paste your **API key** into the DeepSeekSettings.asset.
3. Hit Play — and chat with DeepSeek AI in seconds 💬

---

## 🧪 Sample Scene

To test everything:

1. In **Package Manager**, under **DeepSeek API for Unity**, click **Samples**
2. Click **Import** on `DeepSeek Chat Example`
3. Open:
    
    ```csharp
    Assets/Samples/DeepSeek API for Unity/1.0.1/DeepSeek Chat Example/Scenes/DeepSeek-Chat.unity

    ```
    
4. Press Play — you're live.

- You can change model type and streaming mode during play — the SDK picks up changes automatically for each new message.
- You can also press **Enter** instead of clicking Send button — handy for fast testing.

---

## 🔐 API Key Handling

- During dev: Store key via `EditorPrefs` using the DeepSeek Editor Window
- In production builds: Use the `DeepSeekSettings` ScriptableObject (recommended)

**DO NOT** hardcode your key in scripts or prefabs — Unity will reject the package.

---

## 🧱 Architecture Overview

| Layer | Folder | Role |
| --- | --- | --- |
| API Logic | `Runtime/API/` | HTTP & model logic |
| Data Models | `Runtime/Data/` | DTOs for requests/responses |
| UI Component | `Runtime/UI/` | MonoBehaviour & Controller |
| Config Logic | `Runtime/Common/` | Secure key storage |
| Editor Tools | `Editor/` | Editor-only settings UI |
| Example Scene | `Samples~/` | Demo prefab, scene, assets |

---

## 🧩 Example Integration

### 🕐 Non-Streaming (Full Response)

```csharp
[SerializeField] private DeepSeekSettings config;

private async void Start()
{
    var api = new DeepSeekApi(config);
    var request = new ChatCompletionRequest
    {
        model = DeepSeekModel.DeepSeek_V3.ToModelString(),
        messages = new ChatMessage[]
        {
            new ChatMessage { role = "system", content = "You're a helpful assistant." },
            new ChatMessage { role = "user", content = "Tell me something cool." }
        }
    };

    var response = await api.CreateChatCompletion(request);
    Debug.Log("[FULL RESPONSE] " + response.choices[0].message.content);
}

```

### 🔄 Streaming (Real-Time Updates)
```csharp
[SerializeField] private DeepSeekSettings config;

private void Start()
{
    RunStreamingExample();
}

private void RunStreamingExample()
{
    var request = new ChatCompletionRequest
    {
        model = DeepSeekModel.DeepSeek_V3.ToModelString(),
        messages = new ChatMessage[]
        {
            new ChatMessage { role = "user", content = "Stream a fun fact about the ocean." }
        },
        stream = true
    };

    var streamingApi = new DeepSeekStreamingApi();
    streamingApi.CreateChatCompletionStream(
        request,
        config.apiKey,
        partial =>
        {
            Debug.Log("[STREAMING] " + partial); // Called for each streamed segment
        }
    );
}

```

---

## 🛠 Advanced Usage

### 🔄 Streaming Support

DeepSeek-Unity supports **reliable real-time streaming** using DeepSeek's official `stream: true` Server-Sent Events (SSE) endpoint.

✅ Uses Unity's `DownloadHandlerScript` for chunked response handling  
✅ UI updates per-token (no simulated typewriter effect)  
✅ Automatic resource cleanup and memory management
✅ Built-in error handling with user-friendly messages
✅ Request timeout protection (60s default)
✅ No coroutines, no external libraries — works natively in Unity

To enable:
- Check `Use Streaming` in the chat prefab or component
- Partial responses will automatically stream into the UI

📌 You can toggle streaming on/off at runtime.
### 💬 Multiple Models

```csharp
DeepSeekModel.DeepSeek_V3
DeepSeekModel.DeepSeek_R1

```

---

## 🐞 Troubleshooting

**Can't add component?**

→ Make sure you dragged `DeepSeekSettings.asset` into the DeepSeekChat.cs's Config field.

**Streaming not working?**

→ Make sure you're on a platform that supports `DownloadHandlerScript` (Standalone or Editor).  
→ WebGL and iOS may have platform limitations for live SSE streams.

**Getting error messages in the chat?**

→ Error messages now display directly in the chat interface for better debugging.  
→ Check Unity Console for detailed technical error information.

**Seeing JSON parse warnings in streaming mode?**  

→ These are normal during SSE — they occur when the parser receives partial chunks. They're automatically skipped and won't affect the final output.

---

## 💖 Support This Project

If you find **DeepSeek-Unity** useful, please consider supporting its development!

- [Become a sponsor on GitHub Sponsors](https://github.com/sponsors/yagizeraslan)
- [Buy me a coffee on Ko-fi](https://ko-fi.com/yagizeraslan)

Your support helps me continue maintaining and improving this project. Thank you! 🚀

---

## 📄 License

Unofficial integration. DeepSeek™ is a trademark of Hangzhou DeepSeek Artificial Intelligence Co., Ltd.

This project is licensed under the MIT License.

---

## 🤝 Contact & Support

**Author**: [Yağız ERASLAN](https://www.linkedin.com/in/yagizeraslan/)

📬 yagizeraslan@gmail.com

💬 GitHub Issues welcome!
