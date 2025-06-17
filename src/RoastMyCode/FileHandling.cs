using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace RoastMyCode
{
    public partial class Form1 : Form
    {        
        private async void PbCameraIcon_Click(object? sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Multiselect = true;
                    string filter = "Supported Files|*" + string.Join(";*", _fileUploadOptions.AllowedExtensions) + 
                                 "|ZIP Archives|*.zip|All Files|*.*";
                    openFileDialog.Filter = filter;
                    openFileDialog.Title = $"Select Files to Upload (Max {_fileUploadOptions.MaxFileSizeMB}MB per file, {_fileUploadOptions.MaxTotalSizeMB}MB total)";
                    openFileDialog.CheckFileExists = true;
                    openFileDialog.CheckPathExists = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        List<string> fileContents = new List<string>();
                        var filesToProcess = new List<string>(openFileDialog.FileNames);
                        
                        if (!ValidateFiles(filesToProcess, out var validFiles, out var validationErrors))
                        {
                            foreach (var error in validationErrors.Take(5))
                            {
                                AddChatMessage(error, "system");
                            }
                            
                            if (validationErrors.Count > 5)
                            {
                                AddChatMessage($"... and {validationErrors.Count - 5} more files had issues", "system");
                            }
                            
                            if (validFiles.Count == 0)
                            {
                                return;
                            }
                            
                            filesToProcess = validFiles;
                        }
                        else
                        {
                            ResetUploads();
                        }
                        
                        foreach (string fileName in filesToProcess)
                        {
                            try 
                            {
                                if (Path.GetExtension(fileName).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                                {
                                    var (content, error) = ProcessZipFile(fileName);
                                    if (!string.IsNullOrEmpty(error))
                                    {
                                        fileContents.Add($"Error processing {Path.GetFileName(fileName)}: {error}");
                                    }
                                    else if (!string.IsNullOrEmpty(content))
                                    {
                                        fileContents.Add($"=== Contents of {Path.GetFileName(fileName)} ===\n{content}");
                                    }
                                }
                                else
                                {
                                    string content = File.ReadAllText(fileName);
                                    string displayName = Path.GetFileName(fileName);
                                    string language = DetectLanguage(fileName, content);
                                    
                                    if (!_uploadedFiles.ContainsKey(displayName))
                                    {
                                        _uploadedFiles[displayName] = content;
                                        fileContents.Add($"=== {displayName} ({language}) ===\n{content}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                fileContents.Add($"Error processing {Path.GetFileName(fileName)}: {ex.Message}");
                            }
                        }

                        if (fileContents.Count > 0)
                        {
                            string combinedContent = string.Join("\n\n", fileContents);
                            AddChatMessage(combinedContent, "user");
                            _conversationHistory.Add(new ChatMessage { Content = combinedContent, Role = "user" });
                            
                            if (_uploadedFiles.Count > 0)
                            {
                                ShowDownloadButton();
                                
                                // Trigger AI response
                                string aiResponse = await _aiService.ProcessFiles(_uploadedFiles, cmbRoastLevel.SelectedItem?.ToString() ?? "light");
                                AddChatMessage(aiResponse, "assistant");
                                _conversationHistory.Add(new ChatMessage { Content = aiResponse, Role = "assistant" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddChatMessage($"Error during file upload: {ex.Message}", "system");
            }
        }

        private (string content, string error) ProcessZipFile(string zipPath)
        {
            string tempDir = string.Empty;
            string result = string.Empty;
            var processedFiles = new List<string>();
            var fileLanguages = new Dictionary<string, string>();
            var zipFileInfo = new FileInfo(zipPath);
            
            if (zipFileInfo.Length > _fileUploadOptions.MaxFileSizeBytes)
            {
                return (string.Empty, $"ZIP file '{Path.GetFileName(zipPath)}' exceeds maximum size of {_fileUploadOptions.MaxFileSizeMB}MB");
            }

            long remainingSpace = _fileUploadOptions.MaxTotalSizeBytes - _currentTotalSizeBytes;
            if (zipFileInfo.Length > remainingSpace)
            {
                return (string.Empty, $"Extracting this ZIP would exceed the total size limit of {_fileUploadOptions.MaxTotalSizeMB}MB");
            }

            try
            {
                tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(tempDir);
                   
                using (var archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!entry.FullName.EndsWith("/")) 
                        {
                            string fileName = Path.GetFileName(entry.FullName);
                            if (string.IsNullOrEmpty(fileName)) continue;
                            
                            string ext = Path.GetExtension(entry.Name).ToLowerInvariant();
                            if (!_fileUploadOptions.AllowedExtensions.Contains(ext) && !_languageMap.ContainsKey(fileName.ToLowerInvariant()))
                            {
                                return (string.Empty, $"ZIP contains disallowed file type: {entry.Name}");
                            }
                            
                            if (entry.Length > _fileUploadOptions.MaxFileSizeBytes)
                            {
                                return (string.Empty, $"ZIP contains file '{entry.Name}' that exceeds maximum size of {_fileUploadOptions.MaxFileSizeMB}MB");
                            }
                            
                            if (entry.Length > remainingSpace)
                            {
                                return (string.Empty, $"Extracting this ZIP would exceed the total size limit of {_fileUploadOptions.MaxTotalSizeMB}MB");
                            }
                            
                            remainingSpace -= entry.Length;
                        }
                    }
                    
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith("/")) continue; 
                        
                        string entryPath = Path.Combine(tempDir, entry.FullName);
                        string? directoryPath = Path.GetDirectoryName(entryPath);
                        
                        if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                        
                        entry.ExtractToFile(entryPath, true);
                    }
                }

                var codeFiles = Directory.GetFiles(tempDir, "*.*", SearchOption.AllDirectories)
                    .Where(f => {
                        string ext = Path.GetExtension(f).ToLowerInvariant();
                        string fileName = Path.GetFileName(f).ToLowerInvariant();
                        return _fileUploadOptions.AllowedExtensions.Contains(ext) || _languageMap.ContainsKey(fileName);
                    })
                    .OrderBy(f => f)
                    .ToList();

                if (codeFiles.Count == 0)
                {
                    return ($"No supported code files found in {Path.GetFileName(zipPath)}", string.Empty);
                }

                foreach (string file in codeFiles)
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        string displayName = Path.GetFileName(file);
                        string language = DetectLanguage(file, content);
                        
                        if (!_uploadedFiles.ContainsKey(displayName))
                        {
                            _uploadedFiles[displayName] = content;
                            processedFiles.Add(displayName);
                            fileLanguages[displayName] = language;
                        }
                    }
                    catch (Exception ex)
                    {
                        return (string.Empty, $"Error processing {Path.GetFileName(file)}: {ex.Message}");
                    }
                }

                if (processedFiles.Count > 0)
                {
                    result = string.Join("\n\n", processedFiles.Select(f => $"=== {f} ({fileLanguages[f]}) ===\n{_uploadedFiles[f]}"));
                }

                return (result, string.Empty);
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempDir) && Directory.Exists(tempDir))
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        private bool IsFileTypeAllowed(string fileName)
        {
            // Since we're allowing all file types now, we only need to check if it's a valid file
            return !string.IsNullOrEmpty(fileName) && File.Exists(fileName);
        }

        private bool IsFileSizeWithinLimit(string filePath, out string error)
        {
            error = string.Empty;
            try
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > _fileUploadOptions.MaxFileSizeBytes)
                {
                    error = $"File '{Path.GetFileName(filePath)}' exceeds maximum size of {_fileUploadOptions.MaxFileSizeMB}MB";
                    return false;
                }

                if (fileInfo.Length > (_fileUploadOptions.MaxTotalSizeBytes - _currentTotalSizeBytes))
                {
                    error = $"Adding this file would exceed the total size limit of {_fileUploadOptions.MaxTotalSizeMB}MB";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = $"Error checking file size: {ex.Message}";
                return false;
            }
        }

        private bool ValidateFiles(IEnumerable<string> filePaths, out List<string> validFiles, out List<string> errors)
        {
            validFiles = new List<string>();
            errors = new List<string>();

            foreach (string filePath in filePaths)
            {
                if (!IsFileTypeAllowed(filePath))
                {
                    errors.Add($"File type not allowed: {Path.GetFileName(filePath)}");
                    continue;
                }

                if (!IsFileSizeWithinLimit(filePath, out string error))
                {
                    errors.Add(error);
                    continue;
                }

                validFiles.Add(filePath);
            }

            return errors.Count == 0;
        }

        private void ResetUploads()
        {
            _uploadedFiles.Clear();
            _currentTotalSizeBytes = 0;
            HideDownloadButton();
        }

        private void DownloadCodeBundle()
        {
            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "ZIP Archive|*.zip";
                    saveFileDialog.Title = "Save Code Bundle";
                    saveFileDialog.FileName = "code_bundle.zip";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                        try
                        {
                            Directory.CreateDirectory(tempDir);

                            foreach (var file in _uploadedFiles)
                            {
                                string filePath = Path.Combine(tempDir, file.Key);
                                File.WriteAllText(filePath, file.Value);
                            }

                            if (File.Exists(saveFileDialog.FileName))
                            {
                                File.Delete(saveFileDialog.FileName);
                            }

                            ZipFile.CreateFromDirectory(tempDir, saveFileDialog.FileName);
                            AddChatMessage($"Code bundle saved to: {saveFileDialog.FileName}", "system");
                        }
                        finally
                        {
                            if (Directory.Exists(tempDir))
                            {
                                Directory.Delete(tempDir, true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddChatMessage($"Error saving code bundle: {ex.Message}", "system");
            }
        }

        private void ShowDownloadButton()
        {
            if (pbUploadIcon == null)
            {
                pbUploadIcon = new PictureBox
                {
                    Size = new Size(32, 32),
                    Location = new Point(pbCameraIcon.Location.X - 40, pbCameraIcon.Location.Y),
                    Cursor = Cursors.Hand,
                    Image = Image.FromFile(Path.Combine(AppContext.BaseDirectory, "assets", "download.png")),
                    SizeMode = PictureBoxSizeMode.StretchImage
                };
                pbUploadIcon.Click += (s, e) => DownloadCodeBundle();
                inputPanel.Controls.Add(pbUploadIcon);
            }
            pbUploadIcon.Visible = true;
        }

        private void HideDownloadButton()
        {
            if (pbUploadIcon != null)
            {
                pbUploadIcon.Visible = false;
            }
        }
    }
}