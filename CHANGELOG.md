# 📋 Changelog

All notable changes to **DeepSeek API for Unity** will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

---

## [1.1.0] - 2025-08-24

### 🚀 Added
- **Advanced Memory Management System**: Prevents memory bloat in long conversations
  - Chat history automatic trimming (50 message limit, configurable)
  - UI GameObject management with automatic cleanup (100 UI element limit, configurable)
  - Controller lifecycle optimization to prevent memory leaks
- New public methods for memory control:
  - `DeepSeekChatController.ClearHistory()` - Manual history cleanup
  - `DeepSeekChatController.GetHistoryCount()` - Monitor conversation size
  - `DeepSeekChat.ClearChat()` - Complete chat reset (history + UI)

### 🛠 Improved  
- **Performance**: Eliminated controller recreation on every message send
- **Resource Management**: Single controller instance reused throughout session
- **UI Performance**: Bounded GameObject hierarchy prevents performance degradation
- **Memory Footprint**: Automatic cleanup reduces garbage collection pressure

### 🔧 Technical Changes
- Added `TrimHistoryIfNeeded()` method with configurable limits
- Added `TrimUIMessagesIfNeeded()` method for UI GameObject management  
- Enhanced `OnDestroy()` cleanup for proper event listener disposal
- Added tracking list for instantiated message GameObjects

---

## [1.0.0] - 2025-04-26

### 🎉 Added
- First public release of unofficial DeepSeek API for Unity
- Support for Unity 2020.3, 2021, 2022, 2023, and 6.0+
- UPM (Unity Package Manager) Git installation support
- Support for multiple DeepSeek models (DeepSeek-V3, DeepSeek-R1)
- Native DeepSeek streaming (stream: true) API support
- Setup Wizard for automatic UniTask installation for better streaming feature
- Runtime-safe API Key storage with ScriptableObject
- Modular, reusable UI Chat components
- Sample Scene, prefabs, and demo UI included

### 🛠 Fixed
- Meta file missing errors in UPM package structure
- Temporary compile errors after first install (with better Setup Wizard flow)

---
