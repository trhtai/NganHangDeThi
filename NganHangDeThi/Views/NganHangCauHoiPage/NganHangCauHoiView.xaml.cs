using NganHangDeThi.ViewModels.NganHangCauHoiPage;
using System.Windows.Controls;

namespace NganHangDeThi.Views.NganHangCauHoiPage;

public partial class NganHangCauHoiView : UserControl
{
    public NganHangCauHoiView(NganHangCauHoiViewModel vm)
    {
        InitializeComponent();

        DataContext = vm;
    }
}
