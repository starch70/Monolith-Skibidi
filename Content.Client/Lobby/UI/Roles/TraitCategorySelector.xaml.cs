using Content.Client.Stylesheets;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Lobby.UI.Roles
{
    public sealed partial class TraitCategorySelector : Control
    {
        private readonly string _categoryId;
        private readonly string _categoryName;
        private readonly int? _maxTraitPoints;
        private readonly BoxContainer _traitsContainer;
        private readonly Label? _countLabel;
        private readonly Collapsible _collapsible;
        private int _currentSelection = 0;

        // Track expanded state explicitly
        private bool _isExpanded = false;

        /// <summary>
        /// The ID of this trait category
        /// </summary>
        public string CategoryId => _categoryId;

        /// <summary>
        /// Whether this category is currently expanded
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                if (_traitsContainer != null)
                    _traitsContainer.Visible = _isExpanded;
            }
        }

        /// <summary>
        /// Creates a new trait category selector.
        /// </summary>
        /// <param name="categoryId">The ID of the category</param>
        /// <param name="categoryName">The localized name of the category</param>
        /// <param name="maxTraitPoints">The maximum number of trait points allowed in this category, or null if unlimited</param>
        public TraitCategorySelector(string categoryId, string categoryName, int? maxTraitPoints)
        {
            // Do NOT load XAML - create everything in code
            _categoryId = categoryId;
            _categoryName = categoryName;
            _maxTraitPoints = maxTraitPoints;

            // Set this control to stop mouse events
            MouseFilter = MouseFilterMode.Stop;

            // Use BoxContainer to hold everything vertically
            var mainContainer = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
                HorizontalExpand = true,
                MouseFilter = MouseFilterMode.Stop
            };

            // Create a simple button for the header
            var headerButton = new Button
            {
                Text = _categoryName,
                StyleClasses = { StyleBase.StyleClassLabelHeading },
                HorizontalExpand = true,
                TextAlign = Label.AlignMode.Left,
                MouseFilter = MouseFilterMode.Stop
            };

            // Create container for traits
            _traitsContainer = new BoxContainer
            {
                Orientation = BoxContainer.LayoutOrientation.Vertical,
                HorizontalExpand = true,
                Margin = new Thickness(10, 5, 0, 5),
                MouseFilter = MouseFilterMode.Stop
            };

            // Set initial visibility based on the expanded state
            _traitsContainer.Visible = IsExpanded;

            // Add counter label if needed
            if (maxTraitPoints != null && maxTraitPoints >= 0)
            {
                _countLabel = new Label
                {
                    Text = Loc.GetString("humanoid-profile-editor-trait-count-hint", ("current", _currentSelection), ("max", maxTraitPoints)),
                    FontColorOverride = Color.White,
                    HorizontalAlignment = HAlignment.Center,
                    Margin = new Thickness(10, 0, 10, 5),
                    MouseFilter = MouseFilterMode.Pass
                };
                _traitsContainer.AddChild(_countLabel);
            }

            // Add header and traits to main container
            mainContainer.AddChild(headerButton);
            mainContainer.AddChild(_traitsContainer);

            // Create a dummy collapsible just to satisfy the field requirement
            _collapsible = new Collapsible(new CollapsibleHeading(""), new CollapsibleBody());

            // Use standard button press event
            headerButton.OnPressed += OnHeaderButtonPressed;

            // Add the main container to this control
            AddChild(mainContainer);
        }

        /// <summary>
        /// Handle header button press
        /// </summary>
        private void OnHeaderButtonPressed(BaseButton.ButtonEventArgs args)
        {
            // Toggle expansion state
            IsExpanded = !IsExpanded;
        }

        /// <summary>
        /// Adds a trait selector to this category
        /// </summary>
        public void AddTrait(TraitPreferenceSelector selector)
        {
            // Make sure the selector stops mouse events
            selector.MouseFilter = MouseFilterMode.Stop;

            // Add the selector directly to the container
            _traitsContainer.AddChild(selector);

            // Calculate initial selection
            if (selector.Preference)
            {
                _currentSelection += selector.Cost;
            }

            // Update the count label
            UpdateCountLabel();

            // Subscribe to preference changes
            selector.PreferenceChanged += (preference) =>
            {
                // Just update the cost
                TraitPreferenceChanged(preference, selector.Cost);
            };
        }

        /// <summary>
        /// Handler for trait preference changes
        /// </summary>
        private void TraitPreferenceChanged(bool preference, int cost)
        {
            if (preference)
            {
                _currentSelection += cost;
            }
            else
            {
                _currentSelection -= cost;
            }

            UpdateCountLabel();
        }

        /// <summary>
        /// Updates the count label to show the current selection total
        /// </summary>
        private void UpdateCountLabel()
        {
            if (_countLabel != null && _maxTraitPoints != null)
            {
                _countLabel.Text = Loc.GetString("humanoid-profile-editor-trait-count-hint",
                    ("current", _currentSelection),
                    ("max", _maxTraitPoints));

                // Keep text white even when over the limit
                _countLabel.FontColorOverride = Color.White;
            }
        }

        /// <summary>
        /// Adds a progress bar to this category
        /// </summary>
        public void AddProgressBar(Control progressBarContainer)
        {
            if (_traitsContainer != null)
            {
                // In TraitCategorySelector constructor, _countLabel is added as the first child of _traitsContainer
                // We want to add our progress bar right after _countLabel (as the second child)
                // but before any trait selectors

                // We'll add the progress bar component at the beginning since we can't easily
                // manipulate the child ordering. This typically places it after the count label
                // which is usually the first child.
                _traitsContainer.AddChild(progressBarContainer);
            }
        }
    }
}
