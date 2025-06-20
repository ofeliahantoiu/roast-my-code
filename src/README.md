# RoastMyCode 

A Windows desktop application that uses AI to roast your code with dark and smart humor. Capture your device camera, upload files, or type your code directly to get entertaining feedback from an AI assistant.

## Features 

- **AI-Powered Code Roasting**: Get humorous and witty feedback on your code
- **Camera Integration**: Capture screenshots of your camera directly from the app
- **File Upload**: Upload code files for roasting
- **Text-to-Speech**: Have the AI responses read aloud
- **Dark/Light Theme**: Toggle between themes for comfortable coding
- **Conversation History**: Save and download your roasting sessions
- **Multiple Roast Levels**: Choose from light to savage roasting intensity
- **Image Display**: View captured photos directly in chat bubbles

## Prerequisites 

### Required Software
- **.NET 6.0 Runtime** (for running pre-built executable)
- **.NET 6.0 SDK** (only if building from source)
- **Windows 10/11** (Windows Forms application)

### Optional but Recommended
- **Webcam/Camera** for screenshot capture functionality
- **Speakers/Headphones** for text-to-speech output

## Installation and Running


### Method 1: Clone and Build 

1. **Clone the repository**
   ```bash
   git clone https://github.com/ourRepository/roast-my-code.git
   cd roast-my-code/src
   ```

2. **Navigate to the project directory**
   ```bash
   cd RoastMyCode
   ```

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the project**
   ```bash
   dotnet build
   ```

5. **Run the project**
   ```bash
   dotnet run
   ```
 
### Method 2: Visual Studio

1. **Open Visual Studio 2022** (Community edition is free)
2. **Open the solution file**: `RoastMyCode.sln`
3. **Restore NuGet packages** (Visual Studio should do this automatically)
4. **Build the solution** (Ctrl+Shift+B)
5. **Press F5** or click the "Start" button

## Configuration 

### API Keys Setup

The application requires an OpenAI API key for AI services. You need to update the `appsettings.json` file in the project root:

1. **There is an `appsettings.json`** in the `src` directory 
2. **Add our OpenAI API key**:

```json
{
  "OpenAI": {
    "ApiKey": "openai-api-key-here",
    "Model": "gpt-3.5-turbo"
  }
}
```

**Important Notes:**
- Replace `openai-api-key-here` with our actual OpenAI API key
- Get our API key from api_key.txt
- The `appsettings.json` file is already in `.gitignore` to protect our API key
- If you encounter any error with the API key please ensure that the key is also present here `src\RoastMyCode\bin\Debug\net6.0-windows\appsettings.json`

### Dependencies

The application uses the following key dependencies:
- **OpenCvSharp4**: For camera capture functionality
- **NAudio**: For audio processing
- **System.Speech**: For text-to-speech
- **AvalonEdit**: For code editing features
- **Microsoft.Extensions.Configuration**: For configuration management

All dependencies are automatically restored when building the project.

## Usage Guide 📖

### Basic Usage

1. **Launch the application**
2. **Type your code** in the input area or use one of the input methods below
3. **Select roast level** (Light, Medium, Savage)
4. **Click Send** or press Enter
5. **Enjoy the AI's humorous feedback!**

### Input Methods

#### Text Input
- Type or paste your code directly into the input box
- Supports all programming languages

#### Camera Capture
- Click the camera icon
- Select your camera from the dropdown
- Click "Start Camera" to begin preview
- Click "Capture" to take a screenshot
- Click "Send to Chat" to roast the captured image

#### File Upload
- Click the upload icon
- Select code files or ZIP archives
- Supported formats: `.txt`, `.cs`, `.js`, `.py`, `.java`, `.cpp`, `.html`, `.css`, etc.
- ZIP files are automatically extracted and processed

### Features

#### Theme Toggle
- Click the theme icon to switch between dark and light modes

#### Text-to-Speech
- Select voice type (Male/Female)
- AI last response will be read aloud automatically

#### Conversation History
- All conversations are saved automatically
- Click "Download Conversation" to save as text file

#### Roast Levels
- **Light**: Gentle, constructive feedback
- **Medium**: Balanced humor and criticism
- **Savage**: Brutal, hilarious roasting


**Happy Coding and Roasting!**  