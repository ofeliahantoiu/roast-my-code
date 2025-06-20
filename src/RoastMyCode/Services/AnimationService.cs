using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RoastMyCode.Services
{
    public class AnimationService
    {
        // Singleton instance
        private static AnimationService? _instance;
        public static AnimationService Instance => _instance ??= new AnimationService();

        // Animation settings
        public bool AnimationsEnabled { get; set; } = true;
        
        // Shake intensity settings based on roast level
        private readonly Dictionary<string, int> _shakeIntensity = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["Light"] = 3,    // Mild shake
            ["Savage"] = 6,   // Medium shake
            ["Brutal"] = 10   // Intense shake
        };

        // Control references
        private Control? _targetControl;
        private List<Control> _animatedControls = new List<Control>();
        private Dictionary<Control, Point> _originalPositions = new Dictionary<Control, Point>();
        private Random _random = new Random();
        private bool _isAnimating = false;
        
        // Private constructor for singleton
        private AnimationService()
        {
        }
        
        /// <summary>
        /// Register the target control (like a Panel or Form) that contains all elements to be animated
        /// </summary>
        public void RegisterTarget(Control target)
        {
            _targetControl = target;
            
            // Clear any existing controls
            _animatedControls.Clear();
            _originalPositions.Clear();
        }
        
        /// <summary>
        /// Register an individual control to be animated
        /// </summary>
        public void RegisterAnimatedControl(Control control)
        {
            if (!_animatedControls.Contains(control))
            {
                _animatedControls.Add(control);
                _originalPositions[control] = control.Location;
            }
        }
        
        /// <summary>
        /// Trigger a shake animation with intensity based on the roast level
        /// </summary>
        public async Task ShakeAsync(string roastLevel)
        {
            // Don't animate if animations are disabled
            if (!AnimationsEnabled) return;
            
            // Don't start a new animation if one is already in progress
            if (_isAnimating) return;
            
            // Get the intensity based on roast level or use default
            int intensity = _shakeIntensity.TryGetValue(roastLevel, out int value) ? value : 5;
            
            await PerformShakeAnimationAsync(intensity);
        }
        
        /// <summary>
        /// Performs the actual shake animation
        /// </summary>
        private async Task PerformShakeAnimationAsync(int intensity)
        {
            _isAnimating = true;
            
            try
            {
                // Check if we have a valid target
                if (_targetControl == null) return;
                
                // If no specific controls are registered, animate the whole target
                if (_animatedControls.Count == 0)
                {
                    _animatedControls.Add(_targetControl);
                    _originalPositions[_targetControl] = _targetControl.Location;
                }
                
                // Calculate duration based on intensity (more intense = slightly longer duration)
                int durationMs = 350 + (intensity * 15); 
                int iterations = 10 + (intensity / 2);
                
                // Perform the shake animation
                for (int i = 0; i < iterations; i++)
                {
                    foreach (var control in _animatedControls)
                    {
                        if (control.IsDisposed) continue;
                        
                        // Original position from dictionary
                        Point original = _originalPositions[control];
                        
                        // Calculate random offset based on intensity
                        int offsetX = _random.Next(-intensity, intensity + 1);
                        int offsetY = _random.Next(-intensity, intensity + 1);
                        
                        // Apply the offset
                        control.Location = new Point(
                            original.X + offsetX,
                            original.Y + offsetY
                        );
                    }
                    
                    // Wait a small amount before next shake position
                    await Task.Delay(durationMs / iterations);
                }
                
                // Reset all controls to their original positions
                foreach (var control in _animatedControls)
                {
                    if (control.IsDisposed) continue;
                    control.Location = _originalPositions[control];
                }
            }
            finally
            {
                _isAnimating = false;
            }
        }
    }
}
