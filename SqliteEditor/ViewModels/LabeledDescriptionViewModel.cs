using Reactive.Bindings;

namespace SqliteEditor.ViewModels
{
    public class LabeledDescriptionViewModel : ReactiveProperty<string>
    {
        public LabeledDescriptionViewModel(string label)
        {
            Label = label;
        }

        public string Label { get; }
    }
}
