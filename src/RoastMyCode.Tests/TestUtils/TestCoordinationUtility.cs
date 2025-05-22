using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace RoastMyCode.Tests.TestUtils
{
    /// <summary>
    /// Utility class for coordinating internal testing feedback (BA-14)
    /// </summary>
    public class TestCoordinationUtility
    {
        private readonly string _feedbackStoragePath;
        
        public TestCoordinationUtility(string feedbackStoragePath)
        {
            _feedbackStoragePath = feedbackStoragePath;
            
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(_feedbackStoragePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        
        /// <summary>
        /// Submit feedback from an internal tester
        /// </summary>
        public async Task SubmitFeedbackAsync(TestFeedback feedback)
        {
            var feedbackList = await LoadFeedbackAsync();
            feedback.Id = GenerateId();
            feedback.SubmittedAt = DateTime.Now;
            feedbackList.Add(feedback);
            await SaveFeedbackAsync(feedbackList);
        }
        
        /// <summary>
        /// Get all submitted feedback
        /// </summary>
        public async Task<List<TestFeedback>> GetAllFeedbackAsync()
        {
            return await LoadFeedbackAsync();
        }
        
        /// <summary>
        /// Get feedback for a specific feature
        /// </summary>
        public async Task<List<TestFeedback>> GetFeedbackByFeatureAsync(string feature)
        {
            var allFeedback = await LoadFeedbackAsync();
            return allFeedback.FindAll(f => f.Feature.Equals(feature, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Get feedback by priority level
        /// </summary>
        public async Task<List<TestFeedback>> GetFeedbackByPriorityAsync(FeedbackPriority priority)
        {
            var allFeedback = await LoadFeedbackAsync();
            return allFeedback.FindAll(f => f.Priority == priority);
        }
        
        /// <summary>
        /// Get unresolved feedback items
        /// </summary>
        public async Task<List<TestFeedback>> GetUnresolvedFeedbackAsync()
        {
            var allFeedback = await LoadFeedbackAsync();
            return allFeedback.FindAll(f => !f.IsResolved);
        }
        
        /// <summary>
        /// Mark feedback as resolved
        /// </summary>
        public async Task ResolveFeedbackAsync(string feedbackId, string resolution)
        {
            var feedbackList = await LoadFeedbackAsync();
            var feedback = feedbackList.Find(f => f.Id == feedbackId);
            
            if (feedback != null)
            {
                feedback.IsResolved = true;
                feedback.Resolution = resolution;
                feedback.ResolvedAt = DateTime.Now;
                await SaveFeedbackAsync(feedbackList);
            }
        }
        
        /// <summary>
        /// Generate a summary report of all feedback
        /// </summary>
        public async Task<FeedbackSummary> GenerateSummaryReportAsync()
        {
            var allFeedback = await LoadFeedbackAsync();
            
            var summary = new FeedbackSummary
            {
                TotalFeedbackCount = allFeedback.Count,
                ResolvedCount = allFeedback.Count(f => f.IsResolved),
                UnresolvedCount = allFeedback.Count(f => !f.IsResolved),
                HighPriorityCount = allFeedback.Count(f => f.Priority == FeedbackPriority.High),
                MediumPriorityCount = allFeedback.Count(f => f.Priority == FeedbackPriority.Medium),
                LowPriorityCount = allFeedback.Count(f => f.Priority == FeedbackPriority.Low),
                FeedbackByFeature = new Dictionary<string, int>()
            };
            
            // Count feedback by feature
            foreach (var feedback in allFeedback)
            {
                if (summary.FeedbackByFeature.ContainsKey(feedback.Feature))
                {
                    summary.FeedbackByFeature[feedback.Feature]++;
                }
                else
                {
                    summary.FeedbackByFeature[feedback.Feature] = 1;
                }
            }
            
            return summary;
        }
        
        private async Task<List<TestFeedback>> LoadFeedbackAsync()
        {
            if (!File.Exists(_feedbackStoragePath))
            {
                return new List<TestFeedback>();
            }
            
            try
            {
                var json = await File.ReadAllTextAsync(_feedbackStoragePath);
                return JsonSerializer.Deserialize<List<TestFeedback>>(json) ?? new List<TestFeedback>();
            }
            catch (Exception)
            {
                // If there's an error reading the file, return an empty list
                return new List<TestFeedback>();
            }
        }
        
        private async Task SaveFeedbackAsync(List<TestFeedback> feedbackList)
        {
            var json = JsonSerializer.Serialize(feedbackList, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_feedbackStoragePath, json);
        }
        
        private string GenerateId()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8);
        }
    }
    
    /// <summary>
    /// Represents feedback from an internal tester
    /// </summary>
    public class TestFeedback
    {
        public string Id { get; set; } = string.Empty;
        public string TesterName { get; set; } = string.Empty;
        public string Feature { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public FeedbackPriority Priority { get; set; } = FeedbackPriority.Medium;
        public string Environment { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public bool IsResolved { get; set; }
        public string Resolution { get; set; } = string.Empty;
        public DateTime? ResolvedAt { get; set; }
        public List<string> Screenshots { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// Priority levels for test feedback
    /// </summary>
    public enum FeedbackPriority
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    /// <summary>
    /// Summary of all feedback
    /// </summary>
    public class FeedbackSummary
    {
        public int TotalFeedbackCount { get; set; }
        public int ResolvedCount { get; set; }
        public int UnresolvedCount { get; set; }
        public int HighPriorityCount { get; set; }
        public int MediumPriorityCount { get; set; }
        public int LowPriorityCount { get; set; }
        public Dictionary<string, int> FeedbackByFeature { get; set; } = new Dictionary<string, int>();
    }
}
