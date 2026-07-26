using HandyControl.Controls;
using HandyControl.Data;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.DependencyInjection;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.Views.Dialogs;
using NganHangDeThi.Views.MonHoc;
using NganHangDeThi.Views.Settings;
using NganHangDeThi.Views.StudentClasses;
using NganHangDeThi.Views.Teachers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Hc = HandyControl.Controls;

namespace NganHangDeThi.Views;

public partial class MainView : System.Windows.Window
{
    private readonly ICaiDatRepository _caiDatRepo;

    public MainView(ICaiDatRepository caiDatRepo)
    {
        InitializeComponent();
        _caiDatRepo = caiDatRepo;

        // Đồng bộ icon của nút maximize/restore với trạng thái ban đầu của cửa sổ.
        UpdateMaximizeIcon();

        // Breadcrumb mặc định.
        SetHomeBreadcrumb();
    }

    private void SetHomeBreadcrumb()
    {
        BreadcrumbPanel.Children.Clear();
        BreadcrumbPanel.Children.Add(new TextBlock
        {
            Text = "Trang chủ",
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            Foreground = (Brush)FindResource("PrimaryTextBrush"),
            VerticalAlignment = VerticalAlignment.Center
        });
    }

    #region Điều hướng Sidebar và Breadcrumb
    /* ====================================================================================
     * GIẢI THÍCH LOGIC XỬ LÝ CLICK MENU (HACK HANDYCONTROL AUTO-SELECT)
     * ====================================================================================
     * Vấn đề: 
     * Khi click vào một Mục Cha (chứa các mục con, VD: Quản lý danh mục) để xổ menu ra, 
     * HandyControl mặc định sẽ TỰ ĐỘNG CHỌN (auto-select) mục con đầu tiên bên trong nó.
     * Điều này khiến sự kiện SelectionChanged bị kích hoạt sai mục đích, làm menu bị đóng 
     * hoặc nhảy trang dù người dùng chưa hề bấm vào mục con.
     * 
     * Giải pháp: Sử dụng 2 sự kiện kết hợp (PreviewMouseLeftButtonUp chạy trước, SelectionChanged chạy sau).
     * Cụ thể:
     * 1. PreviewMouseLeftButtonUp (Tunneling Event): Đi từ ngoài vào trong, chạy TRƯỚC. 
     *    -> Dùng để "bắt lõi" xem con chuột thực sự click vào đâu. Nếu click vào Mục Cha, bật cờ báo hiệu lên.
     * 2. SelectionChanged: Chạy SAU. 
     *    -> Kiểm tra cờ báo hiệu. Nếu cờ đang bật (do click vào Mục Cha), ta hủy bỏ lệnh chuyển trang.
     * Thứ tự sự kiện khi user click chuột trái như sau: 
     * PreviewMouseLeftButtonDown -> MouseLeftButtonDown 
     * -> PreviewMouseLeftButtonUp -> MouseLeftButtonUp 
     * -> SelectionChanged.
     * ==================================================================================== */

    // Đánh dấu click vừa rồi là click vào Group Header như: QuestionBankMenu, ExamManagementMenu, CategoryMenu,...
    // Từ đây có thể biết là đang click vào cha hay vào con.
    private bool _ignoreNextSelectionChanged;

    // Danh sách Tag có thể được điều hướng
    private static readonly HashSet<string> NavigablePageTags = new()
    {
        "StudentClassView", "SubjectView", "TeacherView", "SettingView"
    };

    // Xác định phần tử người dùng thật sự nhấn vào trước khi HandyControl kịp xử lý.
    private void MainSideMenu_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // Láy ra phần từ SideMenuItem thật sự mà user click vào (có thể là TextBlock, Border, StackPanel, v.v...).
        var clickedItem = FindAncestor<Hc.SideMenuItem>(e.OriginalSource as DependencyObject);

