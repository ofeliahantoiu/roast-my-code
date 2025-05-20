# Roast My Code

A code review application that uses AI to analyze and provide feedback on your code.

## Project Structure

- `RoastCode/` - Main backend application
  - Contains the core functionality for code analysis and AI integration
- `AI_Implementation/` - AI-specific implementations
  - Contains AI model integration and related utilities

## Prerequisites

- .NET 6.0 or later
- Visual Studio 2022 or Visual Studio Code
- OpenAI API key (for AI functionality)

## Setup Instructions

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/roast-my-code.git
   cd roast-my-code
   ```

2. Configure the environment:
   - Create a `.env` file in the `RoastCode` directory
   - Add your OpenAI API key:
     ```
     OPENAI_API_KEY=your_api_key_here
     ```

3. Build and run the application:
   - Open the solution in Visual Studio
   - Restore NuGet packages
   - Build the solution
   - Run the application

## Development

- The main application logic is in the `RoastCode` directory
- AI implementations are in the `AI_Implementation` directory
- Configuration settings can be modified in `Config.cs`

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.