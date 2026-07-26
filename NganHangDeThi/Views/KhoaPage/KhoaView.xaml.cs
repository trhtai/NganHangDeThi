using NganHangDeThi.ViewModels.KhoaPage;
using System.Windows.Controls;

namespace NganHangDeThi.Views.KhoaPage;

public partial class KhoaView : UserControl
{
    public KhoaView(KhoaViewModel vm)
    {
        InitializeComponent();

        DataContext = vm;
    }
}
