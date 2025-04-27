# DeepSeek API for Unity

> A clean, modular Unity integration for DeepSeek's powerful LLMs — chat, reasoning, and task automation made easy.

**Note**: This is an unofficial integration not affiliated with or endorsed by DeepSeek.

---

## ✨ Features

- Clean, reusable SDK for DeepSeek API
- Supports both streaming and non-streaming chat completions
- Compatible with multiple models (DeepSeek Chat, Reasoner)
- Modular & customizable UI chat component
- Secure API key storage (runtime-safe)
- Built with Unity Package Manager (UPM)
- Includes sample scene & prefabs
- Automatic dependency installer (UniTask Setup Wizard)

---

## 💎 Supported Platforms & Unity Versions

| Platform | Unity 2020.3 | Unity 2021 | Unity 2022 | Unity 6 | Notes |
|:---|:---|:---|:---|:---|
| **Windows** | ✅ | ✅ | ✅ | ✅ | Fully supported (tested with IL2CPP & Mono) |
| **Android** | ✅ | ✅ | ✅ | ✅ | Internet permission required |
| **WebGL** | ⚠️ Partial | ⚠️ Partial | ✅ | ✅ | Streaming unsupported; CORS config needed |
| **Linux** | ❓ | ❓ | ❓ | ❓ | Not officially tested yet |
| **macOS** | ❓ | ❓ | ❓ | ❓ | Not officially tested yet |
| **iOS** | ❓ | ❓ | ❓ | ❓ | HTTPS required; not yet tested |
| **Consoles** | ❌ | ❌ | ❌ | ❌ | Not supported |

> ❓ = Expected to work, but not tested yet  
> ⚠️ = Partial support (limitations)

---

## 🛠️ Requirements

- Unity 2020.3 LTS or newer
- TextMeshPro package (via Package Manager)
- DeepSeek API Key from [platform.deepseek.com](https://platform.deepseek.com/)
- Internet access at runtime

---

## 📦 Installation

### Install via Git URL

1. Open your Unity project
2. Go to **Window > Package Manager**
3. Click `+` → **Add package from Git URL**
4. Paste:

   ```
   https://github.com/yagizeraslan/DeepSeek-Unity.git
   ```

5. Click **Add**
6. After install, a **Setup Wizard** will pop up automatically.

---

## 🔧 DeepSeek Setup Wizard

- If **UniTask** is not installed yet, the wizard prompts you to install it.
- Install UniTask with one click.
- Automatic domain reload ensures full streaming support.
- No manual steps required after setup!

You can re-open the wizard anytime from:
**DeepSeek → Setup Wizard**

---

## 🚀 Getting Started

1. Go to **DeepSeek > Settings** from Unity’s top menu
2. Paste your **DeepSeek API key**
3. (Optional) Create a `DeepSeekSettings` asset for secure runtime builds
4. Drag the **DeepSeekChat** prefab from the imported sample scene
5. Hit **Play** — you’re chatting with DeepSeek AI in seconds 💬

---

## 🧪 Sample Scene

1. Open **Package Manager**
2. Under **DeepSeek API for Unity**, find **Samples**
3. Click **Import** on `DeepSeek Chat Example`
4. Open:

   ```
   Assets/Samples/DeepSeek API for Unity/1.0.1/DeepSeek Chat Example/Scenes/DeepSeek-Chat.unity
   ```

5. Play and chat!

---

## 🔐 API Key Handling

| Environment | How it works |
|:---|:---|
| Editor (development) | Key stored securely via EditorPrefs |
| Runtime (builds) | Use `DeepSeekSettings` ScriptableObject |

No hardcoded keys.  
Asset Store safe.

---

## 🛠️ Architecture Overview

| Layer | Folder | Purpose |
|:---|:---|:---|
| API Logic | `Runtime/API/` | DeepSeek HTTP handlers |
| Data Models | `Runtime/Data/` | Requests, Responses (DTOs) |
| UI Logic | `Runtime/UI/` | Chat Controller & Prefabs |
| Common | `Runtime/Common/` | Config, runtime settings |
| Editor | `Editor/` | Setup Wizard, Settings windows |
| Samples | `Samples~/` | Example scenes and prefabs |

---

## 📂 Example Usage

```csharp
[SerializeField] private DeepSeekSettings config;

private async UniTaskVoid Start()
{
    var api = new DeepSeekApi(config);
    
    var request = new ChatCompletionRequest
    {
        model = DeepSeekModel.DeepSeekChat.ToModelString(),
        messages = new List<ChatMessage>
        {
            new ChatMessage { role = "system", content = "You are a helpful assistant." },
            new ChatMessage { role = "user", content = "Hello!" }
        }
    };

    var response = await api.CreateChatCompletion(request);
    Debug.Log(response.choices[0].message.content);
}
```

---

## 🔄 Streaming Chat (Optional)

Enable **streaming** mode easily from Inspector:

- Toggle `Use Streaming` option on `DeepSeekChat` component.
- See messages appear token-by-token during generation.

Built on UniTask for maximum async performance.

---

## 🛡️ Troubleshooting

| Issue | Solution |
|:--|:--|
| Meta file warnings | Already handled inside UPM package. |
| Can't add DeepSeekChat to scene | Make sure `DeepSeekSettings` is assigned. |
| Streaming doesn't work | Ensure UniTask is installed properly (Setup Wizard helps). |

---

## 📄 License

MIT License © Yağız ERASLAN  
DeepSeek™ is a trademark of Hangzhou DeepSeek Artificial Intelligence Co., Ltd.

This project is unofficial and not affiliated with DeepSeek.

---

## 💖 Support This Project

If you find **DeepSeek-Unity** useful, please consider supporting its development!

- [Sponsor me on GitHub](https://github.com/sponsors/yagizeraslan)
- [Buy me a coffee on Ko-fi](https://ko-fi.com/yagizeraslan)

Your support helps me continue maintaining and improving this project. Thank you! 🚀

---

## 🤝 Contact & Support

- LinkedIn: [Yağız ERASLAN](https://www.linkedin.com/in/yagizeraslan/)
- Email: yagizeraslan@gmail.com
- GitHub Issues and PRs welcome!
