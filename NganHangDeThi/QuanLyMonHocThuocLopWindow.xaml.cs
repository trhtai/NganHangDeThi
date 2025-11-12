using Microsoft.EntityFrameworkCore;
using NganHangDeThi.Data.DataContext;
using NganHangDeThi.Data.Entity;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace NganHangDeThi
{
    public partial class QuanLyMonHocThuocLopWindow : Window, INotifyPropertyChanged
    {
        private readonly AppDbContext _db;
        private readonly int _lopHocId;

        public ObservableCollection<MonHocThuocLop> DsMonHocCuaLop { get; set; } = new();
        public ObservableCollection<MonHoc> MonHocChuaThuocLop { get; set; } = new();
        public MonHoc? MonHocDuocChon { get; set; }

        public QuanLyMonHocThuocLopWindow(AppDbContext db, int lopHocId)
        {
            InitializeComponent();
            DataContext = this;

            _db = db;
            _lopHocId = lopHocId;

            LoadData();
        }

        private void LoadData()
        {
            // Danh sách môn học đã thuộc lớp
            var monThuocLop = _db.MonHocThuocLop
                .Include(mhl => mhl.MonHoc)
                .Where(mhl => mhl.LopHocId == _lopHocId)
                .ToList();

            DsMonHocCuaLop = new ObservableCollection<MonHocThuocLop>(monThuocLop);
            OnPropertyChanged(nameof(DsMonHocCuaLop));

            // Tất cả môn học
            var tatCaMon = _db.MonHoc.ToList();
            // Môn học chưa thuộc lớp
            var monChuaThuoc = tatCaMon
                .Where(m => !DsMonHocCuaLop.Any(x => x.MonHocId == m.Id))
                .ToList();

            MonHocChuaThuocLop = new ObservableCollection<MonHoc>(monChuaThuoc);
            OnPropertyChanged(nameof(MonHocChuaThuocLop));
        }

        private void BtnThem_Click(object sender, RoutedEventArgs e)
        {
            if (MonHocDuocChon == null)
            {
                MessageBox.Show("Vui lòng chọn một môn học.");
                return;
            }

            var moi = new MonHocThuocLop
            {
                LopHocId = _lopHocId,
                MonHocId = MonHocDuocChon.Id
            };

            _db.MonHocThuocLop.Add(moi);
            _db.SaveChanges();

            LoadData(); // Refresh lại
            MonHocDuocChon = null;
            OnPropertyChanged(nameof(MonHocDuocChon));
        }

        private void BtnXoa_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is MonHocThuocLop item)
            {
                var confirm = MessageBox.Show($"Xóa môn \"{item.MonHoc.TenMon}\" khỏi lớp?", "Xác nhận", MessageBoxButton.YesNo);
                if (confirm != MessageBoxResult.Yes) return;

                _db.MonHocThuocLop.Remove(item);
                _db.SaveChanges();

                LoadData();
            }
        }

        #region NotifyPropertyChanged
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
        #endregion

        private void BtnDongForm_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
