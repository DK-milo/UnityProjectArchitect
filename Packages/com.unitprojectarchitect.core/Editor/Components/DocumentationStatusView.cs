using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityProjectArchitect.Core;

namespace UnityProjectArchitect.Unity.Editor.Components
{
    /// <summary>
    /// Real-time documentation status UI component
    /// Shows documentation sections status with progress tracking and generation controls
    /// </summary>
    public class DocumentationStatusView : VisualElement
    {
        private VisualElement _rootContainer;
        private Foldout _documentationFoldout;
        private VisualElement _sectionsContainer;
        private ProgressBar _overallProgressBar;
        private Label _progressLabel;
        private Button _generateAllButton;
        private Button _exportButton;
        
        private List<DocumentationSectionData> _documentationSections;
        private bool _isGenerating = false;
        
        public event System.Action<DocumentationSectionData> OnSectionGenerateRequested;
        public event System.Action OnGenerateAllRequested;
        public event System.Action OnExportRequested;
        public event System.Action<DocumentationSectionData, bool> OnSectionEnabledChanged;
        
        public bool IsGenerating
        {
            get => _isGenerating;
            set
            {
                _isGenerating = value;
                UpdateGenerationState();
            }
        }
        
        public DocumentationStatusView()
        {
            CreateUI();
        }
        
        private void CreateUI()
        {
            _rootContainer = new VisualElement();
            _rootContainer.style.flexGrow = 1;
            
            CreateDocumentationHeader();
            CreateProgressSection();
            CreateSectionsContainer();
            CreateActionsSection();
            
            Add(_rootContainer);
            
            // Initialize with empty state
            UpdateDocumentationSections(null);
        }
        
        private void CreateDocumentationHeader()
        {
            VisualElement headerContainer = new VisualElement();
            headerContainer.style.flexDirection = FlexDirection.Row;
            headerContainer.style.justifyContent = Justify.SpaceBetween;
            headerContainer.style.alignItems = Align.Center;
            headerContainer.style.marginBottom = 10;
            
            _documentationFoldout = new Foldout { text = "📖 Documentation Status", value = true };
            _documentationFoldout.style.flexGrow = 1;
            
            _rootContainer.Add(headerContainer);
            _rootContainer.Add(_documentationFoldout);
        }
        
        private void CreateProgressSection()
        {
            VisualElement progressContainer = new VisualElement();
            progressContainer.style.marginBottom = 15;
            
            _progressLabel = new Label("Ready to generate documentation");
            _progressLabel.style.marginBottom = 5;
            _progressLabel.style.fontSize = 12;
            
            _overallProgressBar = new ProgressBar();
            _overallProgressBar.style.height = 20;
            _overallProgressBar.style.marginBottom = 5;
            
            Label progressDetailsLabel = new Label("Select sections and click Generate to start");
            progressDetailsLabel.style.fontSize = 10;
            progressDetailsLabel.style.color = Color.gray;
            
            progressContainer.Add(_progressLabel);
            progressContainer.Add(_overallProgressBar);
            progressContainer.Add(progressDetailsLabel);
            
            _documentationFoldout.Add(progressContainer);
        }
        
        private void CreateSectionsContainer()
        {
            _sectionsContainer = new VisualElement();
            _sectionsContainer.style.marginBottom = 15;
            
            _documentationFoldout.Add(_sectionsContainer);
        }
        
        private void CreateActionsSection()
        {
            VisualElement actionsContainer = new VisualElement();
            actionsContainer.style.flexDirection = FlexDirection.Row;
            actionsContainer.style.justifyContent = Justify.SpaceBetween;
            
            _generateAllButton = new Button(() => OnGenerateAllRequested?.Invoke()) 
            { 
                text = "🚀 Generate All Enabled" 
            };
            _generateAllButton.style.flexGrow = 1;
            _generateAllButton.style.marginRight = 5;
            
            _exportButton = new Button(() => OnExportRequested?.Invoke()) 
            { 
                text = "📤 Export" 
            };
            _exportButton.style.flexGrow = 1;
            
            actionsContainer.Add(_generateAllButton);
            actionsContainer.Add(_exportButton);
            
            _documentationFoldout.Add(actionsContainer);
        }
        
        public void UpdateDocumentationSections(List<DocumentationSectionData> sections)
        {
            _documentationSections = sections;
            
            if (sections == null || sections.Count == 0)
            {
                ShowEmptyState();
                return;
            }
            
            RefreshSectionsList();
            UpdateOverallProgress();
        }
        
