using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace ManaxClient;

public class SortedObservableCollection<T>(IEnumerable<T> collection) : ObservableCollection<T>(collection)
{
    private Func<T,object>? _sortingSelector;
    public Func<T, object>? SortingSelector
    {
        get => _sortingSelector;
        set
        {
            _sortingSelector = value;
            Sort();
        }
    }

    private bool _descending;

    public bool Descending
    {
        get => _descending;
        set
        {
            _descending = value;
            Sort();
        }
    }

    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnCollectionChanged(e);
        if (e.Action is NotifyCollectionChangedAction.Remove or NotifyCollectionChangedAction.Reset) { return; }
        Sort();
    }

    private void Sort()
    {
        if (SortingSelector is null) { return; }
                      
        IEnumerable<(T Item, int Index)> query = this
            .Select((item, index) => (Item: item, Index: index));
        query = Descending
            ? query.OrderByDescending(tuple => SortingSelector(tuple.Item))
            : query.OrderBy(tuple => SortingSelector(tuple.Item));

        IEnumerable<(int OldIndex, int NewIndex)> map = query.Select((tuple, index) => (OldIndex:tuple.Index, NewIndex:index))
            .Where(o => o.OldIndex != o.NewIndex);

        using IEnumerator<(int OldIndex, int NewIndex)> enumerator = map.GetEnumerator();
        if (enumerator.MoveNext())
        {
            Move(enumerator.Current.OldIndex, enumerator.Current.NewIndex);
        }
    }
}