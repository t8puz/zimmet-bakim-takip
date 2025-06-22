using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Zimmet_Bakim_Takip.Models;
using Zimmet_Bakim_Takip.Services;

namespace Zimmet_Bakim_Takip.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ICihazService _cihazService;
        private readonly IPersonelService _personelService;
        private readonly IZimmetService _zimmetService;
        private readonly IBakimService _bakimService;

        public MainViewModel(
            ICihazService cihazService,
            IPersonelService personelService,
            IZimmetService zimmetService,
            IBakimService bakimService)
        {
            _cihazService = cihazService;
            _personelService = personelService;
            _zimmetService = zimmetService;
            _bakimService = bakimService;

            Title = "Zimmet ve Bakım Takip Uygulaması";
            AktifZimmetler = new ObservableCollection<Zimmet>();
            BugunkuBakimlar = new ObservableCollection<Bakim>();
            GecikmisBakimlar = new ObservableCollection<Bakim>();
            SonBakimlar = new ObservableCollection<Bakim>();

            // Komutlar
            LoadDataCommand = new AsyncRelayCommand(async _ => await LoadDataAsync());
        }

        #region Properties

        private int _toplamCihazSayisi;
        public int ToplamCihazSayisi
        {
            get => _toplamCihazSayisi;
            set => SetProperty(ref _toplamCihazSayisi, value);
        }

        private int _zimmetliCihazSayisi;
        public int ZimmetliCihazSayisi
        {
            get => _zimmetliCihazSayisi;
            set => SetProperty(ref _zimmetliCihazSayisi, value);
        }

        private int _bugunkuBakimSayisi;
        public int BugunkuBakimSayisi
        {
            get => _bugunkuBakimSayisi;
            set => SetProperty(ref _bugunkuBakimSayisi, value);
        }

        private int _gecikmisBakimSayisi;
        public int GecikmisBakimSayisi
        {
            get => _gecikmisBakimSayisi;
            set => SetProperty(ref _gecikmisBakimSayisi, value);
        }

        public ObservableCollection<Zimmet> AktifZimmetler { get; }
        public ObservableCollection<Bakim> BugunkuBakimlar { get; }
        public ObservableCollection<Bakim> GecikmisBakimlar { get; }
        public ObservableCollection<Bakim> SonBakimlar { get; }

        #endregion

        #region Commands

        public ICommand LoadDataCommand { get; }

        #endregion

        #region Methods

        private async Task RunCommandAsync(Func<Task> action)
        {
            try
            {
                IsBusy = true;
                await action();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // Assignment'dan Zimmet'e dönüştürme yardımcı metodu
        private Zimmet ConvertAssignmentToZimmet(Assignment assignment)
        {
            return new Zimmet
            {
                Id = assignment.Id,
                PersonelId = assignment.PersonnelId,
                CihazId = assignment.DeviceId,
                ZimmetTarihi = assignment.AssignmentDate,
                IadeTarihi = assignment.ReturnDate,
                Aciklama = assignment.Notes
            };
        }

        public async Task LoadDataAsync()
        {
            await RunCommandAsync(async () =>
            {
                // Tüm verileri yükle
                var cihazlar = await _cihazService.GetAllDevicesAsync();
                var aktifZimmetler = await _zimmetService.GetAllAsync();
                var bugunkuBakimlar = await _bakimService.GetBugunkuBakimlarAsync();
                var gecikmisBakimlar = await _bakimService.GetGecikmisBakimlarAsync();
                var tamamlananBakimlar = await _bakimService.GetTamamlananBakimlarAsync();

                // İstatistikleri güncelle
                ToplamCihazSayisi = cihazlar.Count;
                ZimmetliCihazSayisi = aktifZimmetler.Count;
                BugunkuBakimSayisi = bugunkuBakimlar.Count;
                GecikmisBakimSayisi = gecikmisBakimlar.Count;

                // Listeleri güncelle
                AktifZimmetler.Clear();
                foreach (var zimmet in aktifZimmetler)
                {
                    AktifZimmetler.Add(ConvertAssignmentToZimmet(zimmet));
                }

                BugunkuBakimlar.Clear();
                foreach (var bakim in bugunkuBakimlar)
                {
                    BugunkuBakimlar.Add(bakim);
                }

                GecikmisBakimlar.Clear();
                foreach (var bakim in gecikmisBakimlar)
                {
                    GecikmisBakimlar.Add(bakim);
                }

                SonBakimlar.Clear();
                foreach (var bakim in tamamlananBakimlar.Take(5))
                {
                    SonBakimlar.Add(bakim);
                }
            });
        }

        #endregion
    }
}