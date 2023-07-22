using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DocumentScanningBlankApp;

public class ScannedDocumentTemplateSelector : DataTemplateSelector
{
    public DataTemplate ParentTemplate { get; set; }
    public DataTemplate ChildTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        if (item is ScannedDocumentModel node)
        {
            return node.IsParent ? this.ParentTemplate : this.ChildTemplate;
        }

        return base.SelectTemplateCore(item);
    }
}