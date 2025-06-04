using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RoastMyCode
{
    /// <summary>
    /// Provides syntax highlighting functionality for different programming languages
    /// </summary>
    public class SyntaxHighlighter
    {
        private readonly Dictionary<string, LanguageDefinition> _languageDefinitions;
        private readonly Color _defaultTextColor;
        private readonly bool _isDarkMode;

        public SyntaxHighlighter(bool isDarkMode)
        {
            _isDarkMode = isDarkMode;
            _defaultTextColor = isDarkMode ? Color.White : Color.Black;
            _languageDefinitions = InitializeLanguageDefinitions();
        }

        /// <summary>
        /// Applies syntax highlighting to a RichTextBox based on the detected language
        /// </summary>
        /// <param name="rtb">The RichTextBox to highlight</param>
        /// <param name="language">The programming language</param>
        public void ApplyHighlighting(RichTextBox rtb, string language)
        {
            if (rtb == null || string.IsNullOrEmpty(rtb.Text))
                return;

            // Store the current selection
            int selectionStart = rtb.SelectionStart;
            int selectionLength = rtb.SelectionLength;

            // Default color for the entire text
            rtb.SelectionStart = 0;
            rtb.SelectionLength = rtb.TextLength;
            rtb.SelectionColor = _defaultTextColor;

            // Get language definition
            if (!_languageDefinitions.TryGetValue(language.ToLowerInvariant(), out var langDef))
            {
                // If language not found, use a simple default highlighting
                ApplyDefaultHighlighting(rtb);
            }
            else
            {
                // Apply language-specific highlighting
                ApplyLanguageHighlighting(rtb, langDef);
            }

            // Restore the selection
            rtb.SelectionStart = selectionStart;
            rtb.SelectionLength = selectionLength;
            rtb.SelectionColor = _defaultTextColor;
        }

        private void ApplyDefaultHighlighting(RichTextBox rtb)
        {
            // Simple highlighting for unknown languages
            HighlightPattern(rtb, @"\/\/.*$", Color.Green, RegexOptions.Multiline); // Single line comments
            HighlightPattern(rtb, @"\/\*[\s\S]*?\*\/", Color.Green, RegexOptions.Multiline); // Multi-line comments
            HighlightPattern(rtb, "\".*?\"", Color.FromArgb(214, 157, 133), RegexOptions.None); // Strings
            HighlightPattern(rtb, "'.*?'", Color.FromArgb(214, 157, 133), RegexOptions.None); // Chars
            HighlightPattern(rtb, @"\b(if|else|while|for|foreach|return|break|continue|switch|case|default|try|catch|finally|throw|new|this|base|class|interface|struct|enum|namespace|using|public|private|protected|internal|static|readonly|const|virtual|override|abstract|sealed|async|await)\b", 
                Color.FromArgb(86, 156, 214), RegexOptions.None); // Keywords
        }

        private void ApplyLanguageHighlighting(RichTextBox rtb, LanguageDefinition langDef)
        {
            // Apply each pattern in the language definition
            foreach (var pattern in langDef.Patterns)
            {
                HighlightPattern(rtb, pattern.Regex, pattern.Color, pattern.Options);
            }
        }

        private void HighlightPattern(RichTextBox rtb, string pattern, Color color, RegexOptions options)
        {
            Regex regex = new Regex(pattern, options);
            MatchCollection matches = regex.Matches(rtb.Text);

            foreach (Match match in matches)
            {
                rtb.SelectionStart = match.Index;
                rtb.SelectionLength = match.Length;
                rtb.SelectionColor = color;
            }
        }

        private Dictionary<string, LanguageDefinition> InitializeLanguageDefinitions()
        {
            var definitions = new Dictionary<string, LanguageDefinition>(StringComparer.OrdinalIgnoreCase);

            // C# language definition
            definitions["c#"] = new LanguageDefinition
            {
                Patterns = new List<HighlightPattern>
                {
                    new HighlightPattern(@"\/\/.*$", Color.Green, RegexOptions.Multiline), // Single line comments
                    new HighlightPattern(@"\/\*[\s\S]*?\*\/", Color.Green, RegexOptions.Multiline), // Multi-line comments
                    new HighlightPattern("\".*?\"", Color.FromArgb(214, 157, 133), RegexOptions.None), // Strings
                    new HighlightPattern("'.*?'", Color.FromArgb(214, 157, 133), RegexOptions.None), // Chars
                    new HighlightPattern(@"\b(abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|false|finally|fixed|float|for|foreach|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|private|protected|public|readonly|ref|return|sbyte|sealed|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|virtual|void|volatile|while)\b", 
                        Color.FromArgb(86, 156, 214), RegexOptions.None), // Keywords
                    new HighlightPattern(@"\b(bool|byte|char|decimal|double|float|int|long|object|sbyte|short|string|uint|ulong|ushort|var|void)\b", 
                        Color.FromArgb(78, 201, 176), RegexOptions.None), // Types
                    new HighlightPattern(@"^\s*#\w+", Color.FromArgb(155, 155, 155), RegexOptions.Multiline) // Preprocessor directives
                }
            };

            // JavaScript language definition
            definitions["javascript"] = new LanguageDefinition
            {
                Patterns = new List<HighlightPattern>
                {
                    new HighlightPattern(@"\/\/.*$", Color.Green, RegexOptions.Multiline), // Single line comments
                    new HighlightPattern(@"\/\*[\s\S]*?\*\/", Color.Green, RegexOptions.Multiline), // Multi-line comments
                    new HighlightPattern("\".*?\"", Color.FromArgb(214, 157, 133), RegexOptions.None), // Double-quoted strings
                    new HighlightPattern("'.*?'", Color.FromArgb(214, 157, 133), RegexOptions.None), // Single-quoted strings
                    new HighlightPattern("`.*?`", Color.FromArgb(214, 157, 133), RegexOptions.None), // Template literals
                    new HighlightPattern(@"\b(break|case|catch|class|const|continue|debugger|default|delete|do|else|export|extends|finally|for|function|if|import|in|instanceof|new|return|super|switch|this|throw|try|typeof|var|void|while|with|yield|async|await|of)\b", 
                        Color.FromArgb(86, 156, 214), RegexOptions.None), // Keywords
                    new HighlightPattern(@"\b(true|false|null|undefined|NaN|Infinity)\b", 
                        Color.FromArgb(86, 156, 214), RegexOptions.None), // Constants
                    new HighlightPattern(@"\b(Array|Boolean|Date|Error|Function|JSON|Math|Number|Object|RegExp|String|Promise|Map|Set|Symbol|setTimeout|clearTimeout|setInterval|clearInterval)\b", 
                        Color.FromArgb(78, 201, 176), RegexOptions.None) // Built-in objects
                }
            };

            // Python language definition
            definitions["python"] = new LanguageDefinition
            {
                Patterns = new List<HighlightPattern>
                {
                    new HighlightPattern(@"#.*$", Color.Green, RegexOptions.Multiline), // Single line comments
                    new HighlightPattern("'''.*?'''", Color.Green, RegexOptions.Singleline), // Multi-line strings/comments
                    new HighlightPattern("\"\"\".*?\"\"\"", Color.Green, RegexOptions.Singleline), // Multi-line strings/comments
                    new HighlightPattern("\".*?\"", Color.FromArgb(214, 157, 133), RegexOptions.None), // Double-quoted strings
                    new HighlightPattern("'.*?'", Color.FromArgb(214, 157, 133), RegexOptions.None), // Single-quoted strings
                    new HighlightPattern(@"\b(and|as|assert|async|await|break|class|continue|def|del|elif|else|except|finally|for|from|global|if|import|in|is|lambda|nonlocal|not|or|pass|raise|return|try|while|with|yield)\b", 
                        Color.FromArgb(86, 156, 214), RegexOptions.None), // Keywords
                    new HighlightPattern(@"\b(True|False|None|NotImplemented|Ellipsis|__debug__)\b", 
                        Color.FromArgb(86, 156, 214), RegexOptions.None), // Constants
                    new HighlightPattern(@"\b(object|int|float|bool|str|list|tuple|set|dict|frozenset|bytearray|bytes)\b", 
                        Color.FromArgb(78, 201, 176), RegexOptions.None) // Built-in types
                }
            };

            // Java language definition
            definitions["java"] = new LanguageDefinition
            {
                Patterns = new List<HighlightPattern>
                {
                    new HighlightPattern(@"\/\/.*$", Color.Green, RegexOptions.Multiline), // Single line comments
                    new HighlightPattern(@"\/\*[\s\S]*?\*\/", Color.Green, RegexOptions.Multiline), // Multi-line comments
                    new HighlightPattern("\".*?\"", Color.FromArgb(214, 157, 133), RegexOptions.None), // Strings
                    new HighlightPattern("'.*?'", Color.FromArgb(214, 157, 133), RegexOptions.None), // Chars
                    new HighlightPattern(@"\b(abstract|assert|boolean|break|byte|case|catch|char|class|const|continue|default|do|double|else|enum|extends|final|finally|float|for|goto|if|implements|import|instanceof|int|interface|long|native|new|package|private|protected|public|return|short|static|strictfp|super|switch|synchronized|this|throw|throws|transient|try|void|volatile|while)\b", 
                        Color.FromArgb(86, 156, 214), RegexOptions.None), // Keywords
                    new HighlightPattern(@"\b(true|false|null)\b", 
                        Color.FromArgb(86, 156, 214), RegexOptions.None), // Constants
                    new HighlightPattern(@"\b(boolean|byte|char|double|float|int|long|short|void)\b", 
                        Color.FromArgb(78, 201, 176), RegexOptions.None) // Primitive types
                }
            };

            return definitions;
        }

        /// <summary>
        /// Updates the color scheme based on dark/light mode
        /// </summary>
        /// <param name="isDarkMode">Whether dark mode is enabled</param>
        public void UpdateTheme(bool isDarkMode)
        {
            // Reinitialize with new theme colors
            _languageDefinitions.Clear();
            InitializeLanguageDefinitions();
        }
    }

    /// <summary>
    /// Defines a language for syntax highlighting
    /// </summary>
    public class LanguageDefinition
    {
        public List<HighlightPattern> Patterns { get; set; } = new List<HighlightPattern>();
    }

    /// <summary>
    /// Defines a pattern to highlight in code
    /// </summary>
    public class HighlightPattern
    {
        public string Regex { get; }
        public Color Color { get; }
        public RegexOptions Options { get; }

        public HighlightPattern(string regex, Color color, RegexOptions options)
        {
            Regex = regex;
            Color = color;
            Options = options;
        }
    }
}
