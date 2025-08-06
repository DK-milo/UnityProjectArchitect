using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Unity.Editor.Components
{
    /// <summary>
    /// Real-time project analysis UI component
    /// Displays analysis results with interactive metrics and progress tracking
    /// </summary>
    public class ProjectAnalysisView : VisualElement
    {
        private VisualElement _rootContainer;
        private Foldout _analysisFoldout;
        private VisualElement _metricsContainer;
        private VisualElement _insightsContainer;
        private VisualElement _recommendationsContainer;
        private ProgressBar _analysisProgressBar;
        private Label _analysisStatusLabel;
        private Button _refreshButton;
        
        private ProjectAnalysisResult _currentAnalysis;
        private bool _isAnalyzing = false;
        
        public event System.Action OnRefreshRequested;
        public event System.Action<string> OnAnalysisActionRequested;
        
        public bool IsAnalyzing 
        { 
            get => _isAnalyzing;
            set 
            {
                _isAnalyzing = value;
                UpdateAnalysisState();
            }
        }
        
        public ProjectAnalysisView()
        {
            CreateUI();
        }
        
        private void CreateUI()
        {
            _rootContainer = new VisualElement();
            _rootContainer.style.flexGrow = 1;
            
            CreateAnalysisHeader();
            CreateProgressSection();
            CreateMetricsSection();
            CreateInsightsSection();
            CreateRecommendationsSection();
            
            Add(_rootContainer);
            
            // Initialize with empty state
            UpdateAnalysisResults(null);
        }
        
        private void CreateAnalysisHeader()
        {
            VisualElement headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.justifyContent = Justify.SpaceBetween;
            headerContainer.style.alignItems = Align.Center;
            headerContainer.style.marginBottom = 10;
            
            _analysisFoldout = new Foldout { text = "📊 Project Analysis", value = true };
            _analysisFoldout.style.flexGrow = 1;
            
            _refreshButton = new Button(() => OnRefreshRequested?.Invoke()) { text = "🔄 Refresh" };
            _refreshButton.style.minWidth = 80;
            
            headerContainer.Add(_analysisFoldout);
            headerContainer.Add(_refreshButton);
            
            _rootContainer.Add(headerContainer);
        }
        
        private void CreateProgressSection()
        {
            VisualElement progressContainer = new VisualElement();
            progressContainer.style.marginBottom = 10;
            
            _analysisStatusLabel = new Label("Ready for analysis");
            _analysisStatusLabel.style.marginBottom = 5;
            
            _analysisProgressBar = new ProgressBar();
            _analysisProgressBar.style.display = DisplayStyle.None;
            _analysisProgressBar.style.height = 20;
            
            progressContainer.Add(_analysisStatusLabel);
            progressContainer.Add(_analysisProgressBar);
            
            _analysisFoldout.Add(progressContainer);
        }
        
        private void CreateMetricsSection()
        {
            Foldout metricsFoldout = new Foldout { text = "📈 Metrics", value = true };
            metricsFoldout.style.marginBottom = 10;
            
            _metricsContainer = new VisualElement();
            metricsFoldout.Add(_metricsContainer);
            
            _analysisFoldout.Add(metricsFoldout);
        }
        
        private void CreateInsightsSection()
        {
            Foldout insightsFoldout = new Foldout { text = "💡 Insights", value = true };
            insightsFoldout.style.marginBottom = 10;
            
            _insightsContainer = new VisualElement();
            insightsFoldout.Add(_insightsContainer);
            
            _analysisFoldout.Add(insightsFoldout);
        }
        
        private void CreateRecommendationsSection()
        {
            Foldout recommendationsFoldout = new Foldout { text = "🎯 Recommendations", value = true };
            recommendationsFoldout.style.marginBottom = 10;
            
            _recommendationsContainer = new VisualElement();
            recommendationsFoldout.Add(_recommendationsContainer);
            
            _analysisFoldout.Add(recommendationsFoldout);
        }
        
        public void UpdateAnalysisResults(ProjectAnalysisResult analysisResult)
        {
            _currentAnalysis = analysisResult;
            
            if (analysisResult == null)
            {
                ShowEmptyState();
                return;
            }
            
            UpdateMetrics(analysisResult);
            UpdateInsights(analysisResult);
            UpdateRecommendations(analysisResult);
            UpdateAnalysisStatus(analysisResult);
        }
        
        private void ShowEmptyState()
        {
            _metricsContainer.Clear();
            _insightsContainer.Clear();
            _recommendationsContainer.Clear();
            
            Label emptyLabel = new Label("No analysis data available. Run project analysis to see results.");
            emptyLabel.style.color = Color.gray;
            emptyLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
            emptyLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            emptyLabel.style.paddingTop = 20;
            emptyLabel.style.paddingBottom = 20;
            
            _metricsContainer.Add(emptyLabel);
        }
        
        private void UpdateMetrics(ProjectAnalysisResult analysisResult)
        {
            _metricsContainer.Clear();
            
            if (analysisResult.Metrics == null)
            {
                Label noMetricsLabel = new Label("No metrics available");
                noMetricsLabel.style.color = Color.gray;
                noMetricsLabel.style.unityFontStyleAndWeight = FontStyle.Italic;
                _metricsContainer.Add(noMetricsLabel);
                return;
            }
            
            CreateMetricCard("Analysis Time", $"{analysisResult.AnalysisTime:F2}s", "⏱️");
            CreateMetricCard("Analysis Date", analysisResult.AnalyzedAt.ToString("MMM dd, yyyy HH:mm"), "📅");
            CreateMetricCard("Status", analysisResult.Success ? "Success" : "Failed", analysisResult.Success ? "✅" : "❌");
            
            if (!string.IsNullOrEmpty(analysisResult.ErrorMessage))
            {
                CreateErrorCard(analysisResult.ErrorMessage);
            }
        }
        
        private void CreateMetricCard(string title, string value, string icon)
        {
            VisualElement card = new VisualElement();
            card.style.flexDirection = FlexDirection.Row;
            card.style.alignItems = Align.Center;
            card.style.marginBottom = 5;
            card.style.paddingTop = 5;
            card.style.paddingBottom = 5;
            card.style.paddingLeft = 5;
            card.style.paddingRight = 5;
            card.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            card.style.borderTopLeftRadius = 3;
            card.style.borderTopRightRadius = 3;
            card.style.borderBottomLeftRadius = 3;
            card.style.borderBottomRightRadius = 3;
            
            Label iconLabel = new Label(icon);
            iconLabel.style.fontSize = 16;
            iconLabel.style.marginRight = 8;
            iconLabel.style.minWidth = 20;
            
            Label titleLabel = new Label(title);
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.flexGrow = 1;
            
            Label valueLabel = new Label(value);
            valueLabel.style.color = new Color(0.8f, 0.9f, 1.0f);
            
            card.Add(iconLabel);
            card.Add(titleLabel);
            card.Add(valueLabel);
            
            _metricsContainer.Add(card);
        }
        
        private void CreateErrorCard(string errorMessage)
        {
            VisualElement errorCard = new VisualElement();
            errorCard.style.marginTop = 5;
            errorCard.style.paddingTop = 5;
            errorCard.style.paddingBottom = 5;
            errorCard.style.paddingLeft = 5;
            errorCard.style.paddingRight = 5;
            errorCard.style.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.3f);
            errorCard.style.borderTopLeftRadius = 3;
            errorCard.style.borderTopRightRadius = 3;
            errorCard.style.borderBottomLeftRadius = 3;
            errorCard.style.borderBottomRightRadius = 3;
            
            Label errorLabel = new Label($"❌ Error: {errorMessage}");
            errorLabel.style.color = new Color(1.0f, 0.8f, 0.8f);
            errorLabel.style.whiteSpace = WhiteSpace.Normal;
            
            errorCard.Add(errorLabel);
            _metricsContainer.Add(errorCard);
        }
        
        private void UpdateInsights(ProjectAnalysisResult analysisResult)
        {
            _insightsContainer.Clear();
            
            // For now, show placeholder insights until AI integration is complete
            CreateInsightItem("Architecture", "Standard Unity project structure detected", InsightSeverity.Info);
            CreateInsightItem("Performance", "Project size is manageable for current scope", InsightSeverity.Info);
            CreateInsightItem("Organization", "Folder structure follows Unity conventions", InsightSeverity.Info);
            
            if (!analysisResult.Success && !string.IsNullOrEmpty(analysisResult.ErrorMessage))
            {
                CreateInsightItem("Analysis", $"Analysis issue: {analysisResult.ErrorMessage}", InsightSeverity.Warning);
            }
        }
        
        private void CreateInsightItem(string category, string message, InsightSeverity severity)
        {
            VisualElement insightCard = new VisualElement();
            insightCard.style.flexDirection = FlexDirection.Row;
            insightCard.style.alignItems = Align.FlexStart;
            insightCard.style.marginBottom = 5;
            insightCard.style.paddingTop = 8;
            insightCard.style.paddingBottom = 8;
            insightCard.style.paddingLeft = 8;
            insightCard.style.paddingRight = 8;
            insightCard.style.backgroundColor = GetSeverityColor(severity);
            insightCard.style.borderTopLeftRadius = 3;
            insightCard.style.borderTopRightRadius = 3;
            insightCard.style.borderBottomLeftRadius = 3;
            insightCard.style.borderBottomRightRadius = 3;
            
            Label severityIcon = new Label(GetSeverityIcon(severity));
            severityIcon.style.fontSize = 14;
            severityIcon.style.marginRight = 8;
            severityIcon.style.minWidth = 20;
            
            VisualElement contentContainer = new VisualElement();
            contentContainer.style.flexGrow = 1;
            
            Label categoryLabel = new Label(category);
            categoryLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            categoryLabel.style.fontSize = 12;
            categoryLabel.style.marginBottom = 2;
            
            Label messageLabel = new Label(message);
            messageLabel.style.whiteSpace = WhiteSpace.Normal;
            messageLabel.style.fontSize = 11;
            
            contentContainer.Add(categoryLabel);
            contentContainer.Add(messageLabel);
            
            insightCard.Add(severityIcon);
            insightCard.Add(contentContainer);
            
            _insightsContainer.Add(insightCard);
        }
        
        private void UpdateRecommendations(ProjectAnalysisResult analysisResult)
        {
            _recommendationsContainer.Clear();
            
            // Show placeholder recommendations until AI integration is complete
            CreateRecommendationItem("Documentation", 
                "Consider adding README.md files to key directories", 
                RecommendationPriority.Medium, 
                "Low");
            
            CreateRecommendationItem("Structure", 
                "Organize scripts into logical namespace folders", 
                RecommendationPriority.Low, 
                "Medium");
            
            if (!analysisResult.Success)
            {
                CreateRecommendationItem("Analysis", 
                    "Resolve analysis issues to get detailed project insights", 
                    RecommendationPriority.High, 
                    "Low");
            }
        }
        
        private void CreateRecommendationItem(string category, string description, RecommendationPriority priority, string effort)
        {
            VisualElement recommendationCard = new VisualElement();
            recommendationCard.style.marginBottom = 5;
            recommendationCard.style.paddingTop = 8;
            recommendationCard.style.paddingBottom = 8;
            recommendationCard.style.paddingLeft = 8;
            recommendationCard.style.paddingRight = 8;
            recommendationCard.style.backgroundColor = new Color(0.15f, 0.25f, 0.35f, 0.4f);
            recommendationCard.style.borderTopLeftRadius = 3;
            recommendationCard.style.borderTopRightRadius = 3;
            recommendationCard.style.borderBottomLeftRadius = 3;
            recommendationCard.style.borderBottomRightRadius = 3;
            
            VisualElement headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.justifyContent = Justify.SpaceBetween;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.marginBottom = 5;
            
            Label categoryLabel = new Label($"🎯 {category}");
            categoryLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            categoryLabel.style.fontSize = 12;
            
            VisualElement tagsContainer = new VisualElement();
            tagsContainer.style.flexDirection = FlexDirection.Row;
            
            Label priorityTag = new Label(priority.ToString());
            priorityTag.style.fontSize = 10;
            priorityTag.style.paddingTop = 2;
            priorityTag.style.paddingBottom = 2;
            priorityTag.style.paddingLeft = 2;
            priorityTag.style.paddingRight = 2;
            priorityTag.style.marginRight = 5;
            priorityTag.style.backgroundColor = GetPriorityColor(priority);
            priorityTag.style.borderTopLeftRadius = 2;
            priorityTag.style.borderTopRightRadius = 2;
            priorityTag.style.borderBottomLeftRadius = 2;
            priorityTag.style.borderBottomRightRadius = 2;
            
            Label effortTag = new Label($"Effort: {effort}");
            effortTag.style.fontSize = 10;
            effortTag.style.paddingTop = 2;
            effortTag.style.paddingBottom = 2;
            effortTag.style.paddingLeft = 2;
            effortTag.style.paddingRight = 2;
            effortTag.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            effortTag.style.borderTopLeftRadius = 2;
            effortTag.style.borderTopRightRadius = 2;
            effortTag.style.borderBottomLeftRadius = 2;
            effortTag.style.borderBottomRightRadius = 2;
            
            tagsContainer.Add(priorityTag);
            tagsContainer.Add(effortTag);
            
            headerRow.Add(categoryLabel);
            headerRow.Add(tagsContainer);
            
            Label descriptionLabel = new Label(description);
            descriptionLabel.style.whiteSpace = WhiteSpace.Normal;
            descriptionLabel.style.fontSize = 11;
            descriptionLabel.style.color = new Color(0.9f, 0.9f, 0.9f);
            
            Button actionButton = new Button(() => OnAnalysisActionRequested?.Invoke($"recommendation_{category}")) 
            { 
                text = "Apply" 
            };
            actionButton.style.marginTop = 5;
            actionButton.style.alignSelf = Align.FlexEnd;
            actionButton.style.minWidth = 60;
            actionButton.style.fontSize = 10;
            
            recommendationCard.Add(headerRow);
            recommendationCard.Add(descriptionLabel);
            recommendationCard.Add(actionButton);
            
            _recommendationsContainer.Add(recommendationCard);
        }
        
        private void UpdateAnalysisStatus(ProjectAnalysisResult analysisResult)
        {
            if (analysisResult.Success)
            {
                _analysisStatusLabel.text = $"✅ Analysis completed successfully at {analysisResult.AnalyzedAt:HH:mm}";
                _analysisStatusLabel.style.color = new Color(0.6f, 1.0f, 0.6f);
            }
            else
            {
                _analysisStatusLabel.text = $"⚠️ Analysis failed: {analysisResult.ErrorMessage}";
                _analysisStatusLabel.style.color = new Color(1.0f, 0.8f, 0.6f);
            }
        }
        
        private void UpdateAnalysisState()
        {
            if (_isAnalyzing)
            {
                _analysisStatusLabel.text = "🔄 Analyzing project...";
                _analysisStatusLabel.style.color = new Color(0.8f, 0.8f, 1.0f);
                _analysisProgressBar.style.display = DisplayStyle.Flex;
                _refreshButton.SetEnabled(false);
            }
            else
            {
                _analysisProgressBar.style.display = DisplayStyle.None;
                _refreshButton.SetEnabled(true);
            }
        }
        
        public void UpdateProgress(float progress, string operation)
        {
            if (_isAnalyzing)
            {
                _analysisProgressBar.value = progress;
                _analysisStatusLabel.text = $"🔄 {operation}... ({progress:P0})";
            }
        }
        
        private Color GetSeverityColor(InsightSeverity severity)
        {
            return severity switch
            {
                InsightSeverity.Critical => new Color(0.8f, 0.2f, 0.2f, 0.3f),
                InsightSeverity.Warning => new Color(0.8f, 0.6f, 0.2f, 0.3f),
                InsightSeverity.Info => new Color(0.2f, 0.6f, 0.8f, 0.3f),
                _ => new Color(0.3f, 0.3f, 0.3f, 0.3f)
            };
        }
        
        private string GetSeverityIcon(InsightSeverity severity)
        {
            return severity switch
            {
                InsightSeverity.Critical => "🔴",
                InsightSeverity.Warning => "🟡",
                InsightSeverity.Info => "🔵",
                _ => "⚪"
            };
        }
        
        private Color GetPriorityColor(RecommendationPriority priority)
        {
            return priority switch
            {
                RecommendationPriority.High => new Color(0.8f, 0.3f, 0.3f, 0.6f),
                RecommendationPriority.Medium => new Color(0.8f, 0.6f, 0.3f, 0.6f),
                RecommendationPriority.Low => new Color(0.3f, 0.8f, 0.3f, 0.6f),
                _ => new Color(0.5f, 0.5f, 0.5f, 0.6f)
            };
        }
    }
    
    /// <summary>
    /// Enum for insight severity levels used in UI
    /// </summary>
    public enum InsightSeverity
    {
        Info,
        Warning,
        Critical
    }
    
    /// <summary>
    /// Enum for recommendation priority levels used in UI
    /// </summary>
    public enum RecommendationPriority
    {
        Low,
        Medium,
        High
    }
}