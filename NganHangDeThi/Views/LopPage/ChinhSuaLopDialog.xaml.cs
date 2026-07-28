using NganHangDeThi.ViewModels.LopPage;
using System.Windows;

namespace NganHangDeThi.Views.LopPage;

public partial class ChinhSuaLopDialog : Window
{
    private readonly ChinhSuaLopViewModel _viewModel;
    private bool _closingFromViewModel;
    private bool _isClosing;

    public ChinhSuaLopDialog(ChinhSuaLopViewModel vm)
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

        Loaded += (_, _) => MaLopTextBox.Focus();

        Closing += (_, _) =>
        {
            _isClosing = true;

            if (!_closingFromViewModel)
            {
                _viewModel.CancelCommand.Execute(null);
            }
        };
    }

    public static bool? ShowDialog(Window owner, ChinhSuaLopViewModel viewModel)
    {
        var window = new ChinhSuaLopDialog(viewModel) { Owner = owner };
        window.ShowDialog();

        return viewModel.DialogResult;
    }
}