using System;
using System.Collections.Generic;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RoastMyCode
{
    /// <summary>
    /// Manages text-to-speech voice output for roast content
    /// </summary>
    public class VoiceOutputManager : IDisposable
    {
        private SpeechSynthesizer? _synthesizer;
        private bool _isInitialized = false;
        private bool _isSpeaking = false;
        private float _volume = 100; // 0-100
        private float _rate = 0; // -10 to 10
        private string? _selectedVoice = null;
        private List<string> _availableVoices = new List<string>();
        
        public event EventHandler<SpeakStartedEventArgs>? SpeakStarted;
        public event EventHandler<SpeakCompletedEventArgs>? SpeakCompleted;
        public event EventHandler<SpeakProgressEventArgs>? SpeakProgress;
        
        public bool IsInitialized => _isInitialized;
        public bool IsSpeaking => _isSpeaking;
        public float Volume 
        { 
            get => _volume; 
            set 
            {
                _volume = Math.Clamp(value, 0, 100);
                if (_synthesizer != null)
                {
                    _synthesizer.Volume = (int)_volume;
                }
            }
        }
        
        public float Rate
        {
            get => _rate;
            set
            {
                _rate = Math.Clamp(value, -10, 10);
                if (_synthesizer != null)
                {
                    _synthesizer.Rate = (int)_rate;
                }
            }
        }
        
        public string SelectedVoice
        {
            get => _selectedVoice ?? string.Empty;
            set
            {
                if (_availableVoices.Contains(value))
                {
                    _selectedVoice = value;
                    if (_synthesizer != null)
                    {
                        _synthesizer.SelectVoice(_selectedVoice);
                    }
                }
            }
        }
        
        public List<string> AvailableVoices => _availableVoices;
        
        public VoiceOutputManager()
        {
            InitializeSynthesizer();
        }
        
        private void InitializeSynthesizer()
        {
            try
            {
                _synthesizer = new SpeechSynthesizer();
                _synthesizer.SpeakStarted += (s, e) => 
                {
                    _isSpeaking = true;
                    SpeakStarted?.Invoke(this, e);
                };
                
                _synthesizer.SpeakCompleted += (s, e) => 
                {
                    _isSpeaking = false;
                    SpeakCompleted?.Invoke(this, e);
                };
                
                _synthesizer.SpeakProgress += (s, e) => 
                {
                    SpeakProgress?.Invoke(this, e);
                };
                
                _synthesizer.Volume = (int)_volume;
                _synthesizer.Rate = (int)_rate;
                
                // Get available voices
                _availableVoices.Clear();
                foreach (var voice in _synthesizer.GetInstalledVoices())
                {
                    if (voice.Enabled)
                    {
                        _availableVoices.Add(voice.VoiceInfo.Name);
                    }
                }
                
                // Select default voice
                if (_availableVoices.Count > 0)
                {
                    _selectedVoice = _availableVoices[0];
                    _synthesizer.SelectVoice(_selectedVoice);
                }
                
                _isInitialized = true;
                Debug.WriteLine("Speech synthesizer initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize speech synthesizer: {ex.Message}");
                _isInitialized = false;
            }
        }
        
        /// <summary>
        /// Speaks the provided text asynchronously
        /// </summary>
        /// <param name="text">Text to speak</param>
        /// <param name="priority">Priority of speech (can be used to interrupt current speech)</param>
        /// <returns>Task representing the asynchronous operation</returns>
        public async Task SpeakAsync(string text, SpeechPriority priority = SpeechPriority.Normal)
        {
            if (!_isInitialized || string.IsNullOrEmpty(text))
                return;
                
            try
            {
                // Cancel current speech if higher priority
                if (_isSpeaking && priority == SpeechPriority.High)
                {
                    _synthesizer?.SpeakAsyncCancelAll();
                }
                
                // Speak asynchronously
                await Task.Run(() => 
                {
                    if (priority == SpeechPriority.High || !_isSpeaking)
                    {
                        _synthesizer?.SpeakAsync(text);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error speaking text: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Speaks the provided text synchronously (blocking)
        /// </summary>
        /// <param name="text">Text to speak</param>
        public void Speak(string text)
        {
            if (!_isInitialized || string.IsNullOrEmpty(text))
                return;
                
            try
            {
                _synthesizer?.Speak(text);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error speaking text: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Stops any current speech
        /// </summary>
        public void StopSpeaking()
        {
            if (_isInitialized && _isSpeaking)
            {
                _synthesizer?.SpeakAsyncCancelAll();
                _isSpeaking = false;
            }
        }
        
        /// <summary>
        /// Pauses the current speech
        /// </summary>
        public void PauseSpeaking()
        {
            if (_isInitialized && _isSpeaking)
            {
                _synthesizer?.Pause();
            }
        }
        
        /// <summary>
        /// Resumes paused speech
        /// </summary>
        public void ResumeSpeaking()
        {
            if (_isInitialized)
            {
                _synthesizer?.Resume();
            }
        }
        
        public void Dispose()
        {
            if (_synthesizer != null)
            {
                _synthesizer.SpeakAsyncCancelAll();
                _synthesizer!.Dispose();
                _synthesizer = null;
            }
            _isInitialized = false;
            _isSpeaking = false;
        }
    }
    
    /// <summary>
    /// Defines the priority of speech output
    /// </summary>
    public enum SpeechPriority
    {
        Low,
        Normal,
        High
    }
}