        _ignoreNextSelectionChanged = clickedItem != null && clickedItem.Items.Count > 0;
    }

    // Cách hoạt động: bắt đầu từ phần tử bị click, hãy leo ngược dần lên
    // cây Visual Tree để tìm phần tử cha phù hợp. Nếu không tìm thấy, trả về null.
    // DependencyObject là lớp cơ sở cho tất cả các phần tử trong WPF, bao gồm cả UIElement và FrameworkElement.
    private static T? FindAncestor<T>(DependencyObject? current) where T : DependencyObject
    {
        while (current != null)
        {
            if (current is T target)
            {
                return target;
            }
            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private void MainSideMenu_SelectionChanged(object sender, FunctionEventArgs<object> e)
    {
        // Bỏ qua nếu click vào Group Header như: QuestionBankMenu, ExamManagementMenu, CategoryMenu,...
        if (_ignoreNextSelectionChanged)
        {
            _ignoreNextSelectionChanged = false;
            return;
        }

        var selectedItem = e.Info as Hc.SideMenuItem;
        if (selectedItem == null || selectedItem.Tag == null)
        {
            return;
        }

        string tag = selectedItem.Tag.ToString() ?? string.Empty;

        // Chỉ điều hướng nếu Tag thuộc danh sách Tag được phép điều hướng.
        if (!NavigablePageTags.Contains(tag))
        {
            return;
        }

        NavigateTo(selectedItem);
    }

    // Điều hướng trang + cập nhật breadcrumb + đóng sidebar.
    private void NavigateTo(Hc.SideMenuItem item)
    {
        string tag = item.Tag.ToString() ?? string.Empty;

        // Điều hướng trang.
        UserControl? page = tag switch
        {
            "StudentClassView" => ResolvePage<StudentClassView>(),
            "SubjectView" => ResolvePage<MonHocView>(),
            "TeacherView" => ResolvePage<TeacherView>(),
            "SettingView" => ResolvePage<SettingView>(),
            _ => null
        };

        PageContent.Content = page;

        // Cập nhật breadcrumb.
        SetBreadcrumb(GetBreadcrumbPath(item));

        // Đóng sidebar.
        Sidebar.IsOpen = false;
    }

    /* ====================================================================================
     * Quản lý vòng đời Trang & Chống Rò rỉ bộ nhớ (Memory Leak)
     * TẠI SAO BẮT BUỘC PHẢI CÓ HÀM NÀY? (NẾU KHÔNG CÓ THÌ BỊ GÌ?)
     * ====================================================================================
     * 1. NGUYÊN NHÂN GỐC RỄ (CÁI BẪY TRANSIENT + IDISPOSABLE):
     * - Bình thường, để lấy trang mới, ta hay gọi thẳng từ DI gốc: App.AppHost.Services...
     * - Các trang này thường được đăng ký dạng AddTransient (chuyển tới là tạo mới).
     * - LUẬT NGẦM CỦA DI: Nếu một class Transient có xài IDisposable (như DbContext),
     *   DI gốc sẽ TỰ ĐỘNG LƯU LẠI danh sách toàn bộ bọn chúng để chờ tắt app mới đem hủy.
     * 
     * 2. HẬU QUẢ TÀN KHỐC (TRÀN RAM):
     * - Người dùng chuyển qua lại giữa trang "Lớp học" và "Môn học" 100 lần.
     * - DI gốc đẻ ra 100 cái DbContext + ViewModel, và GIỮ TẤT CẢ LÀM CON TIN.
     * - Garbage Collector (GC) của C# không thể xóa rác vì DI gốc vẫn nắm đuôi chúng.
     * - Tràn RAM (Memory Leak) xảy ra, ứng dụng càng chạy lâu càng lag.
     * 
     * 3. CÁCH HÀM NÀY GIẢI CỨU (DÙNG "TÚI DÙNG 1 LẦN"):
     * - Không lấy thẳng từ DI gốc nữa. Mỗi khi mở trang, ta tạo một "Túi nhỏ" (Scope).
     * - Bắt DI gốc bỏ hết chùm View + ViewModel + DbContext vào cái túi nhỏ này.
     * - Khi chuyển sang trang khác, ta gọi `oldScope.Dispose()` để vứt hẳn túi cũ đi.
     * 
     * -> KẾT QUẢ: 
     * Túi cũ bị vứt -> DbContext bị ép đóng kết nối DB ngay lập tức -> ViewModel/View 
     * rơi vào trạng thái "vô chủ" -> Garbage Collector tự động quét sạch khỏi RAM.
     * Ứng dụng lúc nào cũng nhẹ tênh vì chỉ giữ đúng 1 trang trên RAM!
     * ==================================================================================== */

    private IServiceScope? _currentPageScope;

    private T ResolvePage<T>() where T : UserControl
    {
        var oldPage = _currentPageScope;

        _currentPageScope = App.AppHost!.Services.CreateScope();
        var page = _currentPageScope.ServiceProvider.GetRequiredService<T>();

        oldPage?.Dispose();

        return page;
    }

    // Đi ngược visual tree từ SideMenuItem được chọn lên tới gốc SideMenu,
    // thu thập từng SideMenuItem tổ tiên (bao gồm chính nó) theo thứ tự root -> leaf.
    private static List<Hc.SideMenuItem> GetBreadcrumbPath(DependencyObject item)
    {
        var path = new List<Hc.SideMenuItem>();
        var current = item;

        while (current != null)
        {
            if (current is Hc.SideMenuItem menuItem)
            {
                path.Add(menuItem);
            }

            if (current is Hc.SideMenu)
            {
                break;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        path.Reverse();
        return path;
    }

    // Render breadcrumb từ đường dẫn SideMenuItem, ngăn cách bằng " / ".
    // Mục cuối (trang hiện tại) in đậm, không click được.
    // Các mục trước đó click được để nhảy nhanh về cấp cha.
    private void SetBreadcrumb(List<Hc.SideMenuItem> path)
    {
        BreadcrumbPanel.Children.Clear();

        for (int i = 0; i < path.Count; i++)
        {
            var node = path[i];
            // Xác định đây có phải là node cuối không?
            bool isLast = i == path.Count - 1;

            var segment = new TextBlock
            {
                Text = node.Header?.ToString() ?? string.Empty,
                FontSize = 13,
                FontWeight = isLast ? FontWeights.SemiBold : FontWeights.Normal,
                Foreground = (Brush)FindResource(isLast ? "PrimaryTextBrush" : "SecondaryTextBrush"),
                VerticalAlignment = VerticalAlignment.Center
            };

            if (!isLast)
            {
                segment.Cursor = Cursors.Hand;


                // QUAN TRỌNG: chặn MouseLeftButtonDown nổi bọt lên TitleBarBorder,
                // nếu không nó sẽ trigger DragMove() và "nuốt" mất cú click của segment,
                // khiến MouseLeftButtonUp bên dưới không bao giờ được gọi.
                segment.MouseLeftButtonDown += (_, args) => args.Handled = true;
                segment.MouseLeftButtonUp += (_, _) => OnBreadcrumbSegmentClicked(node);

                // Làm hiệu ứng giống hover.
                segment.MouseEnter += (_, _) => segment.Foreground = (Brush)FindResource("PrimaryTextBrush");
                segment.MouseLeave += (_, _) => segment.Foreground = (Brush)FindResource("SecondaryTextBrush");
            }

            BreadcrumbPanel.Children.Add(segment);

            if (!isLast)
            {
                BreadcrumbPanel.Children.Add(new TextBlock
                {
                    Text = "  /  ",
                    FontSize = 13,
                    Foreground = (Brush)FindResource("SecondaryTextBrush"),
                    VerticalAlignment = VerticalAlignment.Center
                });
            }
        }
    }

    // Xử lý khi người dùng click vào 1 đoạn breadcrumb - segment (không phải đoạn cuối).
    private void OnBreadcrumbSegmentClicked(SideMenuItem node)
    {
        string? tag = node.Tag?.ToString();

        if (tag != null && NavigablePageTags.Contains(tag))
        {
            // Có tag và tag có thể điều hướng.
            NavigateTo(node);
        }
        else
        {
            Sidebar.IsOpen = true;
        }
    }
    #endregion

    #region Window Chrome
    // Nếu trạng thái cửa sổ là Maximized thì chuyển về Normal, ngược lại thì chuyển về Maximized.
    private void ToggleMaximizeRestore()
    {
        WindowState = 
            WindowState == WindowState.Maximized 
            ? WindowState.Normal 
            : WindowState.Maximized;
    }

    // Cập nhật icon của nút maximize/restore dựa trên trạng thái hiện tại của cửa sổ.
    private void UpdateMaximizeIcon()
    {
        WindowMaximizeIcon.Kind = 
            WindowState == WindowState.Maximized
            ? PackIconMaterialKind.WindowRestore
            : PackIconMaterialKind.WindowMaximize;
    }

    private void TitleBarBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Nếu là double-click thì phóng to hoặc thu nhỏ cửa sổ.
        if (e.ClickCount == 2)
        {
            ToggleMaximizeRestore();
            return;
        }

        // Nếu là nhấn giữ chuột trái thì kéo cửa sổ.
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void WindowMinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void WindowMaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleMaximizeRestore();
    }

    private void MainView_StateChanged(object sender, EventArgs e)
    {
        UpdateMaximizeIcon();
    }

    private void WindowCloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    // Cờ đánh dấu: Người dùng đã thực sự xác nhận muốn thoát.
    // Tác dụng: Chống vòng lặp vô hạn khi ta gọi lệnh Close() ở bên dưới.
    private bool _confirmedExit;

    private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        // 1. NẾU CỜ ĐÃ BẬT:
        // Lần gọi này là do chính chúng ta gọi Close() ở cuối hàm.
        // Cho phép cửa sổ đóng thật sự, kết thúc sự kiện tại đây.
        if (_confirmedExit)
        {
            return;
        }

        // 2. NẾU CỜ CHƯA BẬT (Người dùng vừa bấm nút X):
        // Bắt buộc phải HỦY bỏ tiến trình đóng cửa sổ mặc định của WPF để "câu giờ"
        // vì ta cần thời gian để gọi Database và chờ người dùng chọn Dialog.
        e.Cancel = true;

        /* ====================================================================================
         * "BÍ THUẬT" TASK.YIELD() - CHỐNG LỖI CRASH APP KHI ĐÓNG CỬA SỔ
         * ====================================================================================
         * - Vấn đề: Closing là sự kiện ĐỒNG BỘ. Lúc này WPF đang đánh dấu cửa sổ là "Đang bận đóng"
         *   do ta chủ động kéo dài thời gian bằng trick e.Cancel = true bên trên
         *   Nếu SQLite phản hồi quá nhanh (tức thì), luồng code sẽ chạy tuột xuống dưới và 
         *   gọi lệnh Close() lần 2 trong khi hàm Closing lần 1 này CÒN CHƯA KẾT THÚC.
         *   -> Crash App lập tức với lỗi: "Cannot call Close() while a Window is closing".
         * 
         * - Giải pháp Task.Yield(): 
         *   Lệnh này tạo ra một nhịp "nghỉ", nhường quyền điều khiển lại cho UI Thread. Nó bảo WPF: 
         *   "Tao hủy đóng cửa sổ rồi (Cancel=true), mày cứ kết thúc sự kiện Closing lần 1 đi".
         *   Chỉ khi WPF xóa bỏ trạng thái "Đang bận đóng", code bên dưới mới được lôi ra chạy tiếp.
         * ==================================================================================== */
        await Task.Yield();

        // 3. Đọc cấu hình từ Database xem người dùng có muốn tắt thông báo không.
        bool showConfirm = await _caiDatRepo.GetHienThiXacNhanThoatAsync();

        // Nếu trong DB cài đặt là "Không cần hỏi" tức false -> Bật cờ và tự động gọi tắt App.
        if (!showConfirm)
        {
            _confirmedExit = true;
            Close();
            return;
        }

        // 4. Nếu cần hỏi, hiện hộp thoại Dialog tùy chỉnh lên màn hình.
        var dialogContent = new XacNhanThoatDialog();
        Dialog dialogWindow = Hc.Dialog.Show(dialogContent);

        // 5. Lắng nghe quyết định của người dùng trên Dialog.
        dialogContent.ChoiceMade += async (confirmed, dontAskAgain) =>
        {
            dialogWindow.Close();

            if (dontAskAgain)
            {
                await _caiDatRepo.SetHienThiXacNhanThoatAsync(false);
            }

            if (confirmed)
            {
                _confirmedExit = true;
                Close();
            }
        };
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (e.Key == Key.Escape)
        {
            Sidebar.IsOpen = !Sidebar.IsOpen;

            // Báo cho ứng dụng biết sự kiện này đã được xử lý xong, 
            // tránh việc các control khác (như Dialog) vô tình nhận nhầm phím Esc này.
            e.Handled = true;
        }
    }
    #endregion
}
