using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Zimmet_Bakim_Takip
{
    /// <summary>
    /// SplashScreen.xaml için etkileşim mantığı
    /// </summary>
    public partial class SplashScreen : Window
    {
        private DispatcherTimer _timer;
        
        public SplashScreen()
        {
            InitializeComponent();
            
            // Pencere ikonu ayarla
            this.Icon = new BitmapImage(new Uri("pack://application:,,,/Images/logo.png", UriKind.Absolute));
            
            // Zamanlayıcı ayarla
            _timer = new DispatcherTimer();
            _timer.Tick += Timer_Tick;
            _timer.Interval = TimeSpan.FromSeconds(3);
            _timer.Start();
        }
        
        private void Timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            
            // Ana uygulamayı başlat
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            
            // SplashScreen'i kapat
            this.Close();
        }
    }
} 