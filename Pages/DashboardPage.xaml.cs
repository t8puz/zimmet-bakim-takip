using System;
using System.Linq;
using System.Windows.Controls;
using Zimmet_Bakim_Takip.Models;
using Zimmet_Bakim_Takip.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Zimmet_Bakim_Takip.Pages
{
    public partial class DashboardPage : Page
    {
        private readonly ICihazService _cihazService;
        private readonly IBakimService _bakimService;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IZimmetService _zimmetService;

        public DashboardPage(ICihazService cihazService = null, IBakimService bakimService = null, IMaintenanceService maintenanceService = null, IZimmetService zimmetService = null)
        {
            InitializeComponent();
            _cihazService = cihazService ?? App.CihazService;
            _bakimService = bakimService ?? App.BakimService;
            _maintenanceService = maintenanceService ?? App.MaintenanceService;
            _zimmetService = zimmetService ?? App.ZimmetService;
            LoadDashboardData();
        }

        private async void LoadDashboardData()
        {
            try
            {
                // İstatistikleri yükle
                var devices = await _cihazService.GetAllDevicesAsync();
                
                // Toplam cihaz sayısı
                TotalDevicesText.Text = devices.Count.ToString();

                // Zimmetli cihaz sayısı
                var assignments = await _zimmetService.GetAllAsync();
                AssignedDevicesText.Text = assignments.Count(a => a.Status == "Aktif").ToString();

                // Bakımdaki cihaz sayısı
                var maintenances = await _maintenanceService.GetAllAsync();
                MaintenanceDevicesText.Text = maintenances.Count(m => m.Status == "Devam Ediyor").ToString();

                // Yaklaşan bakım sayısı
                var upcomingMaintenanceCount = maintenances.Count(m => 
                    m.NextMaintenanceDate.HasValue && 
                    m.NextMaintenanceDate.Value.Date <= DateTime.Now.AddDays(30));
                UpcomingMaintenanceText.Text = upcomingMaintenanceCount.ToString();

                // Son zimmet işlemleri
                if (_zimmetService != null)
                {
                    try
                    {
                        var recentAssignments = await _zimmetService.GetRecentAssignmentsAsync(5);
                        
                        // Verilerin olduğunu kontrol et
                        if (recentAssignments != null)
                        {
                            // Sonuçları dönüştür
                            var recentAssignmentsViewModel = recentAssignments.Select(a => new
                            {
                                DeviceName = a?.Device?.Name ?? "Bilinmeyen Cihaz",
                                PersonnelName = a?.Personnel?.FullName ?? "Bilinmeyen Personel", 
                                Date = a?.AssignmentDate.ToString("dd.MM.yyyy") ?? "Tarih Bilinmiyor"
                            }).ToList();
                            
                            RecentAssignmentsList.ItemsSource = recentAssignmentsViewModel;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Zimmet bilgileri yüklenirken hata: {ex.Message}");
                        // Hata mesajını gösterme, sadece bu kısmı atla
                    }
                }

                // Yaklaşan bakımlar
                var upcomingMaintenanceRecords = await _maintenanceService.GetUpcomingMaintenanceAsync(30);
                
                // Sonuçları dönüştür
                var upcomingMaintenanceViewModel = upcomingMaintenanceRecords.Select(m => new 
                {
                    DeviceName = m.Device?.Name ?? "Bilinmeyen Cihaz",
                    MaintenanceType = m.MaintenanceType,
                    Date = m.NextMaintenanceDate.HasValue ? m.NextMaintenanceDate.Value.ToString("dd.MM.yyyy") : "Tarih Belirsiz"
                }).ToList();
                
                UpcomingMaintenanceList.ItemsSource = upcomingMaintenanceViewModel;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Veriler yüklenirken bir hata oluştu: {ex.Message}",
                    "Hata",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private async void btnGenerateSampleData_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                btnGenerateSampleData.IsEnabled = false;
                btnGenerateSampleData.Content = "Veriler Oluşturuluyor...";
                
                // Örnek verileri oluştur
                await SampleDataGenerator.GenerateAllSampleData();
                
                // Verileri yeniden yükle
                LoadDashboardData();
                
                System.Windows.MessageBox.Show(
                    "Örnek veriler başarıyla oluşturuldu.",
                    "Bilgi",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Örnek veriler oluşturulurken bir hata oluştu: {ex.Message}",
                    "Hata",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                btnGenerateSampleData.IsEnabled = true;
                btnGenerateSampleData.Content = "Örnek Veri Oluştur";
            }
        }
    }
} 