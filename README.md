# DeepSeek-Unity

Unity package for seamless integration with the DeepSeek-R1 API, enabling advanced reasoning, chat functionality, and task automation within Unity projects.

## Features

- **DeepSeek-R1 API Integration**: Advanced reasoning and response generation
- **Simple Setup**: Easy configuration via JSON
- **Async Requests**: Non-blocking API calls
- **Sample Scenes**: Example implementations
- **Open Source**: MIT licensed

## Getting Started

### 1. Import the Package

In Unity (2019+):
1. Window > Package Manager
2. Click + > Add package from git URL
3. Enter URL:
```
https://github.com/yourusername/DeepSeek-Unity.git
```

### 2. Configure API Key

1. Get API key from DeepSeek Platform
2. Edit `Config.json` in Resources folder:
```json
{
    "api_key": "your-api-key",
    "endpoint": "https://platform.deepseek.com/api"
}
```

### 3. Implementation

1. Add DeepSeekManager prefab to scene
2. Use DeepSeekApi and DeepSeekChat scripts for API interaction

## Example Usage

```csharp
using DeepSeekUnity;
using System.Threading.Tasks;
using UnityEngine;

public class ExampleScript : MonoBehaviour
{
    private async void Start()
    {
        var message = new DeepSeekMessage
        {
            Role = "user",
            Content = "Hello, DeepSeek!"
        };
        var response = await DeepSeekApi.Instance.SendMessageAsync(message);
        Debug.Log($"DeepSeek Response: {response.Content}");
    }
}
```

## Project Structure

```
Assets/
├── DeepSeekUnity/
│   ├── Scripts/
│   │   ├── DeepSeekApi.cs
│   │   ├── DeepSeekChat.cs
│   │   ├── DeepSeekManager.cs
│   │   ├── Models/
│   │   │   ├── RequestModels.cs
│   │   │   └── ResponseModels.cs
│   ├── Resources/
│   │   └── Config.json
│   ├── Examples/
│   │   ├── ChatExample.cs
│   │   ├── Scenes/
│   │   │   └── ChatExampleScene.unity
```

## Requirements

- Unity 2019+
- DeepSeek API key

## Troubleshooting

- **Missing API Key**: Verify Config.json has valid api_key
- **Connection Issues**: Check internet connection and API endpoint

## License

MIT License

## Contributing

Fork repository and submit pull requests with your changes.

## Contact

Name: Yağız ERASLAN
Email: yagizeraslan@gmail.com
LinkedIn: https://www.linkedin.com/in/yagizeraslan/