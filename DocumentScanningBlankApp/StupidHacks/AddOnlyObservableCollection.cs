using System.Collections.ObjectModel;
using System.ComponentModel;

namespace DocumentScanningBlankApp.StupidHacks;

using System.Collections.Generic;
using System.Collections.Specialized;

public class AddOnlyObservableCollection<T> : ObservableCollection<T> // FIXME: this has got to be the dumbest work around to a circular reference sir.
{

    public override event NotifyCollectionChangedEventHandler CollectionChanged;

    public AddOnlyObservableCollection() : base() { }
    public AddOnlyObservableCollection(List<T> list) : base(list) { }
    public AddOnlyObservableCollection(IEnumerable<T> collection) : base(collection) { }
    protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        // Only raise event if the action is Add
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            CollectionChanged?.Invoke(this, e);
        }
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
    }

}
