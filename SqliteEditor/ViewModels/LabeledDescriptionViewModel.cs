using Reactive.Bindings;

namespace SqliteEditor.ViewModels
{
    public class LabeledDescriptionViewModel : ReactiveProperty<string>, IPropertyViewModel
    {
        public LabeledDescriptionViewModel(string label)
        {
            Label = label;
        }

        public string Label { get; }
        public ReactiveProperty<bool> IsVisible { get; } = new(true);
    }
}
