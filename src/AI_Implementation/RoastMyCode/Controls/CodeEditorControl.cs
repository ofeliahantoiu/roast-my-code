using System;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace RoastMyCode.Controls
{
    public class CodeEditorControl : System.Windows.Forms.Control
    {
        private System.Windows.Forms.Panel _host = null!;
        private TextEditor _editor = null!;
        private SearchPanel _searchPanel = null!;
        private ElementHost _elementHost = null!;

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
                FontSize = 12,
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 30)),
                Foreground = new SolidColorBrush(System.Windows.Media.Colors.White),
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

            // Set up syntax highlighting
            _editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");

            // Set up search panel
            _searchPanel = SearchPanel.Install(_editor.TextArea);

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
} 