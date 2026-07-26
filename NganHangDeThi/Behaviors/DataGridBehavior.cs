using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace NganHangDeThi.Behaviors;

public class DataGridBehavior
{
    public static readonly DependencyProperty BindableSelectedItemsProperty =
        DependencyProperty.RegisterAttached(
            "BindableSelectedItems",
            typeof(IList),
            typeof(DataGridBehavior),
            new PropertyMetadata(null, OnBindableSelectedItemsChanged));

    public static void SetBindableSelectedItems(DependencyObject element, IList value) =>
        element.SetValue(BindableSelectedItemsProperty, value);

    public static IList GetBindableSelectedItems(DependencyObject element) =>
        (IList)element.GetValue(BindableSelectedItemsProperty);

    private static void OnBindableSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid grid) return;

        grid.SelectionChanged -= Grid_SelectionChanged;
        grid.SelectionChanged += Grid_SelectionChanged;
    }

    private static void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var grid = (DataGrid)sender;
        var boundList = GetBindableSelectedItems(grid);
        if (boundList == null) return;

        foreach (var removed in e.RemovedItems) boundList.Remove(removed);
        foreach (var added in e.AddedItems)
        {
            if (!boundList.Contains(added)) boundList.Add(added);
        }
    }
}
