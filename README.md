# 🧠 DeepSeek API for Unity

> 💬 A clean, modular Unity integration for DeepSeek's powerful LLMs — chat, reasoning, and task automation made easy.
> 

⚠️ **Note**: This is an unofficial integration not affiliated with or endorsed by DeepSeek.

---

## ✨ Features

- ✅ Clean, reusable SDK for DeepSeek API
- 🔄 Supports both streaming and non-streaming chat completions
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
    
5. Click **Add**

### Option 2: From Unity Asset Store *(Coming Soon)*

- Search for **"DeepSeek API for Unity"**
- Import into your project

---

## 🚀 Getting Started

### 🔧 Setup

1. Go to `DeepSeek > Settings` in Unity’s top menu bar
2. Paste your **API key** into the editor window (dev-only)
3. (Optional) Create a `DeepSeekSettings` asset to store your key securely for runtime builds
4. Drag the `DeepSeekChat` prefab from the sample into your scene
5. Hit Play — and chat with DeepSeek AI in seconds 💬

---

## 🧪 Sample Scene

To test everything:

1. In **Package Manager**, under **DeepSeek API for Unity**, click **Samples**
2. Click **Import** on `DeepSeek Chat Example`
3. Open:
    
    ```csharp
    Assets/Samples/DeepSeek API for Unity/1.0.0/DeepSeek Chat Example/Scenes/DeepSeek-Chat.unity

    ```
    
4. Press Play — you're live.

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

```csharp
[SerializeField] private DeepSeekSettings config;

void Start()
{
    var api = new DeepSeekApi(config);
    var request = new ChatCompletionRequest
    {
        model = DeepSeekModel.DeepSeekChat.ToModelString(),
        messages = new List<ChatMessage>
        {
            new ChatMessage { role = "system", content = "You're a helpful assistant." },
            new ChatMessage { role = "user", content = "Tell me something cool." }
        }
    };

    var response = await api.CreateChatCompletion(request);
    Debug.Log(response.choices[0].message.content);
}

```

---

## 🛠 Advanced Usage

### 🔄 Streaming Support

Use `CreateChatCompletionStreaming` with a callback for partial updates. (Docs coming soon)

### 💬 Multiple Models

```csharp
DeepSeekModel.DeepSeekChat
DeepSeekModel.DeepSeekR1

```

---

## 🐞 Troubleshooting

**Can't add component?**

→ Make sure you dragged `DeepSeekSettings` into the prefab or scene object.

**Meta file warnings?**

→ Fixed by including `.meta` files in the UPM package (already handled ✅)

**Streaming not working?**

→ Some Unity versions may require extra coroutine-based handling. Coming soon.

---

## 💖 Support This Project

If you find **DeepSeek-Unity** useful, please consider supporting its development!

- [Become a sponsor on GitHub Sponsors](https://github.com/sponsors/yagizeraslan)
- [Buy me a coffee on Ko-fi](https://ko-fi.com/yagizeraslan)

Your support helps me continue maintaining and improving this project. Thank you! 🚀

---

## 📄 License

Unofficial integration. DeepSeek™ is a trademark of Hangzhou DeepSeek Artificial Intelligence Co., Ltd.

MIT License.

---

## 🤝 Contact & Support

**Author**: [Yağız ERASLAN](https://www.linkedin.com/in/yagizeraslan/)

📬 yagizeraslan@gmail.com

💬 GitHub Issues welcome!