        private void ShowEmptyState()
        {
            _sectionsContainer.Clear();
            
            Label emptyLabel = new Label("No documentation sections configured. Create a Project Data Asset to get started.");
            emptyLabel.style.color = Color.gray;
            emptyLabel.style.fontStyle = FontStyle.Italic;
            emptyLabel.style.textAlign = TextAnchor.MiddleCenter;
            emptyLabel.style.paddingTop = 20;
            emptyLabel.style.paddingBottom = 20;
            
            _sectionsContainer.Add(emptyLabel);
            
            _overallProgressBar.value = 0;
            _progressLabel.text = "No sections available";
            
            _generateAllButton.SetEnabled(false);
            _exportButton.SetEnabled(false);
        }
        
        private void RefreshSectionsList()
        {
            _sectionsContainer.Clear();
            
            foreach (DocumentationSectionData section in _documentationSections)
            {
                CreateSectionItem(section);
            }
        }
        
        private void CreateSectionItem(DocumentationSectionData section)
        {
            VisualElement sectionCard = new VisualElement();
            sectionCard.style.marginBottom = 8;
            sectionCard.style.padding = 10;
            sectionCard.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.4f);
            sectionCard.style.borderRadius = 5;
            sectionCard.style.borderBottomWidth = 2;
            sectionCard.style.borderBottomColor = GetStatusColor(section.Status);
            
            VisualElement headerRow = new VisualElement();
            headerRow.style.flexDirection = FlexDirection.Row;
            headerRow.style.alignItems = Align.Center;
            headerRow.style.marginBottom = 8;
            
            Toggle enabledToggle = new Toggle();
            enabledToggle.value = section.IsEnabled;
            enabledToggle.style.marginRight = 10;
            enabledToggle.RegisterValueChangedCallback(evt => {
                OnSectionEnabledChanged?.Invoke(section, evt.newValue);
            });
            
            Label sectionIcon = new Label(GetSectionIcon(section.SectionType));
            sectionIcon.style.fontSize = 16;
            sectionIcon.style.marginRight = 8;
            sectionIcon.style.minWidth = 20;
            
            Label sectionTitle = new Label(GetSectionDisplayName(section.SectionType));
            sectionTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            sectionTitle.style.flexGrow = 1;
            sectionTitle.style.fontSize = 13;
            
            Label statusLabel = new Label(GetStatusDisplayText(section.Status));
            statusLabel.style.fontSize = 11;
            statusLabel.style.color = GetStatusColor(section.Status);
            statusLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            statusLabel.style.minWidth = 80;
            statusLabel.style.textAlign = TextAnchor.MiddleRight;
            
            headerRow.Add(enabledToggle);
            headerRow.Add(sectionIcon);
            headerRow.Add(sectionTitle);
            headerRow.Add(statusLabel);
            
            VisualElement detailsRow = new VisualElement();
            detailsRow.style.flexDirection = FlexDirection.Row;
            detailsRow.style.justifyContent = Justify.SpaceBetween;
            detailsRow.style.alignItems = Align.Center;
            
            VisualElement statsContainer = new VisualElement();
            statsContainer.style.flexDirection = FlexDirection.Row;
            
            int wordCount = string.IsNullOrEmpty(section.Content) ? 0 : section.Content.Split(' ').Length;
            Label wordCountLabel = new Label($"{wordCount} words");
            wordCountLabel.style.fontSize = 10;
            wordCountLabel.style.color = Color.gray;
            wordCountLabel.style.marginRight = 15;
            
            string lastUpdated = section.LastUpdated.HasValue ? 
                section.LastUpdated.Value.ToString("MMM dd, HH:mm") : 
                "Never";
            Label lastUpdatedLabel = new Label($"Updated: {lastUpdated}");
            lastUpdatedLabel.style.fontSize = 10;
            lastUpdatedLabel.style.color = Color.gray;
            
            statsContainer.Add(wordCountLabel);
            statsContainer.Add(lastUpdatedLabel);
            
            Button generateButton = new Button(() => OnSectionGenerateRequested?.Invoke(section)) 
            { 
                text = "Generate" 
            };
            generateButton.style.minWidth = 80;
            generateButton.style.fontSize = 11;
            generateButton.SetEnabled(section.IsEnabled && !_isGenerating);
            
            if (section.Status == DocumentationStatus.Generated || section.Status == DocumentationStatus.Completed)
            {
                generateButton.text = "Regenerate";
            }
            
            detailsRow.Add(statsContainer);
            detailsRow.Add(generateButton);
            
            // Progress bar for individual section if generating
            if (_isGenerating)
            {
                ProgressBar sectionProgress = new ProgressBar();
                sectionProgress.style.height = 4;
                sectionProgress.style.marginTop = 5;
                sectionProgress.value = GetSectionProgress(section);
                
                sectionCard.Add(headerRow);
                sectionCard.Add(detailsRow);
                sectionCard.Add(sectionProgress);
            }
            else
            {
                sectionCard.Add(headerRow);
                sectionCard.Add(detailsRow);
            }
            
