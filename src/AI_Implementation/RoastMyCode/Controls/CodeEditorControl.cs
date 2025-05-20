using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace RoastMyCode.Controls
{
    public class CodeEditorControl : System.Windows.Forms.Control
    {
        private System.Windows.Forms.Panel _host = null!;
        private TextEditor _editor = null!;
        private SearchPanel _searchPanel = null!;
        private ElementHost _elementHost = null!;

        // Modern dark theme colors
        private static readonly System.Windows.Media.Color BackgroundColor = System.Windows.Media.Color.FromRgb(30, 30, 35);  // Slightly blue-tinted dark background
        private static readonly System.Windows.Media.Color ForegroundColor = System.Windows.Media.Color.FromRgb(220, 220, 220);  // Soft white
        private static readonly System.Windows.Media.Color LineNumberColor = System.Windows.Media.Color.FromRgb(100, 100, 100);  // Muted gray for line numbers
        private static readonly System.Windows.Media.Color SelectionColor = System.Windows.Media.Color.FromRgb(62, 95, 150);  // Blue selection
        private static readonly System.Windows.Media.Color CaretColor = System.Windows.Media.Color.FromRgb(255, 255, 255);  // White caret

        public event EventHandler<string>? CodeChanged;

        public string Code
        {
            get => _editor?.Text ?? string.Empty;
            set
            {
                if (_editor != null)
                {
                    _editor.Text = value;
                }
            }
        }

        public string Language
        {
            get => _editor?.SyntaxHighlighting?.Name ?? string.Empty;
            set
            {
                if (_editor != null)
                {
                    _editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition(value);
                }
            }
        }

        public CodeEditorControl()
        {
            InitializeEditor();
        }

        private void InitializeEditor()
        {
            _host = new System.Windows.Forms.Panel
            {
                Dock = DockStyle.Fill
            };

            _elementHost = new ElementHost
            {
                Dock = DockStyle.Fill
            };

            _editor = new TextEditor
            {
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 13,
                Background = new SolidColorBrush(BackgroundColor),
                Foreground = new SolidColorBrush(ForegroundColor),
                ShowLineNumbers = true,
                WordWrap = false,
                HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                Options =
                {
                    EnableEmailHyperlinks = false,
                    EnableHyperlinks = false,
                    EnableVirtualSpace = false,
                    ShowSpaces = false,
                    ShowTabs = false,
                    ShowEndOfLine = false,
                    ShowBoxForControlCharacters = false
                }
            };

            // Set up syntax highlighting with custom colors
            var highlighting = HighlightingManager.Instance.GetDefinition("C#");
            if (highlighting != null)
            {
                // Customize syntax highlighting colors
                var customHighlighting = new CustomHighlighting(highlighting);
                _editor.SyntaxHighlighting = customHighlighting;
            }

            // Set up search panel with custom colors
            _searchPanel = SearchPanel.Install(_editor.TextArea);
            _searchPanel.MarkerBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 0));  // Yellow search results
            _searchPanel.MarkerPen = new System.Windows.Media.Pen(new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 165, 0)), 1);  // Orange border

            // Handle text changes
            _editor.TextChanged += (s, e) =>
            {
                CodeChanged?.Invoke(this, _editor.Text);
            };

            // Add the editor to the element host
            _elementHost.Child = _editor;

            // Add the element host to the panel
            _host.Controls.Add(_elementHost);

            // Add the panel to the control
            Controls.Add(_host);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_host != null)
            {
                _host.Width = Width;
                _host.Height = Height;
            }
        }
    }

    // Custom highlighting class to override default colors
    public class CustomHighlighting : IHighlightingDefinition
    {
        private readonly IHighlightingDefinition _baseHighlighting;

        public CustomHighlighting(IHighlightingDefinition baseHighlighting)
        {
            _baseHighlighting = baseHighlighting;
        }

        public string Name => _baseHighlighting.Name;
        public IEnumerable<string> Extensions => Array.Empty<string>();
        public IDictionary<string, string> Properties => _baseHighlighting.Properties;
        public IEnumerable<HighlightingColor> NamedHighlightingColors => _baseHighlighting.NamedHighlightingColors;

        public HighlightingRuleSet MainRuleSet => _baseHighlighting.MainRuleSet;

        public HighlightingRuleSet GetNamedRuleSet(string name) => _baseHighlighting.GetNamedRuleSet(name);

        public HighlightingColor GetNamedColor(string name)
        {
            var color = _baseHighlighting.GetNamedColor(name);
            if (color != null)
            {
                // Customize specific syntax highlighting colors
                switch (name)
                {
                    case "Keyword":
                        color.Foreground = new SimpleHighlightingBrush(System.Windows.Media.Color.FromRgb(86, 156, 214));  // Light blue
                        break;
                    case "String":
                        color.Foreground = new SimpleHighlightingBrush(System.Windows.Media.Color.FromRgb(206, 145, 120));  // Soft orange
                        break;
                    case "Comment":
                        color.Foreground = new SimpleHighlightingBrush(System.Windows.Media.Color.FromRgb(106, 153, 85));  // Muted green
                        break;
                    case "Method":
                        color.Foreground = new SimpleHighlightingBrush(System.Windows.Media.Color.FromRgb(220, 220, 170));  // Light yellow
                        break;
                    case "Type":
                        color.Foreground = new SimpleHighlightingBrush(System.Windows.Media.Color.FromRgb(78, 201, 176));  // Teal
                        break;
                }
            }
            return color ?? new HighlightingColor();
        }
    }
} 