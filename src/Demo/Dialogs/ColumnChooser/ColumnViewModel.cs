using Demo.ViewModels;

using Immutable.ProjectModel;

namespace Demo.Dialogs.ColumnChooser
{
    internal sealed class ColumnViewModel : ViewModel
    {
        private bool _isSelected;

        public ColumnViewModel(FieldDefinition field)
        {
            Field = field;
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public FieldDefinition Field { get; }

        public string Name => Field.Name;

        public override string ToString() => Name;
    }
}
