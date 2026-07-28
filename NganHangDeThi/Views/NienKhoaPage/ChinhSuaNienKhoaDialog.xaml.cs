using NganHangDeThi.ViewModels.NienKhoaPage;
using System.Windows;

namespace NganHangDeThi.Views.NienKhoaPage;

public partial class ChinhSuaNienKhoaDialog : Window
{
    private readonly ChinhSuaNienKhoaViewModel _viewModel;
    private bool _closingFromViewModel;
    private bool _isClosing;

    public ChinhSuaNienKhoaDialog(ChinhSuaNienKhoaViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        DataContext = vm;

        vm.RequestClose += () =>
        {
            _closingFromViewModel = true;

            if (!_isClosing)
            {
                Dispatcher.Invoke(Close);
            }
        };

        Loaded += (_, _) => TenNienKhoaTextBox.Focus();

        Closing += (_, _) =>
        {
            _isClosing = true;

            if (!_closingFromViewModel)
            {
                _viewModel.CancelCommand.Execute(null);
            }
        };
    }

    public static bool? ShowDialog(Window owner, ChinhSuaNienKhoaViewModel viewModel)
    {
        var window = new ChinhSuaNienKhoaDialog(viewModel) { Owner = owner };
        window.ShowDialog();

        return viewModel.DialogResult;
    }
}
