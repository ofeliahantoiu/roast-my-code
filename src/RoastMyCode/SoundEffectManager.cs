using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Diagnostics;

namespace RoastMyCode
{
    /// <summary>
    /// Manages sound effects for the application
    /// </summary>
    public class SoundEffectManager : IDisposable
    {
        private Dictionary<string, SoundPlayer> _soundPlayers;
        private bool _soundEffectsEnabled;
        private string _assetsPath;

        /// <summary>
        /// Gets or sets whether sound effects are enabled
        /// </summary>
        public bool SoundEffectsEnabled
        {
            get => _soundEffectsEnabled;
            set => _soundEffectsEnabled = value;
        }

        /// <summary>
        /// Initializes a new instance of the SoundEffectManager class
        /// </summary>
        public SoundEffectManager()
        {
            _soundPlayers = new Dictionary<string, SoundPlayer>();
            _soundEffectsEnabled = false;
            
            // Get the assets path
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _assetsPath = Path.Combine(baseDirectory, "Assets", "Sounds");
            
            // Create the directory if it doesn't exist
            if (!Directory.Exists(_assetsPath))
            {
                Directory.CreateDirectory(_assetsPath);
            }
            
            // Load available sound effects
            LoadSoundEffects();
        }

        /// <summary>
        /// Loads available sound effects from the Assets/Sounds directory
        /// </summary>
        private void LoadSoundEffects()
        {
            try
            {
                // Define default sound effects
                var defaultSounds = new Dictionary<string, string>
                {
                    { "roast_mild", "roast_mild.wav" },
                    { "roast_medium", "roast_medium.wav" },
                    { "roast_severe", "roast_severe.wav" },
                    { "notification", "notification.wav" },
                    { "success", "success.wav" },
                    { "error", "error.wav" }
                };

                // Load each sound effect
                foreach (var sound in defaultSounds)
                {
                    string soundPath = Path.Combine(_assetsPath, sound.Value);
                    if (File.Exists(soundPath))
                    {
                        try
                        {
                            var player = new SoundPlayer(soundPath);
                            player.LoadAsync();
                            _soundPlayers.Add(sound.Key, player);
                            Debug.WriteLine($"Loaded sound effect: {sound.Key}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error loading sound effect {sound.Key}: {ex.Message}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Sound file not found: {soundPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading sound effects: {ex.Message}");
            }
        }

        /// <summary>
        /// Plays a sound effect by name
        /// </summary>
        /// <param name="soundName">Name of the sound effect to play</param>
        public void PlaySound(string soundName)
        {
            if (!_soundEffectsEnabled)
                return;

            if (_soundPlayers.TryGetValue(soundName, out SoundPlayer player))
            {
                try
                {
                    player.Play();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error playing sound effect {soundName}: {ex.Message}");
                }
            }
            else
            {
                Debug.WriteLine($"Sound effect not found: {soundName}");
            }
        }

        /// <summary>
        /// Plays a sound effect based on roast severity
        /// </summary>
        /// <param name="severity">Severity level (1-5)</param>
        public void PlayRoastSound(int severity)
        {
            if (!_soundEffectsEnabled)
                return;

            string soundName;
            if (severity <= 2)
                soundName = "roast_mild";
            else if (severity <= 4)
                soundName = "roast_medium";
            else
                soundName = "roast_severe";

            PlaySound(soundName);
        }

        /// <summary>
        /// Disposes resources used by the SoundEffectManager
        /// </summary>
        public void Dispose()
        {
            foreach (var player in _soundPlayers.Values)
            {
                player.Dispose();
            }
            _soundPlayers.Clear();
        }
    }
}
