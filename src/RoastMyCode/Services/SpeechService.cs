using System;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RoastMyCode.Services
{
    public class SpeechService
    {
        private readonly SpeechSynthesizer _synthesizer;
        private readonly Random _random;

        public SpeechService()
        {
            _synthesizer = new SpeechSynthesizer();
            _random = new Random();
            InitializeVoiceSettings();
        }

        private void InitializeVoiceSettings()
        {
            try
            {
                // Get all available voices
                InstalledVoice[] voices = _synthesizer.GetInstalledVoices().ToArray();
                
                // Try to select a more natural-sounding voice
                var preferredVoices = voices
                    .Where(v => v.VoiceInfo.Gender == VoiceGender.Female || 
                                v.VoiceInfo.Gender == VoiceGender.Neutral)
                    .OrderByDescending(v => v.VoiceInfo.Age == VoiceAge.Adult)
                    .ThenByDescending(v => v.VoiceInfo.Age == VoiceAge.Teen)
                    .FirstOrDefault();

                if (preferredVoices != null)
                {
                    _synthesizer.SelectVoice(preferredVoices.VoiceInfo.Name);
                }

                // Set optimized settings
                _synthesizer.Volume = 100; // Max volume
                _synthesizer.Rate = -2; // Slightly slower speaking rate for better clarity
                _synthesizer.SetOutputToDefaultAudioDevice();
            }
            catch
            {
                // Fallback to default settings if initialization fails
                _synthesizer.Volume = 100;
                _synthesizer.Rate = 0;
                _synthesizer.SetOutputToDefaultAudioDevice();
            }
        }

        public async Task SpeakAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            try
            {
                // Clear any pending speech
                _synthesizer.SpeakAsyncCancelAll();
                
                // Add some natural pauses and emphasis
                string enhancedText = $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>{EnhanceSpeech(text)}</speak>";
                
                // Queue the speech with a TaskCompletionSource for better async handling
                var tcs = new TaskCompletionSource<bool>();
                _synthesizer.SpeakCompleted += (s, e) => tcs.SetResult(true);
                _synthesizer.SpeakAsync(enhancedText);
                await tcs.Task;
            }
            catch (Exception ex)
            {
                // Log error instead of showing message box to prevent UI blocking
                Debug.WriteLine($"Error speaking message: {ex.Message}");
            }
        }

        private string EnhanceSpeech(string text)
        {
            // Add natural pauses
            text = text.Replace(". ", ". <break time='500ms'/> ");
            text = text.Replace("! ", "! <break time='300ms'/> ");
            text = text.Replace("? ", "? <break time='400ms'/> ");
            
            return text;
        }

        public void Dispose()
        {
            _synthesizer.Dispose();
        }
    }
}
