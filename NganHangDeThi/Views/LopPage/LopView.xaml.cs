using NganHangDeThi.ViewModels.LopPage;
using System.Windows.Controls;

namespace NganHangDeThi.Views.LopPage;

public partial class LopView : UserControl
{
    public LopView(LopViewModel vm)
    {
        InitializeComponent();

        DataContext = vm;
    }
}
