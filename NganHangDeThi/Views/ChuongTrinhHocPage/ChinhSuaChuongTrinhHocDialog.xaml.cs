using NganHangDeThi.ViewModels.Curriculum;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace NganHangDeThi.Views.ChuongTrinhHocPage;

public partial class ChinhSuaChuongTrinhHocDialog : Window
{
    private readonly CurriculumEditViewModel _viewModel;
    private bool _closingFromViewModel;
    private bool _isClosing;

    public ChinhSuaChuongTrinhHocDialog(CurriculumEditViewModel vm)
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

        Loaded += (_, _) => MonHocComboBox.Focus();

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

    // Chỉ cho phép nhập số cho ô Năm học.
    private void NamHocTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
    }

    /// <summary>Hiển thị modal và trả về true nếu người dùng đã lưu thành công.</summary>
    public static bool? ShowDialog(Window owner, CurriculumEditViewModel viewModel)
    {
        var window = new ChinhSuaChuongTrinhHocDialog(viewModel) { Owner = owner };
        window.ShowDialog();

        return viewModel.DialogResult;
    }
}