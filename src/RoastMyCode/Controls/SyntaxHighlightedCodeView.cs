using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace RoastMyCode.Controls
{
    public class SyntaxHighlightedCodeView : UserControl
    {
        private TextEditor _editor;
        private ElementHost _host;
        private SyntaxHighlightingService _syntaxHighlightingService;
        private string _code = string.Empty;
        private string _language = "text";

        public string Code
        {
            get => _code;
            set
            {
                _code = value;
                UpdateEditor();
            }
        }

        public string Language
        {
            get => _language;
            set
            {
                _language = NormalizeLanguageName(value);
                UpdateEditor();
            }
        }

        public SyntaxHighlightedCodeView()
        {
            // Initialize the syntax highlighting service
            _syntaxHighlightingService = new SyntaxHighlightingService();
            
            // Set up control appearance - ChatGPT style dark code block
            this.BackColor = Color.FromArgb(31, 31, 31); // Dark background
            this.BorderStyle = BorderStyle.None;
            this.Margin = new Padding(0);
            this.Padding = new Padding(10);
            
            // Add a border with rounded corners
            this.Paint += (s, e) =>
            {
                using (var path = new System.Drawing.Drawing2D.GraphicsPath())
                {
                    int radius = 6; // Rounded corner radius
                    var rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                    path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
                    path.AddArc(rect.X + rect.Width - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
                    path.AddArc(rect.X + rect.Width - radius * 2, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 0, 90);
                    path.AddArc(rect.X, rect.Y + rect.Height - radius * 2, radius * 2, radius * 2, 90, 90);
                    path.CloseAllFigures();
                    
                    // Create region for the rounded corners
                    this.Region = new Region(path);
                    
                    // Use a subtle border color
                    using (var pen = new Pen(Color.FromArgb(50, 50, 50), 1))
                    {
                        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            };
            
            // Create the WPF TextEditor control
            _editor = new TextEditor
            {
                IsReadOnly = true,
                ShowLineNumbers = true,
                WordWrap = true,
                HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 14, // Slightly larger font size for better readability
                Background = System.Windows.Media.Brushes.Transparent,
                Foreground = System.Windows.Media.Brushes.White,
                Padding = new System.Windows.Thickness(12), // More padding for better readability
                LineNumbersForeground = System.Windows.Media.Brushes.DarkGray // Lighter line numbers
            };
            
            // Disable hyperlinks and other distractions
            _editor.TextArea.TextView.Options.ShowBoxForControlCharacters = false;
            _editor.TextArea.TextView.Options.EnableEmailHyperlinks = false;
            _editor.TextArea.TextView.Options.EnableHyperlinks = false;
            
            // Create an ElementHost to host the WPF control in WinForms
            _host = new ElementHost
            {
                Dock = DockStyle.Fill,
                Child = _editor,
                BackColor = Color.FromArgb(31, 31, 31), // Match the darker background color
                Margin = new Padding(0) // Remove margin to allow the editor to fill the space
            };
            
            // Add the host to this control
            Controls.Add(_host);
            
            // Set minimum size to ensure visibility
            this.MinimumSize = new Size(200, 100);
            
            // Update the editor with initial content
            UpdateEditor();
        }

        /// <summary>
        /// Normalizes language names to ensure consistency
        /// </summary>
        private string NormalizeLanguageName(string language)
        {
            if (string.IsNullOrEmpty(language))
                return "text";
                
            // Log the original language for debugging
            System.Diagnostics.Debug.WriteLine($"Normalizing language name: '{language}'");
            
            string normalizedLanguage = language.ToLowerInvariant().Trim();
            
            // Map alternate language names to standard ones
            switch (normalizedLanguage)
            {
                case "c#":
                case "cs":
                case "csharp":
                    return "csharp";
                    
                case "js":
                case "javascript":
                    return "javascript";
                    
                case "ts":
                case "typescript":
                    return "typescript";
                    
                case "py":
                case "python":
                    return "python";
                    
                case "php":
                case "php3":
                case "php4":
                case "php5":
                case "php7":
                case "php8":
                case "phtml":
                case "pht":
                    return "php";
                    
                case "html":
                case "htm":
                    return "html";
                    
                case "css":
                    return "css";
                    
                case "java":
                    return "java";
                    
                case "json":
                    return "json";
                    
                case "xml":
                    return "xml";
                    
                case "sql":
                    return "sql";
                    
                case "md":
                case "markdown":
                    return "markdown";
                    
                case "rb":
                case "ruby":
                    return "ruby";
                    
                case "go":
                case "golang":
                    return "go";
                    
                case "rs":
                case "rust":
                    return "rust";
                    
                case "swift":
                    return "swift";
                    
                case "kt":
                case "kotlin":
                    return "kotlin";
                    
                default:
                    return normalizedLanguage;
            }
        }

        /// <summary>
        /// Updates the editor with the current code and language
        /// </summary>
        private void UpdateEditor()
        {
            if (_editor == null) return;
            
            // Update the text content
            _editor.Text = _code;
            
            // Normalize the language name
            string normalizedLanguage = NormalizeLanguageName(_language);
            
            // Apply syntax highlighting based on language
            if (!string.IsNullOrEmpty(normalizedLanguage) && normalizedLanguage != "text")
            {
                // Get the highlighting definition for the language
                var definition = _syntaxHighlightingService.GetDefinition(normalizedLanguage);
                
                if (definition != null)
                {
                    _editor.SyntaxHighlighting = definition;
                    
                    // Apply language-specific settings
                    ApplyLanguageSpecificSettings(normalizedLanguage);
                    
                    System.Diagnostics.Debug.WriteLine($"Applied syntax highlighting for {normalizedLanguage}");
                }
                else
                {
                    // Try with some common fallbacks
                    if (normalizedLanguage == "jsx")
                    {
                        definition = _syntaxHighlightingService.GetDefinition("javascript");
                        if (definition != null)
                            _editor.SyntaxHighlighting = definition;
                    }
                    else if (normalizedLanguage == "tsx")
                    {
                        definition = _syntaxHighlightingService.GetDefinition("typescript");
                        if (definition != null)
                            _editor.SyntaxHighlighting = definition;
                    }
                    else
                    {
                        // Final fallback to text
                        _editor.SyntaxHighlighting = null;
                        System.Diagnostics.Debug.WriteLine($"No syntax highlighting available for {normalizedLanguage}, using plain text");
                    }
                }
            }
            else
            {
                // Plain text, no syntax highlighting
                _editor.SyntaxHighlighting = null;
            }
            
            // Set editor options
            _editor.Options.ShowTabs = false;
            _editor.Options.ShowEndOfLine = false;
            _editor.Options.HighlightCurrentLine = false; // Don't highlight the current line
            
            // Force the editor to use our dark theme colors
            _editor.TextArea.TextView.LinkTextForegroundBrush = System.Windows.Media.Brushes.LightBlue;
            _editor.TextArea.TextView.CurrentLineBackground = System.Windows.Media.Brushes.Transparent;
            
            // Refresh the display
            _editor.TextArea.TextView.Redraw();
            
            // Make sure the control is visible and properly sized
            this.Visible = true;
            this.Invalidate();
        }
        
        /// <summary>
        /// Apply language-specific editor settings
        /// </summary>
        private void ApplyLanguageSpecificSettings(string language)
        {
            // Apply common settings for all languages
            _editor.ShowLineNumbers = true;
            _editor.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 31, 31, 31));
            _editor.Foreground = System.Windows.Media.Brushes.White;
            
            // Apply language-specific settings
            switch (language)
            {
                case "php":
                    ApplyPhpEditorSettings();
                    break;
                    
                case "javascript":
                case "typescript":
                    ApplyJsEditorSettings();
                    break;
                    
                case "html":
                case "xml":
                    ApplyMarkupEditorSettings();
                    break;
                    
                case "css":
                    ApplyCssEditorSettings();
                    break;
                    
                case "csharp":
                    ApplyCSharpEditorSettings();
                    break;
                    
                case "python":
                    ApplyPythonEditorSettings();
                    break;
                    
                default:
                    // Use default settings for other languages
                    break;
            }
        }
        
        /// <summary>
        /// Apply PHP-specific editor settings
        /// </summary>
        private void ApplyPhpEditorSettings()
        {
            _editor.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            _editor.FontSize = 14;
            _editor.LineNumbersForeground = System.Windows.Media.Brushes.Gray;
            _editor.TextArea.TextView.Options.EnableHyperlinks = false;
        }
        
        /// <summary>
        /// Apply JavaScript/TypeScript-specific editor settings
        /// </summary>
        private void ApplyJsEditorSettings()
        {
            _editor.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            _editor.FontSize = 14;
            _editor.LineNumbersForeground = System.Windows.Media.Brushes.Gray;
        }
        
        /// <summary>
        /// Apply HTML/XML-specific editor settings
        /// </summary>
        private void ApplyMarkupEditorSettings()
        {
            _editor.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            _editor.FontSize = 14;
            _editor.LineNumbersForeground = System.Windows.Media.Brushes.Gray;
        }
        
        /// <summary>
        /// Apply CSS-specific editor settings
        /// </summary>
        private void ApplyCssEditorSettings()
        {
            _editor.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            _editor.FontSize = 14;
            _editor.LineNumbersForeground = System.Windows.Media.Brushes.Gray;
        }
        
        /// <summary>
        /// Apply C#-specific editor settings
        /// </summary>
        private void ApplyCSharpEditorSettings()
        {
            _editor.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            _editor.FontSize = 14;
            _editor.LineNumbersForeground = System.Windows.Media.Brushes.Gray;
        }
        
        /// <summary>
        /// Apply Python-specific editor settings
        /// </summary>
        private void ApplyPythonEditorSettings()
        {
            _editor.FontFamily = new System.Windows.Media.FontFamily("Consolas");
            _editor.FontSize = 14;
            _editor.LineNumbersForeground = System.Windows.Media.Brushes.Gray;
        }
        
        /// <summary>
        /// Handle resize events to ensure the editor fits properly
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            
            if (_host != null)
            {
                _host.Size = this.ClientSize;
            }
            
            // Refresh the editor when resized
            if (_editor != null)
            {
                _editor.TextArea.TextView.Redraw();
            }
        }
    }
}
