namespace FocLauncher.Controls
{
    public partial class OptionsControl
    {
        public bool IsExpanded
        {
            get => Expander.IsExpanded;
            set => Expander.IsExpanded = value;
        }

        public OptionsControl()
        {
            InitializeComponent();
        }
    }
}
