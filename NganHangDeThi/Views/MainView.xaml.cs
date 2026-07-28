using HandyControl.Controls;
using HandyControl.Data;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.DependencyInjection;
using NganHangDeThi.Data.Repositories.Interfaces;
using NganHangDeThi.Views.Dialogs;
using NganHangDeThi.Views.KhoaPage;
using NganHangDeThi.Views.LopPage;
using NganHangDeThi.Views.MonHocPage;
using NganHangDeThi.Views.NganHangCauHoiPage;
using NganHangDeThi.Views.NienKhoaPage;
using NganHangDeThi.Views.Settings;
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

        UpdateMaximizeIcon();
        SetHomeBreadcrumb();

        Loaded += (_, _) => NavigateTo(QuestionBankMenuItem);
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
     * VẤN ĐỀ 1 - AUTO-SELECT KHI MỞ GROUP:
     * Khi click vào Mục Cha (VD: Quản lý danh mục), HandyControl tự động chọn 
     * (auto-select) mục con đầu tiên (Khoa). Việc này set IsSelected = true CHO KHOA
     * và đồng thời cập nhật CON TRỎ "ĐANG CHỌN" NỘI BỘ của SideMenu thành Khoa —
     * dù user chưa hề bấm vào Khoa.
     *
     * VẤN ĐỀ 2 - HỆ QUẢ KHI CHỈ HỦY IsSelected:
     * Nếu ta chỉ set lại autoSelectedItem.IsSelected = false để "hủy hiển thị", con trỏ 
     * NỘI BỘ của SideMenu (không phải property IsSelected) VẪN đang trỏ vào Khoa.
     * Vì thế khi user click THẬT vào Khoa, dưới góc nhìn của Selector, đó là 
     * "chọn lại đúng item đang được chọn" -> KHÔNG CÓ GÌ THAY ĐỔI -> SelectionChanged 
     * KHÔNG BẮN RA. App không phản ứng. Phải click sang 1 item khác (Môn học) để con trỏ 
     * nội bộ dịch chuyển, rồi quay lại click Khoa mới có tác dụng.
     *
     * GIẢI PHÁP:
     * Không thể chỉ dựa vào SelectionChanged cho trường hợp này vì sự kiện sẽ không bắn ra.
     * Ta tự phát hiện case "user click đúng vào item đang bị phantom-select" NGAY TẠI 
     * PreviewMouseLeftButtonUp (chạy TRƯỚC khi Selector xử lý gì), và tự gọi NavigateTo() 
     * thủ công tại đó, không chờ SelectionChanged nữa.
     *
     * Thứ tự sự kiện khi user click chuột trái:
     * PreviewMouseLeftButtonDown -> MouseLeftButtonDown 
     * -> PreviewMouseLeftButtonUp -> MouseLeftButtonUp 
     * -> (Selector xử lý chọn nội bộ) -> SelectionChanged (có thể không bắn ra).
     * ==================================================================================== */

    // true khi click vừa rồi là vào Group Header (VD: Quản lý danh mục) -> lần 
    // SelectionChanged kế tiếp là do HandyControl tự auto-select, không phải ý user.
    private bool _ignoreGroupHeaderAutoSelect;

    // true khi ta VỪA tự gọi NavigateTo() thủ công từ Preview event -> nếu SelectionChanged 
    // vẫn bắn ra (trùng lặp) thì phải bỏ qua hoàn toàn, tránh xử lý 2 lần / dispose nhầm scope.
    private bool _suppressNextSelectionChanged;

    // Item đang bị "phantom selected": HandyControl coi nó là item đang chọn ở tầng NỘI BỘ,
    // dù màn hình không hiển thị nó là active và trang thật đang hiển thị không phải nó.
    private Hc.SideMenuItem? _phantomSelectedItem;

    // Tag của trang đang thực sự được hiển thị trên PageContent.
    private string _currentPageTag = string.Empty;

    private static readonly HashSet<string> NavigablePageTags = new()
    {
        "QuestionBankMenu",
        "KhoaView",
        "LopHocView",
        "SubjectView",
        "NienKhoaView"
    };

    private void MainSideMenu_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // Xóa cờ suppress từ lần click trước, phòng trường hợp SelectionChanged của lần 
        // click trước không bắn ra (khiến cờ bị "kẹt"), tránh làm ảnh hưởng sai tới lần click này.
        _suppressNextSelectionChanged = false;

        var clickedItem = FindAncestor<Hc.SideMenuItem>(e.OriginalSource as DependencyObject);
        if (clickedItem == null)
        {
            _ignoreGroupHeaderAutoSelect = false;
            return;
        }

        bool isGroupHeader = clickedItem.Items.Count > 0;
        _ignoreGroupHeaderAutoSelect = isGroupHeader;

        if (isGroupHeader)
        {
            return;
        }

        string tag = clickedItem.Tag?.ToString() ?? string.Empty;
        if (!NavigablePageTags.Contains(tag))
        {
            return;
        }

        // Trường hợp đặc biệt: item vừa click chính là item đang bị "phantom selected"
        // (do auto-select trước đó khi mở group cha), và trang thật đang hiển thị KHÔNG 
        // PHẢI trang này. Vì Selector coi đây là chọn lại chính nó, SelectionChanged sẽ 
        // KHÔNG bắn ra sau click này -> tự điều hướng thủ công ngay tại đây.
        if (clickedItem == _phantomSelectedItem && tag != _currentPageTag)
        {
            _suppressNextSelectionChanged = true;
            NavigateTo(clickedItem);
        }
    }

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

    private void MainSideMenu_SelectionChanged(object? sender, FunctionEventArgs<object> e)
    {
        // Nếu SelectionChanged này xảy ra do ta vừa tự gọi NavigateTo() thủ công (từ Preview
        // event) gây ra, bỏ qua hoàn toàn để tránh xử lý 2 lần / dispose nhầm scope vừa tạo.
        if (_suppressNextSelectionChanged)
        {
            _suppressNextSelectionChanged = false;
            return;
        }

        // Bỏ qua nếu click vào Group Header (VD: Quản lý danh mục). Đồng thời ghi nhớ item 
        // vừa bị auto-select là "phantom" (để phối hợp xử lý ở PreviewMouseLeftButtonUp phía 
        // trên), và hủy phần HIỂN THỊ của việc auto-select đó.
        if (_ignoreGroupHeaderAutoSelect)
        {
            _ignoreGroupHeaderAutoSelect = false;

            if (e.Info is Hc.SideMenuItem autoSelectedItem)
            {
                _phantomSelectedItem = autoSelectedItem;

                MainSideMenu.SelectionChanged -= MainSideMenu_SelectionChanged;
                try
                {
                    autoSelectedItem.IsSelected = false;
                }
                finally
                {
                    MainSideMenu.SelectionChanged += MainSideMenu_SelectionChanged;
                }
            }

            return;
        }

        var selectedItem = e.Info as Hc.SideMenuItem;
        if (selectedItem == null || selectedItem.Tag == null)
        {
            return;
        }

        string tag = selectedItem.Tag.ToString() ?? string.Empty;

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

        UserControl? page = tag switch
        {
            "QuestionBankMenu" => ResolvePage<NganHangCauHoiView>(),
            "KhoaView" => ResolvePage<KhoaView>(),
            "LopHocView" => ResolvePage<LopView>(),
            "SubjectView" => ResolvePage<MonHocView>(),
            "NienKhoaView" => ResolvePage<NienKhoaView>(),
            _ => null
        };

        PageContent.Content = page;
        SetBreadcrumb(GetBreadcrumbPath(item));

        _currentPageTag = tag;
        // Vừa điều hướng thật sự thì item này không còn là "phantom" nữa (nếu nó từng là).
        _phantomSelectedItem = null;

        if (!item.IsSelected)
        {
            // Set thủ công để đảm bảo hiển thị đúng, đồng thời tự bảo vệ khỏi việc kích hoạt
            // lại SelectionChanged một cách đệ quy/trùng lặp.
            MainSideMenu.SelectionChanged -= MainSideMenu_SelectionChanged;
            try
            {
                item.IsSelected = true;
            }
            finally
            {
                MainSideMenu.SelectionChanged += MainSideMenu_SelectionChanged;
            }
        }

        Sidebar.IsOpen = false;
    }

    /* ====================================================================================
     * Quản lý vòng đời Trang & Chống Rò rỉ bộ nhớ (Memory Leak)
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

    private void SetBreadcrumb(List<Hc.SideMenuItem> path)
    {
        BreadcrumbPanel.Children.Clear();

        for (int i = 0; i < path.Count; i++)
        {
            var node = path[i];
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

                segment.MouseLeftButtonDown += (_, args) => args.Handled = true;
                segment.MouseLeftButtonUp += (_, _) => OnBreadcrumbSegmentClicked(node);

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

    private void OnBreadcrumbSegmentClicked(SideMenuItem node)
    {
        string? tag = node.Tag?.ToString();

        if (tag != null && NavigablePageTags.Contains(tag))
        {
            NavigateTo(node);
        }
        else
        {
            Sidebar.IsOpen = true;
        }
    }
    #endregion

    #region Window Chrome
    private void ToggleMaximizeRestore()
    {
        WindowState =
            WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;
    }

    private void UpdateMaximizeIcon()
    {
        WindowMaximizeIcon.Kind =
            WindowState == WindowState.Maximized
            ? PackIconMaterialKind.WindowRestore
            : PackIconMaterialKind.WindowMaximize;
    }

    private void TitleBarBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            ToggleMaximizeRestore();
            return;
        }

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

    private bool _confirmedExit;

    private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_confirmedExit)
        {
            return;
        }

        e.Cancel = true;

        await Task.Yield();

        bool showConfirm = await _caiDatRepo.GetHienThiXacNhanThoatAsync();

        if (!showConfirm)
        {
            _confirmedExit = true;
            Close();
            return;
        }

        var dialogContent = new XacNhanThoatDialog();
        Dialog dialogWindow = Hc.Dialog.Show(dialogContent);

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
            e.Handled = true;
        }
    }

    private void SettingButton_Click(object sender, RoutedEventArgs e)
    {
        var scope = App.AppHost!.Services.CreateScope();
        var settingView = scope.ServiceProvider.GetRequiredService<SettingView>();
        var dialog = Hc.Dialog.Show(settingView);

        dialog.Unloaded += (_, _) =>
        {
            scope.Dispose();
        };
    }
    #endregion
}