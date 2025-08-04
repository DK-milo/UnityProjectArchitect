# Unity Project Architect - Hybrid DLL Package

**Version 0.2.0** - Professional Development Architecture

An AI-powered Unity Editor package for project documentation and organization, featuring a hybrid DLL architecture for professional development workflow.

## 🏗️ Hybrid Architecture

This package uses a **hybrid DLL + Unity package structure** for optimal development and distribution:

- **Core Business Logic**: Compiled to DLLs for fast development and IP protection
- **Unity Integration**: Native Unity Editor windows and ScriptableObject wrappers
- **Professional Development**: Unit testing, fast compilation, and CI/CD ready

## 📦 Package Contents

```
Runtime/
├── Plugins/
│   └── UnityProjectArchitect.Core.dll    # Core business logic
└── Unity/
    ├── UnityProjectDataAsset.cs           # Unity ScriptableObject wrapper
    └── UnityServiceBridge.cs              # Bridge to DLL services

Editor/
└── Windows/
    └── ProjectArchitectWindow.cs          # Main Unity Editor window

Tests/
└── Runtime/                               # Unity integration tests
```

## 🚀 Quick Start

1. **Install Package**: Import into Unity via Package Manager
2. **Open Window**: `Window > Unity Project Architect`
3. **Create Project Data**: Click "Create New Project Data Asset"
4. **Configure Project**: Set project type and enable documentation sections
5. **Generate Documentation**: Use AI-powered generation or manual editing
6. **Export**: Export to Markdown, PDF, or Unity assets

## ✨ Features

### 📖 **Documentation Generation**
- 6 standard documentation sections
- AI-powered content generation (Claude API)
- Multiple export formats (Markdown, PDF, Unity assets)
- Progress tracking and validation

### 🔍 **Project Analysis**  
- Comprehensive project structure analysis
- Script and asset dependency analysis
- Architecture pattern detection
- Performance insights and recommendations

### 🏗️ **Project Organization**
- Smart folder structure templates
- Project type-specific configurations
- Team collaboration standards
- Automated organization workflows

## 🧩 DLL Integration

The package integrates with compiled DLLs containing the core business logic:

- **UnityProjectArchitect.Core.dll**: Models, interfaces, core abstractions
- **UnityProjectArchitect.Services.dll**: Analysis, generation, export services  
- **UnityProjectArchitect.AI.dll**: Claude API integration and AI services

This architecture provides:
- ⚡ **Fast Development**: 2-3 second compilation vs Unity's 30+ seconds
- 🧪 **Professional Testing**: Full unit test coverage with mocking
- 🔒 **IP Protection**: Core logic compiled and obfuscated
- 🎯 **Unity Native**: Seamless Unity Editor integration

## 📋 Requirements

- **Unity Version**: 2023.3+ (optimized for Unity 6)
- **Dependencies**: Newtonsoft JSON (automatically imported)
- **Platform**: Windows, macOS, Linux
- **Optional**: Claude API key for AI features

## 🔧 Development

For developers extending this package:

1. **Core Logic**: Modify C# solution in `src/` directory
2. **Unity Integration**: Edit Unity-specific code in package
3. **Testing**: Use both .NET unit tests and Unity integration tests
4. **Building**: Run `dotnet build` then copy DLLs to package

## 🤝 Support

- **Documentation**: [Full documentation site](https://docs.unityprojectarchitect.com)
- **Issues**: [GitHub Issues](https://github.com/your-repo/unity-project-architect/issues)  
- **Community**: [Discord Server](https://discord.gg/unity-project-architect)

## 📄 License

MIT License - See LICENSE file for details.

---

**Transform your Unity development with professional documentation and AI-powered project management.** 🚀