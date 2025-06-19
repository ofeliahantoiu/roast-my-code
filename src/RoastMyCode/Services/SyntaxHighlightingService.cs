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
        private readonly Dictionary<string, string> _resourceMappings;
        
        // Static property to store the list of available resources (for debugging)
        public static List<string> AvailableResources { get; private set; } = new List<string>();
        
        public SyntaxHighlightingService()
        {
            _highlightingDefinitions = new Dictionary<string, IHighlightingDefinition>(StringComparer.OrdinalIgnoreCase);
            _resourceMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            // Initialize resource mappings
            InitializeResourceMappings();
            
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
        /// Initialize resource mappings
        /// </summary>
        private void InitializeResourceMappings()
        {
            // Get all available resources and map them automatically
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            System.Diagnostics.Debug.WriteLine("===== Available Resources =====\n" + string.Join("\n", resources) + "\n===============================\n");
            
            // Map resources based on name patterns
            foreach (var resource in resources)
            {
                if (resource.EndsWith(".xshd", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract language name from resource name
                    string fileName = Path.GetFileNameWithoutExtension(resource.Split('.').Last());
                    string languageName = fileName.ToLowerInvariant();
                    
                    System.Diagnostics.Debug.WriteLine($"Mapping language '{languageName}' to resource '{resource}'");
                    _resourceMappings[languageName] = resource;
                }
            }
            
            // Ensure we have standard mappings for common languages
            // Format: language name -> actual embedded resource name
            foreach (var resource in resources)
            {
                if (resource.IndexOf("java", StringComparison.OrdinalIgnoreCase) >= 0 && resource.EndsWith(".xshd", StringComparison.OrdinalIgnoreCase))
                    _resourceMappings["java"] = resource;
                
                if (resource.IndexOf("csharp", StringComparison.OrdinalIgnoreCase) >= 0 && resource.EndsWith(".xshd", StringComparison.OrdinalIgnoreCase))
                    _resourceMappings["csharp"] = resource;
                    
                if (resource.IndexOf("javascript", StringComparison.OrdinalIgnoreCase) >= 0 && resource.EndsWith(".xshd", StringComparison.OrdinalIgnoreCase))
                    _resourceMappings["javascript"] = resource;
                    
                if (resource.IndexOf("python", StringComparison.OrdinalIgnoreCase) >= 0 && resource.EndsWith(".xshd", StringComparison.OrdinalIgnoreCase))
                    _resourceMappings["python"] = resource;
                    
                if (resource.IndexOf("php", StringComparison.OrdinalIgnoreCase) >= 0 && resource.EndsWith(".xshd", StringComparison.OrdinalIgnoreCase))
                    _resourceMappings["php"] = resource;
            }
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
            // Debug message to trace the call
            System.Diagnostics.Debug.WriteLine($"Attempting to load syntax highlighting for '{language}'");
            
            // Store original language for logging
            string originalLanguage = language;
            
            // First time, collect all resources for debugging
            if (AvailableResources.Count == 0)
            {
                var allResources = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                AvailableResources.AddRange(allResources);
                
                System.Diagnostics.Debug.WriteLine("==== ALL EMBEDDED RESOURCES ====");
                foreach (var res in allResources)
                {
                    System.Diagnostics.Debug.WriteLine($"  {res}");
                }
                System.Diagnostics.Debug.WriteLine("================================");
            }
            
            // First try loading directly from our embedded resources
            try
            {
                // Try each potential resource name format
                List<string> possibleResourceNames = new List<string>
                {
                    $"RoastMyCode.Resources.{language}.xshd",
                    $"Resources.{language}.xshd",
                    $"RoastMyCode.Resources.{language.ToUpperInvariant()}.xshd",
                    $"{language}.xshd"
                };
                
                // Also try checking available resources that contain the language name
                foreach (var res in AvailableResources)
                {
                    if (res.IndexOf(language, StringComparison.OrdinalIgnoreCase) >= 0 && res.EndsWith(".xshd", StringComparison.OrdinalIgnoreCase))
                    {
                        possibleResourceNames.Add(res);
                    }
                }
                
                foreach (var resourceName in possibleResourceNames)
                {
                    System.Diagnostics.Debug.WriteLine($"Trying to load resource: {resourceName}");
                    
                    using (Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                    {
                        if (stream != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"SUCCESS: Found and loaded resource {resourceName}");
                            using (XmlReader reader = XmlReader.Create(stream))
                            {
                                var loadedDefinition = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                                System.Diagnostics.Debug.WriteLine($"Successfully loaded highlighting definition for '{originalLanguage}'");
                                return loadedDefinition;
                            }
                        }
                    }
                }
                
                // If we get here, we couldn't find a matching resource, try AvalonEdit's built-in definitions
                System.Diagnostics.Debug.WriteLine($"No custom definition found for {originalLanguage}, trying AvalonEdit's built-in definitions");
                var definition = HighlightingManager.Instance.GetDefinition(language);
                if (definition != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Found built-in AvalonEdit definition for '{originalLanguage}'");
                    return definition;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading highlighting definition for {originalLanguage}: {ex.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Map language name to resource name
        /// </summary>
        private string? MapLanguageToResourceName(string language)
        {
            // Debug the language name we're trying to map
            System.Diagnostics.Debug.WriteLine($"Mapping language: '{language}' to resource name");
            
            // Direct mapping based on the actual file names in Resources directory
            switch (language.ToLowerInvariant())
            {
                case "java":
                    return "RoastMyCode.Resources.Java.xshd";
                case "csharp":
                case "c#":
                case "cs":
                    return "RoastMyCode.Resources.CSharp.xshd";
                case "javascript":
                case "js":
                    return "RoastMyCode.Resources.JavaScript.xshd";
                case "python":
                case "py":
                    return "RoastMyCode.Resources.Python.xshd";
                case "php":
                    return "RoastMyCode.Resources.PHP.xshd";
                default:
                    // Check resource mappings as a fallback
                    if (_resourceMappings.TryGetValue(language, out var resourceName))
                    {
                        return resourceName;
                    }
                    return null;
            }
        }
    }
}
