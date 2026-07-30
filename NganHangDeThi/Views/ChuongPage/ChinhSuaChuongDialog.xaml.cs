using NganHangDeThi.ViewModels.Chapters;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace NganHangDeThi.Views.ChuongPage;

public partial class ChinhSuaChuongDialog : Window
{
    private readonly ChapterEditViewModel _viewModel;
    private bool _closingFromViewModel;
    private bool _isClosing;

    public ChinhSuaChuongDialog(ChapterEditViewModel vm)
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

        Loaded += (_, _) => TenChuongTextBox.Focus();

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

    // Chỉ cho phép nhập số cho ô Thứ tự chương.
    private void ThuTuTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
    }

    /// <summary>Hiển thị modal và trả về true nếu người dùng đã lưu thành công.</summary>
    public static bool? ShowDialog(Window owner, ChapterEditViewModel viewModel)
    {
        var window = new ChinhSuaChuongDialog(viewModel) { Owner = owner };
        window.ShowDialog();

        return viewModel.DialogResult;
    }
}