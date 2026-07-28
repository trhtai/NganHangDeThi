using NganHangDeThi.ViewModels.KhoaPage;
using NganHangDeThi.ViewModels.Subjects;
using NganHangDeThi.Views.MonHocPage;
using System.Windows;

namespace NganHangDeThi.Views.KhoaPage;

public partial class ChinhSuaKhoaDialogView : Window
{
    private readonly ChinhSuaKhoaViewModel _viewModel;
    private bool _closingFromViewModel;
    private bool _isClosing;

    public ChinhSuaKhoaDialogView(ChinhSuaKhoaViewModel vm)
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

        Loaded += (_, _) => TenKhoaTextBox.Focus();

        Closing += (_, _) =>
        {
            _isClosing = true;

            if (!_closingFromViewModel)
            {
                _viewModel.CancelCommand.Execute(null);
            }
        };
    }

    public static bool? ShowDialog(Window owner, ChinhSuaKhoaViewModel viewModel)
    {
        var window = new ChinhSuaKhoaDialogView(viewModel) { Owner = owner };
        window.ShowDialog();

        return viewModel.DialogResult;
    }
}
