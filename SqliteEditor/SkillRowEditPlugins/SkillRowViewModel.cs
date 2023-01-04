using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteEditor.ViewModels
{
    public class SkillRowViewModel : CompositeDisposableBase
    {
        private readonly DataRow _source;
        private readonly TableViewModel _tableViewModel;

        public SkillRowViewModel(DataRow source, TableViewModel tableViewModel)
        {
            _source = source;
            _tableViewModel = tableViewModel;
            _ = UpdateCommand.Subscribe(() =>
            {
                WriteBackToSource();
            }).AddTo(Disposable);

            SyncFromSource();
        }

        public ReactiveProperty<string> Name { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> Description { get; } = new ReactiveProperty<string>();

        public ReactiveCommand UpdateCommand { get; } = new ReactiveCommand();

        private void SyncFromSource()
        {
            Name.Value = (string)_source["name"];
            Description.Value = (string)_source["description"];
        }

        private void WriteBackToSource()
        {
            _source["name"] = Name.Value;
            _tableViewModel.UpdateDirty();
        }
    }
}
