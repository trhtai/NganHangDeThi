using System.Windows;
using System.Windows.Controls;

namespace NganHangDeThi.Views.Dialogs;

public partial class XacNhanThoatDialog : UserControl
{
    // (confirmed: người dùng có chọn "Thoát" không, dontAskAgain: có tick "Không hiển thị lại" không).
    public event Action<bool, bool>? ChoiceMade;

    public XacNhanThoatDialog()
    {
        InitializeComponent();
    }

    private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        => ChoiceMade?.Invoke(true, DontAskAgainCheckBox.IsChecked == true);

    private void CancelButton_Click(object sender, RoutedEventArgs e)
        => ChoiceMade?.Invoke(false, false);
}
