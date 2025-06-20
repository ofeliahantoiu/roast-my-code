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
        
        // Track whether initialization was completed successfully
        private bool _isInitialized = false;
        
        public SyntaxHighlightingService()
        {
            _highlightingDefinitions = new Dictionary<string, IHighlightingDefinition>(StringComparer.OrdinalIgnoreCase);
            _resourceMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            try
            {
                // Initialize resource mappings
                InitializeResourceMappings();
                
                // Initialize built-in highlighting definitions
                InitializeBuiltInDefinitions();
                
                // Ensure critical language definitions are loaded 
                EnsureCriticalDefinitionsLoaded();
                
                _isInitialized = true;
                System.Diagnostics.Debug.WriteLine("Syntax highlighting service initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing syntax highlighting service: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            }
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
            
            // Detailed debug logging
            System.Diagnostics.Debug.WriteLine($"GetDefinition called for language: '{language}'");
            
            // Check if we already have this definition in cache
            if (_highlightingDefinitions.TryGetValue(language, out var definition))
            {
                System.Diagnostics.Debug.WriteLine($"Found cached definition for '{language}'");
                return definition;
            }
            
            // Special handling for specific languages that might be failing
            if (language == "rust" || language == "ruby" || language == "go" || language == "csharp" || language == "c#")
            {
                // Force reload for these languages
                System.Diagnostics.Debug.WriteLine($"Attempting special handling for '{language}'");
                definition = ForceLoadDefinition(language);
                if (definition != null)
                {
                    _highlightingDefinitions[language] = definition;
                    return definition;
                }
            }
                
            // Try to load from built-in resources
            System.Diagnostics.Debug.WriteLine($"Trying to load built-in definition for '{language}'");
            definition = TryLoadBuiltInDefinition(language);
            if (definition != null)
            {
                System.Diagnostics.Debug.WriteLine($"Successfully loaded built-in definition for '{language}'");
                _highlightingDefinitions[language] = definition;
                return definition;
            }
            
            // Try to load from AvalonEdit's built-in definitions
            try
            {
                System.Diagnostics.Debug.WriteLine($"Trying AvalonEdit built-in definition for '{language}'");
                definition = HighlightingManager.Instance.GetDefinition(language);
                if (definition != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Found AvalonEdit built-in definition for '{language}'");
                    _highlightingDefinitions[language] = definition;
                    return definition;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading highlighting definition for {language}: {ex.Message}");
            }
            
            System.Diagnostics.Debug.WriteLine($"No definition found for '{language}'");
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
                if (resource.IndexOf("java", StringComparison.OrdinalIgnoreCase) >= 0 && resource.EndsWith(".xshd", StringComparison.OrdinalIgnoreCase) && !resource.Contains("script", StringComparison.OrdinalIgnoreCase))
                    _resourceMappings["java"] = resource;
                
                if (resource.IndexOf("csharp", StringComparison.OrdinalIgnoreCase) >= 0 && resource.EndsWith(".xshd", StringComparison.OrdinalIgnoreCase))
                    _resourceMappings["csharp"] = resource;
                    
                // Also map C# directly as an alias to csharp
                if (resource.IndexOf("csharp", StringComparison.OrdinalIgnoreCase) >= 0 && resource.EndsWith(".xshd", StringComparison.OrdinalIgnoreCase))
                    _resourceMappings["c#"] = resource;
                    
                if (resource.IndexOf("javascript", StringComparison.OrdinalIgnoreCase) >= 0 && resource.EndsWith(".xshd", StringComparison.OrdinalIgnoreCase))
                    _resourceMappings["javascript"] = resource;
                    
                if (resource.IndexOf("python", StringComparison.OrdinalIgnoreCase) >= 0 && resource.EndsWith(".xshd", StringComparison.OrdinalIgnoreCase))
                    _resourceMappings["python"] = resource;
                    
                if (resource.IndexOf("php", StringComparison.OrdinalIgnoreCase) >= 0 && resource.EndsWith(".xshd", StringComparison.OrdinalIgnoreCase))
                    _resourceMappings["php"] = resource;
                    
                if (resource.IndexOf("ruby", StringComparison.OrdinalIgnoreCase) >= 0 && resource.EndsWith(".xshd", StringComparison.OrdinalIgnoreCase))
                    _resourceMappings["ruby"] = resource;
                    
                if (resource.IndexOf("rust", StringComparison.OrdinalIgnoreCase) >= 0 && resource.EndsWith(".xshd", StringComparison.OrdinalIgnoreCase))
                    _resourceMappings["rust"] = resource;
                    
                if (resource.IndexOf("go", StringComparison.OrdinalIgnoreCase) >= 0 && 
                    !resource.Contains("mongo", StringComparison.OrdinalIgnoreCase) && 
                    !resource.Contains("cargo", StringComparison.OrdinalIgnoreCase) && 
                    resource.EndsWith(".xshd", StringComparison.OrdinalIgnoreCase))
                    _resourceMappings["go"] = resource;
            }
            
            // Debug output of all registered mappings
            System.Diagnostics.Debug.WriteLine("===== Registered Language Mappings =====");
            foreach (var mapping in _resourceMappings)
            {
                System.Diagnostics.Debug.WriteLine($"Language '{mapping.Key}' -> Resource '{mapping.Value}'");
            }
            System.Diagnostics.Debug.WriteLine("=======================================");
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
        /// Ensure critical language definitions are loaded properly
        /// </summary>
        private void EnsureCriticalDefinitionsLoaded()
        {
            // These are the languages that need special handling
            string[] criticalLanguages = new[] { "csharp", "c#", "ruby", "rust", "go" };
            
            foreach (var language in criticalLanguages)
            {
                System.Diagnostics.Debug.WriteLine($"Ensuring critical definition for '{language}' is loaded");
                
                // Force load the definition
                var definition = ForceLoadDefinition(language);
                if (definition != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Successfully preloaded definition for '{language}'");
                    _highlightingDefinitions[language] = definition;
                    
                    // Add aliases for C#
                    if (language == "csharp")
                    {
                        _highlightingDefinitions["c#"] = definition;
                        _highlightingDefinitions["cs"] = definition;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to preload definition for '{language}'");
                }
            }
        }
        
        /// <summary>
        /// Force load a syntax definition for languages that need special handling
        /// </summary>
        private IHighlightingDefinition? ForceLoadDefinition(string language)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Force loading definition for '{language}'");
                
                // Use consistent names for xshd resources
                string resourceName;
                switch (language.ToLowerInvariant())
                {
                    case "csharp":
                    case "c#":
                    case "cs":
                        resourceName = "CSharp.xshd";
                        break;
                    case "ruby":
                    case "rb":
                        resourceName = "Ruby.xshd";
                        break;
                    case "rust":
                    case "rs":
                        resourceName = "Rust.xshd";
                        break;
                    case "go":
                    case "golang":
                        resourceName = "Go.xshd";
                        break;
                    default:
                        return null;
                }
                
                // Try multiple resource name formats
                string[] possiblePaths = {
                    $"RoastMyCode.Resources.{resourceName}",
                    $"Resources.{resourceName}",
                    resourceName,
                    $"d:\\Universitate\\AppDev\\roast-my-code\\src\\RoastMyCode\\Resources\\{resourceName}"
                };
                
                foreach (var path in possiblePaths)
                {
                    try
                    {
                        // Try embedded resources first
                        using (Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
                        {
                            if (stream != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"Found resource stream for '{path}'");
                                using (XmlReader reader = XmlReader.Create(stream))
                                {
                                    return HighlightingLoader.Load(reader, HighlightingManager.Instance);
                                }
                            }
                        }
                        
                        // If not found as embedded resource, try loading directly from file
                        if (File.Exists(path))
                        {
                            System.Diagnostics.Debug.WriteLine($"Loading definition from file: '{path}'");
                            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                            using (XmlReader reader = XmlReader.Create(fs))
                            {
                                return HighlightingLoader.Load(reader, HighlightingManager.Instance);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error trying path {path}: {ex.Message}");
                        // Continue to try the next path
                    }
                }
                
                // Final attempt - try to load from AvalonEdit's built-in definitions
                return HighlightingManager.Instance.GetDefinition(language);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ForceLoadDefinition for {language}: {ex.Message}");
                return null;
            }
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
                case "ruby":
                case "rb":
                    return "RoastMyCode.Resources.Ruby.xshd";
                case "rust":
                case "rs":
                    return "RoastMyCode.Resources.Rust.xshd";
                case "go":
                case "golang":
                    return "RoastMyCode.Resources.Go.xshd";
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
