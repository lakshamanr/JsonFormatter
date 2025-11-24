# 🎯 JSON Formatter & Validator Pro

**A professional C# WPF desktop application for Windows** that provides comprehensive JSON formatting, validation, and conversion capabilities.

![Platform](https://img.shields.io/badge/platform-Windows-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![License](https://img.shields.io/badge/license-MIT-green)

## ✨ Features

### 🔹 Core JSON Operations
- **Format JSON** - Pretty print with customizable indentation (2 or 4 spaces)
- **Minify JSON** - Remove all whitespace for compact output
- **Validate JSON** - Real-time validation with detailed error messages showing exact line and column
- **Syntax Highlighting** - Professional code editor with color-coded JSON (powered by AvalonEdit)
- **Tree View Explorer** - Hierarchical visualization of JSON structure

### 🔹 File Management
- **Open/Save JSON Files** - Support for `.json`, `.txt`, and `.config` files
- **Recent Files** - Quick access to recently opened files
- **Auto-save Session** - Restore your last working session
- **Drag & Drop Support** - Easy file loading (coming soon)

### 🔹 Conversion Tools
- **JSON ↔ XML** - Convert between JSON and XML formats
- **JSON → C# Classes** - Generate C# POCO classes with Newtonsoft.Json attributes
  - PascalCase property names
  - Automatic type inference
  - Nested class generation
- **JSON → SQL** - Generate SQL INSERT statements from JSON data
- **JSON ↔ YAML** - Two-way conversion between JSON and YAML

### 🔹 Advanced Features
- **JSON Schema Validation** - Validate JSON against JSON Schema files
- **JSON Diff & Compare** - Compare two JSON files with semantic difference analysis
- **Clipboard Operations** - Quick copy/paste with auto-format
- **Base64 Encode/Decode** - Encode and decode Base64 strings
- **URL Encode/Decode** - URL encoding and decoding support

### 🔹 User Experience
- **Dark/Light Themes** - Toggle between professional dark and light themes (Ctrl+T)
- **Keyboard Shortcuts** - Full keyboard shortcut support for all major operations
- **Line Numbers** - Easy navigation with line numbers in the editor
- **Status Bar** - Real-time validation status and file information
- **Large File Support** - Handle JSON files up to 50MB efficiently

## 📋 Requirements

- **Operating System**: Windows 10/11
- **.NET Runtime**: .NET 8.0 or later
- **RAM**: 4GB minimum (8GB recommended for large files)
- **Disk Space**: 50MB

## 🚀 Installation

### Option 1: Build from Source

1. **Clone the repository**:
   ```bash
   git clone https://github.com/yourusername/JsonFormatter.git
   cd JsonFormatter
   ```

2. **Open in Visual Studio**:
   - Open `JsonFormatter.sln` in Visual Studio 2022 or later
   - Ensure you have the .NET 8.0 SDK installed

3. **Restore NuGet packages**:
   ```bash
   dotnet restore
   ```

4. **Build the solution**:
   ```bash
   dotnet build --configuration Release
   ```

5. **Run the application**:
   ```bash
   dotnet run --project JsonFormatterApp
   ```

### Option 2: Download Release

Download the latest release from the [Releases](https://github.com/yourusername/JsonFormatter/releases) page.

## 🎨 Screenshots

### Dark Theme
![Dark Theme](docs/screenshots/dark-theme.png)

### Light Theme
![Light Theme](docs/screenshots/light-theme.png)

### Tree View
![Tree View](docs/screenshots/tree-view.png)

## ⌨️ Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+N` | New File |
| `Ctrl+O` | Open File |
| `Ctrl+S` | Save File |
| `Ctrl+Shift+S` | Save As |
| `Ctrl+F` | Format JSON |
| `Ctrl+M` | Minify JSON |
| `Ctrl+C` | Copy to Clipboard |
| `Ctrl+V` | Paste from Clipboard |
| `Ctrl+T` | Toggle Theme |

## 🏗️ Architecture

The application follows the **MVVM (Model-View-ViewModel)** pattern for clean separation of concerns:

```
JsonFormatterApp/
├── Models/              # Data models (JsonValidationResult, JsonTreeNode, etc.)
├── ViewModels/          # MVVM ViewModels (MainViewModel)
├── Views/               # WPF Views (MainWindow)
├── Services/            # Business logic services
│   ├── JsonService.cs           # Core JSON operations
│   ├── JsonTreeService.cs       # Tree view generation
│   ├── ConversionService.cs     # Format conversions
│   ├── FileService.cs           # File I/O operations
│   ├── SchemaValidationService.cs # JSON Schema validation
│   ├── DiffService.cs           # JSON comparison
│   └── EncodingService.cs       # Encoding/Decoding
├── Helpers/             # Helper classes (RelayCommand, ViewModelBase)
├── Converters/          # WPF value converters
├── Themes/              # Dark and Light theme resources
└── Resources/           # Application resources
```

## 📦 Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| [AvalonEdit](https://github.com/icsharpcode/AvalonEdit) | 6.3.0 | Syntax highlighting editor |
| [Newtonsoft.Json](https://www.newtonsoft.com/json) | 13.0.3 | JSON parsing and manipulation |
| [NJsonSchema](https://github.com/RicoSuter/NJsonSchema) | 11.0.0 | JSON Schema validation |
| [YamlDotNet](https://github.com/aaubry/YamlDotNet) | 13.7.1 | YAML conversion |
| [DiffPlex](https://github.com/mmanela/diffplex) | 1.7.2 | Text diffing |
| [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) | 8.2.2 | MVVM helpers |

## 🛠️ Usage Examples

### Format JSON
1. Paste or type JSON into the editor
2. Click **Format** button or press `Ctrl+F`
3. JSON will be formatted with proper indentation

### Validate JSON
1. Enter JSON in the editor
2. Check the status bar for validation status
3. Error messages show exact line and column numbers

### Convert JSON to C# Classes
1. Load or paste your JSON
2. Go to **Tools → Convert to C# Classes**
3. Copy the generated classes

### Compare Two JSON Files
1. Load first JSON file
2. Go to **Tools → Compare JSON...**
3. Select second file to compare
4. View semantic differences

### JSON Schema Validation
1. Load your JSON file
2. Go to **Tools → Validate Against Schema...**
3. Select your `.schema.json` file
4. View validation results

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- [AvalonEdit](https://github.com/icsharpcode/AvalonEdit) for the excellent text editor component
- [Newtonsoft.Json](https://www.newtonsoft.com/json) for robust JSON handling
- [NJsonSchema](https://github.com/RicoSuter/NJsonSchema) for JSON Schema support
- All contributors who help improve this project

## 📧 Contact

- **GitHub Issues**: [Report a bug or request a feature](https://github.com/yourusername/JsonFormatter/issues)
- **Email**: your.email@example.com

## 🗺️ Roadmap

### Version 1.1 (Planned)
- [ ] API testing integration (REST API client)
- [ ] Plugin architecture for extensibility
- [ ] JSON Path query support
- [ ] Batch file processing
- [ ] Export to PDF/HTML
- [ ] Custom syntax highlighting themes
- [ ] Portable version (no installation required)

### Version 1.2 (Future)
- [ ] Multi-document tabs
- [ ] Search & Replace in JSON
- [ ] JSON beautification with sorting
- [ ] Integration with version control (Git)
- [ ] Cloud storage integration

## ⭐ Star History

[![Star History Chart](https://api.star-history.com/svg?repos=yourusername/JsonFormatter&type=Date)](https://star-history.com/#yourusername/JsonFormatter&Date)

---

**Made with ❤️ for developers who work with JSON daily**
