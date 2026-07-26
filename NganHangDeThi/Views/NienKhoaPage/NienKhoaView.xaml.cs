using NganHangDeThi.ViewModels.NienKhoaPage;
using System.Windows.Controls;

namespace NganHangDeThi.Views.NienKhoaPage;

public partial class NienKhoaView : UserControl
{
    public NienKhoaView(NienKhoaViewModel vm)
    {
        InitializeComponent();

        DataContext = vm;
    }
}
