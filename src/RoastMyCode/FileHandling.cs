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
                        string relativePath = file.Substring(tempDir.Length).TrimStart(Path.DirectorySeparatorChar);
                        string language = DetectLanguage(relativePath, content);
                        fileLanguages[relativePath] = language;
                        
                        _uploadedFiles[relativePath] = content;
                        _currentTotalSizeBytes += new FileInfo(file).Length;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error processing {file}: {ex.Message}");
                    }
                }

                var possibleMains = codeFiles.Where(f => 
                    f.IndexOf("program", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    f.IndexOf("main", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    f.IndexOf("app", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    f.IndexOf("index", StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                var filesToProcess = possibleMains.Concat(codeFiles.Except(possibleMains));

                foreach (string file in filesToProcess)
                {
                    try
                    {
                        string relativePath = file.Substring(tempDir.Length).TrimStart(Path.DirectorySeparatorChar);
                        if (fileLanguages.TryGetValue(relativePath, out string? language) && 
                            _uploadedFiles.TryGetValue(relativePath, out string? content))
                        {
                            result += $"=== {relativePath} ({language}) ===\n{content}\n\n";
                            processedFiles.Add(relativePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        result += $"Error reading {Path.GetFileName(file)}: {ex.Message}\n\n";
                    }
                }

                
                if (processedFiles.Count == 0)
                {
                    return (string.Empty, "No valid code files could be extracted from the ZIP");
                }
                
                return (result, string.Empty);
            }
            catch (Exception ex)
            {
                foreach (var file in processedFiles)
                {
                    _uploadedFiles.Remove(file);
                }
                _currentTotalSizeBytes -= processedFiles.Sum(f => new FileInfo(Path.Combine(tempDir, f)).Length);
                
                return (string.Empty, $"Error processing ZIP file: {ex.Message}");
            }
            finally
            {
                try 
                { 
                    if (!string.IsNullOrEmpty(tempDir) && Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error cleaning up temp directory: {ex.Message}");
                }
            }
        }

        private bool IsFileTypeAllowed(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            return _fileUploadOptions.AllowedExtensions.Contains(extension) || 
                   _languageMap.ContainsKey(Path.GetFileName(fileName).ToLowerInvariant());
        }

        private bool IsFileSizeWithinLimit(string filePath, out string error)
        {
            error = string.Empty;
            var fileInfo = new FileInfo(filePath);
            
            if (fileInfo.Length > _fileUploadOptions.MaxFileSizeBytes)
            {
                error = $"File '{Path.GetFileName(filePath)}' exceeds maximum size of {_fileUploadOptions.MaxFileSizeMB}MB";
                return false;
            }

            if (_currentTotalSizeBytes + fileInfo.Length > _fileUploadOptions.MaxTotalSizeBytes)
            {
                error = $"Adding this file would exceed the total size limit of {_fileUploadOptions.MaxTotalSizeMB}MB";
                return false;
            }

            return true;
        }

        private bool ValidateFiles(IEnumerable<string> filePaths, out List<string> validFiles, out List<string> errors)
        {
            validFiles = new List<string>();
            errors = new List<string>();
            long totalSize = 0;

            foreach (var filePath in filePaths)
            {
                try
                {
                    if (!IsFileTypeAllowed(filePath))
                    {
                        errors.Add($"File type not allowed: {Path.GetFileName(filePath)}");
                        continue;
                    }

                    if (!IsFileSizeWithinLimit(filePath, out string sizeError))
                    {
                        errors.Add(sizeError);
                        continue;
                    }

                    var fileInfo = new FileInfo(filePath);
                    totalSize += fileInfo.Length;
                    validFiles.Add(filePath);
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing {Path.GetFileName(filePath)}: {ex.Message}");
                }
            }

            if (errors.Count == 0)
            {
                _currentTotalSizeBytes += totalSize;
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
            if (_uploadedFiles.Count == 0)
            {
                AddChatMessage("No files available to download.", "system");
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "ZIP Archive|*.zip";
                saveFileDialog.Title = "Save Code Bundle";
                saveFileDialog.FileName = $"code_bundle_{DateTime.Now:yyyyMMddHHmmss}.zip";
                
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                        Directory.CreateDirectory(tempDir);

                        try
                        {
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
                            
                            AddChatMessage($"Code bundle saved successfully: {Path.GetFileName(saveFileDialog.FileName)}", "system");
                        }
                        finally
                        {
                            try { Directory.Delete(tempDir, true); }
                            catch { /* Ignore cleanup errors */ }
                        }
                    }
                    catch (Exception ex)
                    {
                        AddChatMessage($"Error creating code bundle: {ex.Message}", "system");
                    }
                }
            }
        }

        private void ShowDownloadButton()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ShowDownloadButton));
                return;
            }

            var existingButton = this.Controls.OfType<Button>().FirstOrDefault(b => b.Name == "btnDownloadBundle");
            existingButton?.Dispose();

            var downloadButton = new Button
            {
                Name = "btnDownloadBundle",
                Text = "Download Code Bundle",
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                FlatAppearance = { BorderSize = 0 },
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                Padding = new Padding(15, 5, 15, 5),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(this.ClientSize.Width - 220, 10)
            };

            downloadButton.Click += (s, e) => DownloadCodeBundle();
            
            downloadButton.MouseEnter += (s, e) => {
                downloadButton.BackColor = Color.FromArgb(0, 100, 180);
            };
            downloadButton.MouseLeave += (s, e) => {
                downloadButton.BackColor = Color.FromArgb(0, 120, 215);
            };

            this.Controls.Add(downloadButton);
            downloadButton.BringToFront();
        }
        
        private void HideDownloadButton()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(HideDownloadButton));
                return;
            }

            var downloadButton = this.Controls.OfType<Button>().FirstOrDefault(b => b.Name == "btnDownloadBundle");
            if (downloadButton != null)
            {
                this.Controls.Remove(downloadButton);
                downloadButton.Dispose();
            }
        }
    }
}