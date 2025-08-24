# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

DeepSeek-Unity is a Unity Package Manager (UPM) library that integrates DeepSeek's AI API into Unity projects. It provides both streaming and non-streaming chat completion functionality with a clean, modular architecture.

## Development Commands

### Unity Package Development
This project is structured as a Unity package, not a traditional Unity project:

- **Package Testing**: Import into Unity Editor via Package Manager → Add package from Git URL
- **Sample Scene**: Import samples via Package Manager → Samples → Import "DeepSeek Chat Example" 
- **Editor Tools**: Access via Unity menu `DeepSeek/Settings` for API key management

### Build Commands
No traditional build commands - Unity packages are consumed by Unity projects that handle building.

## Architecture Overview

### Core API Layer (`Runtime/API/`)
- **`IDeepSeekApi`**: Contract for API implementations
- **`DeepSeekApi`**: Standard async/await implementation using UnityWebRequest
- **`DeepSeekStreamingApi`**: Server-Sent Events (SSE) streaming implementation with custom `DownloadHandlerScript`
- **`DeepSeekModel`**: Enum/extension for model selection (deepseek-chat, deepseek-reasoner)

**Key Architecture Decision**: Two separate API classes rather than unified interface - `DeepSeekApi` for full responses, `DeepSeekStreamingApi` for real-time streaming with different callback patterns.

### Data Layer (`Runtime/Data/`)
- **`ChatCompletionRequest`**: API request DTO (model, messages, temperature, stream flag)
- **`ChatCompletionResponse`**: Standard API response DTO 
- **`ChatMessage`**: Message structure (role, content)

**Note**: Minimal API surface - only essential DeepSeek parameters implemented.

### UI Controller (`Runtime/UI/`)
- **`DeepSeekChatController`**: Business logic mediator between API and UI
- Manages conversation history as `List<ChatMessage>`
- Handles streaming state with `currentStreamContent` accumulation
- Provides error handling with user-friendly error messages in chat

### Configuration (`Runtime/Common/`)
- **`DeepSeekSettings`**: ScriptableObject for API key storage
- **Security Note**: API key stored as plain text in ScriptableObject - visible in builds

### Editor Tools (`Editor/`)
- **`DeepSeekSettingsEditor`**: EditorWindow for development API key management via EditorPrefs
- Accessible via `DeepSeek/Settings` menu

### Sample Implementation (`Samples~/DeepSeek-Chat/`)
- **`DeepSeekChat`**: MonoBehaviour demonstrating complete integration
- Instantiates message prefabs, handles UI updates, manages streaming text updates
- **Critical Pattern**: Creates new `DeepSeekChatController` instance per message (not persistent)

## Key Implementation Patterns

### Async/Await with Unity
Uses `UnityWebRequestAwaiter` extension method to make UnityWebRequest awaitable:
```csharp
await www.SendWebRequest();
```

### Streaming Architecture
- Custom `StreamingDownloadHandler : DownloadHandlerScript` 
- Parses SSE data chunks in `ReceiveData()` method
- Handles incomplete lines with StringBuilder buffer
- Graceful JSON parsing with warnings (not errors) for partial chunks

### Error Handling Strategy
- Network/API errors displayed directly in chat interface as "❌ Error: [message]"
- Streaming errors append to current content rather than replacing
- All errors added to conversation history for context preservation

### UI Update Pattern
- `AddFullMessageToUI()` for complete messages (user input, non-streaming responses)
- `AppendStreamingCharacter()` for real-time streaming updates to `activeStreamingText`
- Forces layout rebuild after each message: `LayoutRebuilder.ForceRebuildLayoutImmediate()`

## Important Technical Constraints

### Unity Package Manager Requirements
- Package structure with `package.json` manifest
- Runtime code in `Runtime/` folder with `.asmdef`
- Samples in `Samples~/` (tilde prevents inclusion in package)
- Editor code in `Editor/` folder

### Platform Limitations
- **WebGL**: Streaming may not work due to browser limitations with DownloadHandlerScript
- **iOS/Android**: HTTPS required for API calls
- All platforms: Requires internet permission in build settings

### API Key Security Issue
Current implementation stores API keys in ScriptableObject, making them visible in builds. Consider this when working on security improvements.

### Streaming Implementation Details
- Uses `stream: true` in ChatCompletionRequest for DeepSeek SSE endpoint
- Parses "data: " prefixed lines from SSE stream  
- Handles "[DONE]" termination signal
- Accumulates partial content in controller, not in streaming handler

## Testing & Validation

- Use Unity Package Manager to import into test project
- Import sample scene from Package Manager
- Configure API key in sample DeepSeekSettings.asset
- Test both streaming and non-streaming modes
- Verify cross-platform compatibility for target deployment platforms

## Git Commit Instructions
- Do NOT add Claude attribution in commit messages
- Do NOT include "Co-Authored-By: Claude" lines in commits  
- Use standard commit messages without co-author tags
- Only commit when explicitly requested by user
- Keep commit messages clean and professional without AI attribution