using Prism.Events;

namespace DocumentScanningBlankApp.Events;

public class GlobalEventHandler
{
    private static readonly IEventAggregator _eventAggregator = new EventAggregator();

    public static IEventAggregator Instance => _eventAggregator;
}