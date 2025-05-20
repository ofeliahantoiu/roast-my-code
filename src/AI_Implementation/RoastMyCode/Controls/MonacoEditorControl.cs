using System;
using System.Windows.Forms;

namespace RoastMyCode.Controls
{
    public class MonacoEditorControl : Control
    {
        private WebBrowser _browser;
        private string _currentLanguage = "javascript";
        private string _currentCode = string.Empty;

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
            InitializeBrowser();
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
                                        horizontal: 'visible'
                                    }
                                });
                                
                                window.editor.onDidChangeModelContent(function(e) {
                                    window.external.CodeChanged(window.editor.getValue());
                                });
                            });
                        };
                        document.head.appendChild(script);

                        function setValue(value) {
                            if (window.editor) {
                                window.editor.setValue(value);
                            }
                        }

                        function setLanguage(language) {
                            if (window.editor) {
                                monaco.editor.setModelLanguage(window.editor.getModel(), language);
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
            if (_browser?.Document != null)
            {
                _browser.Document.InvokeScript("eval", new object[] { "if(window.editor) window.editor.layout()" });
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