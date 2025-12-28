using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using JsonFormatterApp.Models;

namespace JsonFormatterApp.Services
{
    public class FileService
    {
        private const string RecentFilesFileName = "recent_files.json";
        private const int MaxRecentFiles = 10;
        private readonly string _appDataPath;

        public FileService()
        {
            _appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "JsonFormatterPro"
            );

            Directory.CreateDirectory(_appDataPath);
        }

        /// <summary>
        /// Read file contents
        /// </summary>
        public string ReadFile(string filePath)
        {
            try
            {
                var content = File.ReadAllText(filePath);
                AddRecentFile(filePath);
                return content;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to read file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Write content to file
        /// </summary>
        public void WriteFile(string filePath, string content)
        {
            try
            {
                File.WriteAllText(filePath, content);
                AddRecentFile(filePath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to write file: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get recent files list
        /// </summary>
        public List<RecentFile> GetRecentFiles()
        {
            var recentFiles = new List<RecentFile>();
            var filePath = Path.Combine(_appDataPath, RecentFilesFileName);

            if (!File.Exists(filePath))
                return recentFiles;

            try
            {
                var json = File.ReadAllText(filePath);
                var deserializedFiles = JsonConvert.DeserializeObject<List<RecentFile>>(json);

                if (deserializedFiles != null)
                {
                    // Only include files that still exist
                    recentFiles = deserializedFiles
                        .Where(f => File.Exists(f.FilePath))
                        .OrderByDescending(f => f.LastAccessed)
                        .Take(MaxRecentFiles)
                        .ToList();
                }

                return recentFiles;
            }
            catch
            {
                // Return empty list if deserialization fails (corrupted file, old format, etc.)
                return recentFiles;
            }
        }

        /// <summary>
        /// Add file to recent files list
        /// </summary>
        private void AddRecentFile(string filePath)
        {
            try
            {
                var recentFiles = GetRecentFiles();
                var existing = recentFiles.FirstOrDefault(f => f.FilePath == filePath);

                if (existing != null)
                {
                    recentFiles.Remove(existing);
                }

                recentFiles.Insert(0, new RecentFile
                {
                    FilePath = filePath,
                    LastAccessed = DateTime.Now
                });

                // Keep only the max number of recent files
                var filesToSave = recentFiles.Take(MaxRecentFiles).ToList();

                var recentFilePath = Path.Combine(_appDataPath, RecentFilesFileName);
                var json = JsonConvert.SerializeObject(filesToSave, Formatting.Indented);
                File.WriteAllText(recentFilePath, json);
            }
            catch
            {
                // Silently fail if we can't update recent files
            }
        }

        /// <summary>
        /// Clear recent files list
        /// </summary>
        public void ClearRecentFiles()
        {
            var filePath = Path.Combine(_appDataPath, RecentFilesFileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
