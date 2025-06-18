using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace RoastMyCode.Controls
{
    public class SyntaxHighlightingService
    {
        private readonly Dictionary<string, IHighlightingDefinition> _highlightingDefinitions;
        
        public SyntaxHighlightingService()
        {
            _highlightingDefinitions = new Dictionary<string, IHighlightingDefinition>(StringComparer.OrdinalIgnoreCase);
            
            // Initialize built-in highlighting definitions
            InitializeBuiltInDefinitions();
        }
        
        /// <summary>
        /// Gets a highlighting definition for the specified language
        /// </summary>
        public IHighlightingDefinition? GetDefinition(string language)
        {
            if (string.IsNullOrEmpty(language))
                return null;
                
            // Normalize language name
            language = language.ToLowerInvariant().Trim();
            
            // Check if we already have this definition
            if (_highlightingDefinitions.TryGetValue(language, out var definition))
                return definition;
                
            // Try to load from built-in resources
            definition = TryLoadBuiltInDefinition(language);
            if (definition != null)
            {
                _highlightingDefinitions[language] = definition;
                return definition;
            }
            
            // Try to load from AvalonEdit's built-in definitions
            try
            {
                definition = HighlightingManager.Instance.GetDefinition(language);
                if (definition != null)
                {
                    _highlightingDefinitions[language] = definition;
                    return definition;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading highlighting definition for {language}: {ex.Message}");
            }
            
            return null; // Return null when no definition is found
        }
        
        /// <summary>
        /// Initialize built-in highlighting definitions
        /// </summary>
        private void InitializeBuiltInDefinitions()
        {
            // Register common languages
            RegisterBuiltInDefinition("csharp");
            RegisterBuiltInDefinition("javascript");
            RegisterBuiltInDefinition("typescript");
            RegisterBuiltInDefinition("html");
            RegisterBuiltInDefinition("xml");
            RegisterBuiltInDefinition("css");
            RegisterBuiltInDefinition("php");
            RegisterBuiltInDefinition("python");
            RegisterBuiltInDefinition("java");
            RegisterBuiltInDefinition("sql");
            RegisterBuiltInDefinition("json");
            RegisterBuiltInDefinition("markdown");
            RegisterBuiltInDefinition("ruby");
            RegisterBuiltInDefinition("go");
            RegisterBuiltInDefinition("rust");
            RegisterBuiltInDefinition("swift");
            RegisterBuiltInDefinition("kotlin");
        }
        
        /// <summary>
        /// Register a built-in highlighting definition
        /// </summary>
        private void RegisterBuiltInDefinition(string language)
        {
            var definition = TryLoadBuiltInDefinition(language);
            if (definition != null)
            {
                _highlightingDefinitions[language] = definition;
            }
        }
        
        /// <summary>
        /// Try to load a built-in highlighting definition
        /// </summary>
        private IHighlightingDefinition? TryLoadBuiltInDefinition(string language)
        {
            try
            {
                // Try to get from AvalonEdit's built-in definitions first
                var definition = HighlightingManager.Instance.GetDefinition(language);
                if (definition != null)
                    return definition;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading built-in highlighting definition for {language}: {ex.Message}");
            }
            
            // If not available from AvalonEdit, try to load from custom resources
            try
            {
                // Map language to resource name
                string? resourceName = MapLanguageToResourceName(language);
                if (string.IsNullOrEmpty(resourceName))
                    return null;
                
                // Try to load from embedded resources
                using (Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (XmlReader reader = XmlReader.Create(stream))
                        {
                            return HighlightingLoader.Load(reader, HighlightingManager.Instance);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading custom highlighting definition for {language}: {ex.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Map language name to resource name
        /// </summary>
        private string? MapLanguageToResourceName(string language)
        {
            // This would map to embedded resources if we had custom definitions
            // For now, we'll rely on AvalonEdit's built-in definitions
            return null;
        }
    }
}