            _sectionsContainer.Add(sectionCard);
        }
        
        private void UpdateOverallProgress()
        {
            if (_documentationSections == null || _documentationSections.Count == 0)
            {
                _overallProgressBar.value = 0;
                _progressLabel.text = "No sections to track";
                return;
            }
            
            int totalEnabledSections = 0;
            int completedSections = 0;
            
            foreach (DocumentationSectionData section in _documentationSections)
            {
                if (section.IsEnabled)
                {
                    totalEnabledSections++;
                    if (section.Status == DocumentationStatus.Generated || 
                        section.Status == DocumentationStatus.Completed ||
                        section.Status == DocumentationStatus.Approved)
                    {
                        completedSections++;
                    }
                }
            }
            
            if (totalEnabledSections == 0)
            {
                _overallProgressBar.value = 0;
                _progressLabel.text = "No sections enabled";
            }
            else
            {
                float progress = (float)completedSections / totalEnabledSections;
                _overallProgressBar.value = progress;
                _progressLabel.text = $"Documentation Progress: {completedSections}/{totalEnabledSections} sections completed ({progress:P0})";
            }
        }
        
        private void UpdateGenerationState()
        {
            _generateAllButton.SetEnabled(!_isGenerating);
            _exportButton.SetEnabled(!_isGenerating);
            
            if (_isGenerating)
            {
                _progressLabel.text = "🔄 Generating documentation...";
            }
            else
            {
                UpdateOverallProgress();
            }
        }
        
        public void UpdateGenerationProgress(float progress, string currentSection)
        {
            if (_isGenerating)
            {
                _progressLabel.text = $"🔄 Generating {currentSection}... ({progress:P0})";
                // Update overall progress during generation
            }
        }
        
        private float GetSectionProgress(DocumentationSectionData section)
        {
            return section.Status switch
            {
                DocumentationStatus.NotStarted => 0f,
                DocumentationStatus.InProgress => 0.5f,
                DocumentationStatus.Generated => 1f,
                DocumentationStatus.Completed => 1f,
                DocumentationStatus.Approved => 1f,
                _ => 0f
            };
        }
        
        private string GetSectionIcon(SectionType sectionType)
        {
            return sectionType switch
            {
                SectionType.GeneralProductDescription => "📋",
                SectionType.SystemArchitecture => "🏗️",
                SectionType.DataModel => "🗄️",
                SectionType.APISpecification => "🔌",
                SectionType.UserStories => "👤",
                SectionType.WorkTickets => "🎫",
                _ => "📄"
            };
        }
        
        private string GetSectionDisplayName(SectionType sectionType)
        {
            return sectionType switch
            {
                SectionType.GeneralProductDescription => "General Description",
                SectionType.SystemArchitecture => "System Architecture",
                SectionType.DataModel => "Data Model",
                SectionType.APISpecification => "API Specification",
                SectionType.UserStories => "User Stories",
                SectionType.WorkTickets => "Work Tickets",
                _ => sectionType.ToString()
            };
        }
        
        private string GetStatusDisplayText(DocumentationStatus status)
        {
            return status switch
            {
                DocumentationStatus.NotStarted => "Not Started",
                DocumentationStatus.InProgress => "In Progress",
                DocumentationStatus.Generated => "Generated",
                DocumentationStatus.Completed => "Completed",
                DocumentationStatus.Reviewed => "Reviewed",
                DocumentationStatus.Approved => "Approved",
                DocumentationStatus.Published => "Published",
                DocumentationStatus.Outdated => "Outdated",
                _ => status.ToString()
            };
        }
        
        private Color GetStatusColor(DocumentationStatus status)
        {
            return status switch
            {
                DocumentationStatus.NotStarted => Color.gray,
                DocumentationStatus.InProgress => new Color(1f, 0.8f, 0.4f), // Orange
                DocumentationStatus.Generated => new Color(0.4f, 0.8f, 1f), // Light Blue
                DocumentationStatus.Completed => new Color(0.4f, 1f, 0.6f), // Light Green
                DocumentationStatus.Reviewed => new Color(0.6f, 0.8f, 1f), // Blue
                DocumentationStatus.Approved => new Color(0.2f, 1f, 0.4f), // Green
                DocumentationStatus.Published => new Color(0.8f, 0.4f, 1f), // Purple
                DocumentationStatus.Outdated => new Color(1f, 0.6f, 0.4f), // Red
                _ => Color.white
            };
        }
    }
}