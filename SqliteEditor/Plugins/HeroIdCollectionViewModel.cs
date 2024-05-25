using Reactive.Bindings;
using SqliteEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SqliteEditor.Plugins;

public record HeroInfo(int Id, string Name, string Epithet)
{
    public string DisplayName => $"{Name}({Epithet})";
}

public class HeroIdViewModel : CompositeDisposableBase
{
    private string _filterText = string.Empty;
    private string _id = string.Empty;

    public HeroIdViewModel()
    {
        FilteredHeroInfos = CollectionViewSource.GetDefaultView(HeroInfos);
        FilteredHeroInfos.Filter = FilterItems;
    }

    public string Id
    {
        get => _id;
        set
        {
            SetProperty(ref _id, value);
        }
    }

    public ReactiveProperty<int> SelectedIndex { get; } = new ReactiveProperty<int>(-1);

    public ObservableCollection<HeroInfo> HeroInfos { get; } = new();
    public ICollectionView FilteredHeroInfos { get; set; }

    public void SyncSelectedIndex()
    {
        var id = Id;
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        if (int.TryParse(id, out var idValue))
        {
            for (int index = 0; index < HeroInfos.Count; ++index)
            {
                var info = HeroInfos[index];
                if (info.Id == idValue)
                {
                    SelectedIndex.Value = index;
                    return;
                }
            }
        }
    }

    public string FilterText
    {
        get => _filterText;
        set
        {
            if (SetProperty(ref _filterText, value))
            {
                FilteredHeroInfos.Refresh();
            }
        }
    }

    private bool FilterItems(object obj)
    {
        if (obj is HeroInfo item)
        {
            return string.IsNullOrEmpty(FilterText) || item.DisplayName.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}

public class HeroIdCollectionViewModel : CompositeDisposableBase
{
    public HeroIdCollectionViewModel(string label)
    {
        Label = label;
    }

    public string Label { get; }

    public ObservableCollection<HeroIdViewModel> Values { get; } = new();

    public void AddHeroInfo(HeroInfo info)
    {
        foreach (var value in Values)
        {
            value.HeroInfos.Add(info);
        }
    }
    public void AddNewItemsWhile(Func<bool> condition)
    {
        while (condition())
        {
            var item = new HeroIdViewModel();
            Values.Add(item);
        }
    }

    public void SyncSelectedIndices()
    {
        foreach (var value in Values)
        {
            value.SyncSelectedIndex();
        }
    }
}
