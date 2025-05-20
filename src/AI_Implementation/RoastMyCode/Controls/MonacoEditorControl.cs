using System;
using System.Windows.Forms;

namespace RoastMyCode.Controls
{
    public class MonacoEditorControl : Control
    {
        private WebBrowser _browser;
        private string _currentLanguage = "javascript";
        private string _currentCode = string.Empty;
        private bool _isInitialized = false;
        private Timer _resizeTimer;

        public event EventHandler<string> CodeChanged;

        public string Code
        {
            get => _currentCode;
            set
            {
                _currentCode = value;
                if (_browser?.Document != null)
                {
                    _browser.Document.InvokeScript("setValue", new object[] { value });
                }
            }
        }

        public string Language
        {
            get => _currentLanguage;
            set
            {
                _currentLanguage = value;
                if (_browser?.Document != null)
                {
                    _browser.Document.InvokeScript("setLanguage", new object[] { value });
                }
            }
        }

        public MonacoEditorControl()
        {
            InitializeResizeTimer();
            InitializeBrowser();
        }

        private void InitializeResizeTimer()
        {
            _resizeTimer = new Timer
            {
                Interval = 100 // Debounce resize events
            };
            _resizeTimer.Tick += (s, e) =>
            {
                _resizeTimer.Stop();
                if (_browser?.Document != null)
                {
                    _browser.Document.InvokeScript("eval", new object[] { "if(window.editor) window.editor.layout()" });
                }
            };
        }

        private void InitializeBrowser()
        {
            _browser = new WebBrowser
            {
                Dock = DockStyle.Fill,
                ScriptErrorsSuppressed = true
            };

            _browser.ObjectForScripting = new ScriptInterface(this);
            _browser.Navigated += (s, e) =>
            {
                if (_browser.Document != null)
                {
                    _browser.Document.InvokeScript("setValue", new object[] { _currentCode });
                    _browser.Document.InvokeScript("setLanguage", new object[] { _currentLanguage });
                    _isInitialized = true;
                }
            };

            Controls.Add(_browser);
            LoadMonacoEditor();
        }

        private void LoadMonacoEditor()
        {
            string html = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset='UTF-8'>
                    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
                    <style>
                        html, body, #container {
                            width: 100%;
                            height: 100%;
                            margin: 0;
                            padding: 0;
                            overflow: hidden;
                            background-color: #1e1e1e;
                        }
                    </style>
                </head>
                <body>
                    <div id='container'></div>
                    <script>
                        // Load Monaco Editor
                        var script = document.createElement('script');
                        script.src = 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.34.1/min/vs/loader.min.js';
                        script.onload = function() {
                            require.config({ paths: { 'vs': 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.34.1/min/vs' }});
                            require(['vs/editor/editor.main'], function() {
                                window.editor = monaco.editor.create(document.getElementById('container'), {
                                    value: '',
                                    language: 'javascript',
                                    theme: 'vs-dark',
                                    automaticLayout: true,
                                    minimap: { enabled: false },
                                    scrollBeyondLastLine: false,
                                    fontSize: 14,
                                    lineNumbers: 'on',
                                    roundedSelection: false,
                                    scrollbar: {
                                        vertical: 'visible',
                                        horizontal: 'visible',
                                        useShadows: true,
                                        verticalScrollbarSize: 10,
                                        horizontalScrollbarSize: 10,
                                        verticalSliderSize: 10,
                                        horizontalSliderSize: 10
                                    },
                                    wordWrap: 'on',
                                    wrappingStrategy: 'advanced',
                                    lineDecorationsWidth: 0,
                                    lineNumbersMinChars: 3,
                                    glyphMargin: false,
                                    folding: true,
                                    foldingStrategy: 'indentation',
                                    showFoldingControls: 'always',
                                    matchBrackets: 'always',
                                    autoClosingBrackets: 'always',
                                    autoClosingQuotes: 'always',
                                    autoIndent: 'full',
                                    formatOnPaste: true,
                                    formatOnType: true,
                                    suggestOnTriggerCharacters: true,
                                    acceptSuggestionOnEnter: 'on',
                                    tabCompletion: 'on',
                                    wordBasedSuggestions: 'on',
                                    parameterHints: {
                                        enabled: true,
                                        cycle: true
                                    }
                                });
                                
                                window.editor.onDidChangeModelContent(function(e) {
                                    window.external.CodeChanged(window.editor.getValue());
                                });

                                // Handle window resize
                                window.addEventListener('resize', function() {
                                    if (window.editor) {
                                        window.editor.layout();
                                    }
                                });
                            });
                        };
                        document.head.appendChild(script);

                        function setValue(value) {
                            if (window.editor) {
                                window.editor.setValue(value);
                                window.editor.layout();
                            }
                        }

                        function setLanguage(language) {
                            if (window.editor) {
                                monaco.editor.setModelLanguage(window.editor.getModel(), language);
                                window.editor.layout();
                            }
                        }

                        function getScrollPosition() {
                            if (window.editor) {
                                return {
                                    scrollTop: window.editor.getScrollTop(),
                                    scrollLeft: window.editor.getScrollLeft()
                                };
                            }
                            return { scrollTop: 0, scrollLeft: 0 };
                        }

                        function setScrollPosition(scrollTop, scrollLeft) {
                            if (window.editor) {
                                window.editor.setScrollTop(scrollTop);
                                window.editor.setScrollLeft(scrollLeft);
                            }
                        }
                    </script>
                </body>
                </html>";

            _browser.DocumentText = html;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_isInitialized)
            {
                _resizeTimer.Stop();
                _resizeTimer.Start();
            }
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (_isInitialized)
            {
                _resizeTimer.Stop();
                _resizeTimer.Start();
            }
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            if (_isInitialized)
            {
                _resizeTimer.Stop();
                _resizeTimer.Start();
            }
        }

        [System.Runtime.InteropServices.ComVisible(true)]
        public class ScriptInterface
        {
            private readonly MonacoEditorControl _control;

            public ScriptInterface(MonacoEditorControl control)
            {
                _control = control;
            }

            public void CodeChanged(string code)
            {
                _control._currentCode = code;
                _control.CodeChanged?.Invoke(_control, code);
            }
        }
    }
} 