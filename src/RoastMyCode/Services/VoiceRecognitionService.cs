using System;
using System.Speech.Recognition;
using System.Threading.Tasks;

namespace RoastMyCode.Services
{
    public class VoiceRecognitionService
    {
        private readonly SpeechRecognitionEngine _recognizer;
        private bool _isListening;

        public event EventHandler<string>? SpeechRecognized;

        public VoiceRecognitionService()
        {
            _recognizer = new SpeechRecognitionEngine();
            _recognizer.SetInputToDefaultAudioDevice();
            _recognizer.SpeechRecognized += OnSpeechRecognized;
            InitializeGrammar();
        }

        private void InitializeGrammar()
        {
            var grammar = new GrammarBuilder();
            grammar.AppendDictation();
            _recognizer.LoadGrammar(new Grammar(grammar));
        }

        private void OnSpeechRecognized(object? sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result != null && !string.IsNullOrEmpty(e.Result.Text))
            {
                SpeechRecognized?.Invoke(this, e.Result.Text);
            }
        }

        public async Task StartListeningAsync()
        {
            if (_isListening) return;

            try
            {
                _isListening = true;
                await Task.Run(() => _recognizer.RecognizeAsync(RecognizeMode.Multiple));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting voice recognition: {ex.Message}");
            }
        }

        public void StopListening()
        {
            if (!_isListening) return;

            try
            {
                _recognizer.RecognizeAsyncStop();
                _isListening = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping voice recognition: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _recognizer.Dispose();
        }
    }
}
