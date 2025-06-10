using System;
using System.Drawing;

namespace RoastMyCode
{
    /// <summary>
    /// Manages theme colors and typography for the application
    /// </summary>
    public static class ThemeManager
    {
        // Modern minimal theme colors - Dark Mode
        public static class DarkTheme
        {
            // Primary colors
            public static readonly Color Background = Color.FromArgb(30, 30, 34);
            public static readonly Color Surface = Color.FromArgb(40, 40, 45);
            public static readonly Color SurfaceVariant = Color.FromArgb(50, 50, 55);
            public static readonly Color Primary = Color.FromArgb(130, 170, 255);
            public static readonly Color PrimaryVariant = Color.FromArgb(100, 140, 230);
            
            // Text colors
            public static readonly Color TextPrimary = Color.FromArgb(240, 240, 245);
            public static readonly Color TextSecondary = Color.FromArgb(190, 190, 200);
            public static readonly Color TextDisabled = Color.FromArgb(130, 130, 140);
            
            // UI element colors
            public static readonly Color InputBackground = Color.FromArgb(45, 45, 50);
            public static readonly Color Border = Color.FromArgb(70, 70, 75);
            public static readonly Color Divider = Color.FromArgb(60, 60, 65);
            public static readonly Color ButtonBackground = Color.FromArgb(55, 55, 60);
            public static readonly Color ButtonHover = Color.FromArgb(65, 65, 70);
            
            // Accent colors
            public static readonly Color Accent1 = Color.FromArgb(130, 170, 255); // Blue
            public static readonly Color Accent2 = Color.FromArgb(255, 150, 100); // Orange
            public static readonly Color Accent3 = Color.FromArgb(130, 230, 160); // Green
            public static readonly Color Accent4 = Color.FromArgb(230, 130, 170); // Pink
            
            // Status colors
            public static readonly Color Success = Color.FromArgb(100, 210, 150);
            public static readonly Color Warning = Color.FromArgb(255, 190, 90);
            public static readonly Color Error = Color.FromArgb(255, 110, 110);
            public static readonly Color Info = Color.FromArgb(100, 180, 255);
        }
        
        // Modern minimal theme colors - Light Mode
        public static class LightTheme
        {
            // Primary colors
            public static readonly Color Background = Color.FromArgb(248, 248, 250);
            public static readonly Color Surface = Color.FromArgb(255, 255, 255);
            public static readonly Color SurfaceVariant = Color.FromArgb(240, 240, 245);
            public static readonly Color Primary = Color.FromArgb(70, 110, 200);
            public static readonly Color PrimaryVariant = Color.FromArgb(50, 90, 180);
            
            // Text colors
            public static readonly Color TextPrimary = Color.FromArgb(30, 30, 35);
            public static readonly Color TextSecondary = Color.FromArgb(90, 90, 95);
            public static readonly Color TextDisabled = Color.FromArgb(150, 150, 155);
            
            // UI element colors
            public static readonly Color InputBackground = Color.FromArgb(245, 245, 250);
            public static readonly Color Border = Color.FromArgb(220, 220, 225);
            public static readonly Color Divider = Color.FromArgb(230, 230, 235);
            public static readonly Color ButtonBackground = Color.FromArgb(240, 240, 245);
            public static readonly Color ButtonHover = Color.FromArgb(230, 230, 235);
            
            // Accent colors
            public static readonly Color Accent1 = Color.FromArgb(70, 110, 200); // Blue
            public static readonly Color Accent2 = Color.FromArgb(230, 110, 60); // Orange
            public static readonly Color Accent3 = Color.FromArgb(60, 170, 100); // Green
            public static readonly Color Accent4 = Color.FromArgb(190, 80, 120); // Pink
            
            // Status colors
            public static readonly Color Success = Color.FromArgb(60, 180, 120);
            public static readonly Color Warning = Color.FromArgb(230, 150, 40);
            public static readonly Color Error = Color.FromArgb(220, 70, 70);
            public static readonly Color Info = Color.FromArgb(60, 140, 210);
        }
        
        // Typography
        public static class Typography
        {
            // Font families
            public const string PrimaryFontFamily = "Segoe UI";
            public const string MonospaceFontFamily = "Cascadia Code";
            
            // Font sizes
            public const float HeadingSize = 16f;
            public const float SubheadingSize = 14f;
            public const float BodySize = 11f;
            public const float SmallSize = 9f;
            
            // Font styles
            public static readonly Font Heading = new Font(PrimaryFontFamily, HeadingSize, FontStyle.Bold);
            public static readonly Font Subheading = new Font(PrimaryFontFamily, SubheadingSize, FontStyle.Bold);
            public static readonly Font Body = new Font(PrimaryFontFamily, BodySize, FontStyle.Regular);
            public static readonly Font BodyBold = new Font(PrimaryFontFamily, BodySize, FontStyle.Bold);
            public static readonly Font Small = new Font(PrimaryFontFamily, SmallSize, FontStyle.Regular);
            public static readonly Font Code = new Font(MonospaceFontFamily, BodySize, FontStyle.Regular);
            
            // Get a font with specific size and style
            public static Font GetFont(float size, FontStyle style = FontStyle.Regular, string fontFamily = PrimaryFontFamily)
            {
                return new Font(fontFamily, size, style);
            }
        }
        
        // Get current theme colors based on dark mode setting
        public static Color GetColor(bool isDarkMode, Func<Color> darkColor, Func<Color> lightColor)
        {
            return isDarkMode ? darkColor() : lightColor();
        }
    }
}
