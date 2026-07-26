using NganHangDeThi.ViewModels.Subjects;
using System.Windows;

namespace NganHangDeThi.Views.MonHoc;

public partial class ChinhSuaMonHocDialogView : Window
{
    private readonly SubjectEditViewModel _viewModel;
    private bool _closingFromViewModel;
    private bool _isClosing;

    public ChinhSuaMonHocDialogView(SubjectEditViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        DataContext = vm;
        
        vm.RequestClose += () =>
        {
            _closingFromViewModel = true;

            // Nếu Window đã ở giữa quá trình đóng (đang xử lý sự kiện Closing) thì
            // KHÔNG được gọi Close() lần nữa - WPF sẽ ném InvalidOperationException
            // ("Cannot ... Close ... while a Window is closing"). Trường hợp này xảy ra
            // khi người dùng bấm [X]: Closing -> CancelCommand -> RequestClose -> đây.
            if (!_isClosing)
            {
                Dispatcher.Invoke(Close);
            }
        };

        Loaded += (_, _) => TenMonHocTextBox.Focus();

        Closing += (_, _) =>
        {
            _isClosing = true;

            // Người dùng bấm nút [X] thay vì Hủy -> vẫn phải trả kết quả "hủy"
            // cho caller đang await, để tránh treo dialog.
            if (!_closingFromViewModel)
            {
                _viewModel.CancelCommand.Execute(null);
            }
        };
    }
    
    /// <summary>Hiển thị modal và trả về true nếu người dùng đã lưu thành công.</summary>
    public static bool? ShowDialog(Window owner, SubjectEditViewModel viewModel)
    {
        var window = new ChinhSuaMonHocDialogView(viewModel) { Owner = owner };
        window.ShowDialog();

        return viewModel.DialogResult;
    }
}
