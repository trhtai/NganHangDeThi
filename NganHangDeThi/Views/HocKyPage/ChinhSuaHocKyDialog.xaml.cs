using NganHangDeThi.ViewModels.Semesters;
using System.Windows;

namespace NganHangDeThi.Views.HocKyPage;

public partial class ChinhSuaHocKyDialog : Window
{
    private readonly SemesterEditViewModel _viewModel;
    private bool _closingFromViewModel;
    private bool _isClosing;

    public ChinhSuaHocKyDialog(SemesterEditViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        DataContext = vm;

        vm.RequestClose += () =>
        {
            _closingFromViewModel = true;

            // Nếu Window đã ở giữa quá trình đóng thì KHÔNG được gọi Close() lần nữa.
            if (!_isClosing)
            {
                Dispatcher.Invoke(Close);
            }
        };

        Loaded += (_, _) => TenHocKyTextBox.Focus();

        Closing += (_, _) =>
        {
            _isClosing = true;

            // Người dùng bấm nút [X] thay vì Hủy -> vẫn phải trả kết quả "hủy".
            if (!_closingFromViewModel)
            {
                _viewModel.CancelCommand.Execute(null);
            }
        };
    }

    /// <summary>Hiển thị modal và trả về true nếu người dùng đã lưu thành công.</summary>
    public static bool? ShowDialog(Window owner, SemesterEditViewModel viewModel)
    {
        var window = new ChinhSuaHocKyDialog(viewModel) { Owner = owner };
        window.ShowDialog();

        return viewModel.DialogResult;
    }
}