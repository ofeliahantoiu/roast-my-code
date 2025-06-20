# RoastMyCode 🍖

A Windows desktop application that uses AI to roast your code with dark and smart humor. Capture your device camera, upload files, or type your code directly to get entertaining feedback from an AI assistant.

## Features 

- **AI-Powered Code Roasting**: Get humorous and witty feedback on your code
- **Camera Integration**: Capture screenshots of you camera directly from the app
- **File Upload**: Upload code files for roasting
- **Text-to-Speech**: Have the AI responses read aloud
- **Dark/Light Theme**: Toggle between themes for comfortable coding
- **Conversation History**: Save and download your roasting sessions
- **Multiple Roast Levels**: Choose from light to savage roasting intensity
- **Image Display**: View captured photos directly in chat bubbles

## Prerequisites 

### Required Software
- **.NET 6.0 SDK** or later
- **Windows 10/11** (Windows Forms application)
- **Visual Studio 2022** 

### Optional but Recommended
- **Webcam/Camera** for screenshot capture functionality
- **Speakers/Headphones** for text-to-speech output

## Installation and Running

### Method 1: Clone and Build 

1. **Clone the repository**
   ```bash
   git clone <repository-url>
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

1. **Open Visual Studio 2022**
2. **Open the solution file**: `RoastMyCode.sln`
3. **Restore NuGet packages** (Visual Studio should do this automatically)
4. **Build the solution** (Ctrl+Shift+B)
5. **Press F5** or click the "Start" button

## Configuration 

### API Keys Setup

The application requires API keys for AI services. In the `gitignore` please comment out this file `appsettings.json`. There is another file that should contain the key, if you encounter any issue with the AI not working please sure that the key is also here`src\RoastMyCode\bin\Debug\net6.0-windows\appsettings.json`.

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

#### heme Toggle
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