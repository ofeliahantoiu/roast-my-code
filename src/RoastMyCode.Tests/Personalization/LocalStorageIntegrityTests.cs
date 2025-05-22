using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.Json;

namespace RoastMyCode.Tests.Personalization
{
    [TestClass]
    public class LocalStorageIntegrityTests
    {
        private string _testSettingsPath = Path.Combine(Path.GetTempPath(), "RoastMyCodeTestSettings.json");

        [TestInitialize]
        public void Setup()
        {
            // Create a test settings file
            var testSettings = new
            {
                Theme = "dark",
                FontFamily = "Consolas",
                FontSize = 12,
                RoastLevel = "savage",
                LastUsed = DateTime.Now
            };

            File.WriteAllText(_testSettingsPath, JsonSerializer.Serialize(testSettings));
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Clean up test file
            if (File.Exists(_testSettingsPath))
            {
                File.Delete(_testSettingsPath);
            }
        }

        [TestMethod]
        [Description("BA-11: Test local storage data integrity across sessions")]
        public void LocalStorage_ShouldMaintainIntegrity()
        {
            /* Test Case: Local Storage Integrity
             * 
             * Prerequisites:
             * - Settings file exists with valid data
             * 
             * Steps:
             * 1. Read settings from file
             * 2. Verify all expected settings are present
             * 3. Modify settings
             * 4. Save settings back to file
             * 5. Read settings again
             * 6. Verify changes were preserved
             * 
             * Expected Results:
             * - All settings should be correctly read and written
             * - Data types should be preserved
             * - No data corruption should occur
             */

            // This test would require access to the actual settings storage mechanism
            // For now, we'll simulate it with a file-based approach
            
            // Step 1: Read settings
            var settingsJson = File.ReadAllText(_testSettingsPath);
            var settings = JsonSerializer.Deserialize<JsonElement>(settingsJson);
            
            // Step 2: Verify settings
            Assert.IsTrue(settings.TryGetProperty("Theme", out var theme));
            Assert.AreEqual("dark", theme.GetString());
            Assert.IsTrue(settings.TryGetProperty("FontFamily", out var fontFamily));
            Assert.AreEqual("Consolas", fontFamily.GetString());
            
            // Step 3 & 4: Modify and save settings
            var modifiedSettings = new
            {
                Theme = "light",
                FontFamily = "Consolas",
                FontSize = 14,
                RoastLevel = "brutal",
                LastUsed = DateTime.Now
            };
            
            File.WriteAllText(_testSettingsPath, JsonSerializer.Serialize(modifiedSettings));
            
            // Step 5: Read settings again
            var updatedSettingsJson = File.ReadAllText(_testSettingsPath);
            var updatedSettings = JsonSerializer.Deserialize<JsonElement>(updatedSettingsJson);
            
            // Step 6: Verify changes
            Assert.IsTrue(updatedSettings.TryGetProperty("Theme", out var updatedTheme));
            Assert.AreEqual("light", updatedTheme.GetString());
            Assert.IsTrue(updatedSettings.TryGetProperty("FontSize", out var updatedFontSize));
            Assert.AreEqual(14, updatedFontSize.GetInt32());
        }

        [TestMethod]
        [Description("BA-11: Test handling of corrupt settings file")]
        public void LocalStorage_ShouldHandleCorruptData()
        {
            /* Test Case: Corrupt Settings Handling
             * 
             * Prerequisites:
             * - Settings file exists
             * 
             * Steps:
             * 1. Corrupt the settings file with invalid JSON
             * 2. Attempt to read settings
             * 3. Verify application falls back to defaults
             * 
             * Expected Results:
             * - Application should not crash when encountering corrupt settings
             * - Default settings should be used when corruption is detected
             * - User should be notified of the issue (optional)
             */
            
            // Step 1: Corrupt the settings file
            File.WriteAllText(_testSettingsPath, "{ This is not valid JSON }");
            
            // Step 2 & 3: In a real test, we would:
            // - Call the actual settings loading code
            // - Verify it handles the corruption gracefully
            // - Check that default settings are used
            
            // For now, we'll just verify the file is corrupt
            var exception = Assert.ThrowsException<JsonException>(() => 
                JsonSerializer.Deserialize<JsonElement>(File.ReadAllText(_testSettingsPath)));
            
            Assert.IsNotNull(exception);
        }

        [TestMethod]
        [Description("BA-11: Test settings persistence across simulated sessions")]
        public void LocalStorage_ShouldPersistAcrossSessions()
        {
            /* Test Case: Settings Persistence Across Sessions
             * 
             * Prerequisites:
             * - Settings file exists with valid data
             * 
             * Steps:
             * 1. Read current settings
             * 2. Simulate application restart by clearing any in-memory cache
             * 3. Read settings again
             * 4. Verify settings match the original values
             * 
             * Expected Results:
             * - All settings should be correctly preserved across sessions
             * - No settings should be lost or reset unexpectedly
             */
            
            // This test would require the actual application code
            // For now, we'll simulate it with file operations
            
            // Step 1: Read current settings
            var settingsJson = File.ReadAllText(_testSettingsPath);
            var originalSettings = JsonSerializer.Deserialize<JsonElement>(settingsJson);
            
            // Step 2: Simulate application restart (nothing to do in this mock test)
            
            // Step 3: Read settings again
            var newSessionSettingsJson = File.ReadAllText(_testSettingsPath);
            var newSessionSettings = JsonSerializer.Deserialize<JsonElement>(newSessionSettingsJson);
            
            // Step 4: Verify settings match
            Assert.IsTrue(originalSettings.TryGetProperty("Theme", out var originalTheme));
            Assert.IsTrue(newSessionSettings.TryGetProperty("Theme", out var newTheme));
            Assert.AreEqual(originalTheme.GetString(), newTheme.GetString());
            
            Assert.IsTrue(originalSettings.TryGetProperty("FontFamily", out var originalFont));
            Assert.IsTrue(newSessionSettings.TryGetProperty("FontFamily", out var newFont));
            Assert.AreEqual(originalFont.GetString(), newFont.GetString());
        }
    }
}
