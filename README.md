# 🔧 Zimmet Bakım Takip Sistemi

**Zimmet Bakım Takip Sistemi**, kurumsal çevrelerde demirbaş zimmetleri ve bakım süreçlerini kolayca yönetmenizi sağlayan modern bir WPF desktop uygulamasıdır.

![.NET](https://img.shields.io/badge/.NET-8.0-blue)
![WPF](https://img.shields.io/badge/WPF-Windows-blue)
![SQLite](https://img.shields.io/badge/Database-SQLite-green)
![License](https://img.shields.io/badge/License-MIT-yellow)

## ✨ Özellikler

### 📋 Ana Fonksiyonlar
- **Cihaz Yönetimi**: Demirbaş kayıtları, seri numaraları ve teknik özellikler
- **Personel Yönetimi**: Çalışan bilgileri ve departman organizasyonu
- **Zimmet Takibi**: Cihaz-personel atama ve takip sistemi
- **Bakım Planlaması**: Periyodik bakım ve arıza kayıtları
- **Raporlama**: Detaylı Excel ve CSV raporları
- **Ek Dosya Yönetimi**: Faturalar, garanti belgeleri ve fotoğraflar

### 🎨 Kullanıcı Deneyimi
- **Modern Arayüz**: Dark/Light tema desteği
- **Kullanıcı Yönetimi**: Rol tabanlı erişim kontrolü
- **Veri Güvenliği**: Otomatik yedekleme ve geri yükleme
- **Çoklu Dil**: Türkçe ve İngilizce dil desteği
- **Responsive Tasarım**: Farklı ekran boyutları için optimize edilmiş

### 💾 Veri Yönetimi
- **SQLite Veritabanı**: Yerel ve paylaşımlı veritabanı desteği
- **Veri Temizleme**: Örnek verileri kolayca temizleme
- **Import/Export**: Verilerinizi kolayca yedekleme ve aktarma
- **Cloud Sync**: Bulut senkronizasyon desteği

## 📥 Hızlı İndirme

### 💻 Hazır Exe Dosyası (Önerilen)

**⚡ En hızlı yol - Direkt kullanım için:**

### 🚀 **STANDALONE EXE (ÖNERİLEN) - .NET Gerektirmez**
1. **GitHub'a gidin**: https://github.com/t8puz/zimmet-bakim-takip
2. **Standalone zip'i indirin**: `Zimmet_Bakim_Takip_Standalone_v1.0.1.zip` (65MB)
3. **Zip'i açın** ve `Zimmet_Bakim_Takip.exe` dosyasına **çift tıklayın**
4. **Hiçbir yükleme gerekmez!** Direkt çalışır ✅

**🔗 Direkt Bağlantı**: [Zimmet_Bakim_Takip_Standalone_v1.0.1.zip](https://github.com/t8puz/zimmet-bakim-takip/blob/main/Zimmet_Bakim_Takip_Standalone_v1.0.1.zip)

### 📦 **Normal Exe (.NET 8.0 Gerektirir)**
1. **Alternatif olarak**: `Zimmet_Bakim_Takip_v1.0.0.zip` dosyasını indirin
2. **Daha küçük** (66KB) ama .NET 8.0 Desktop Runtime gerektirir

**🔗 Normal Exe**: [Zimmet_Bakim_Takip_v1.0.0.zip](https://github.com/t8puz/zimmet-bakim-takip/blob/main/Zimmet_Bakim_Takip_v1.0.0.zip)

### 📦 Releases'den İndirme (Alternatif)

1. **Releases sekmesi**: GitHub sayfasında sağ tarafta "Releases" sekmesine tıklayın
2. **En son sürüm**: "Latest" etiketli sürümü bulun
3. **Assets**: "Assets" bölümünde zip dosyasını indirin

### ⚠️ .NET 8.0 Runtime Gereksinimi

Eğer program çalışmazsa, .NET 8.0 Desktop Runtime'ı yükleyin:
- **İndirme Linki**: https://dotnet.microsoft.com/download/dotnet/8.0
- **Seçin**: ".NET Desktop Runtime 8.0" (Windows x64)

## 🚀 Kurulum

### Sistem Gereksinimleri
- **İşletim Sistemi**: Windows 10/11 (x64)
- **.NET Runtime**: .NET 8.0 Desktop Runtime
- **RAM**: Minimum 4GB (8GB önerilen)
- **Disk Alanı**: 500MB boş alan

### Geliştiriciler İçin
```bash
# Repository'yi klonlayın
git clone https://github.com/kullaniciadi/zimmet-bakim-takip.git

# Proje klasörüne gidin
cd zimmet-bakim-takip/Zimmet_Bakim_Takip

# Bağımlılıkları yükleyin
dotnet restore

# Projeyi derleyin
dotnet build --configuration Release

# Uygulamayı çalıştırın
dotnet run
```

### Son Kullanıcılar İçin
1. **Yukarıdaki "Hızlı İndirme" bölümünden** zip dosyasını indirin
2. **Zip dosyasını açın** ve bir klasöre çıkarın
3. **Zimmet_Bakim_Takip.exe** dosyasına çift tıklayın
4. **İlk giriş** için varsayılan kullanıcıyı kullanın:
   - **Kullanıcı Adı**: `admin`
   - **Şifre**: `admin123`
5. **Güvenlik için** ilk girişten sonra şifrenizi değiştirin

## 📖 Kullanım Kılavuzu

### İlk Kurulum
1. **Firma Bilgileri**: Ayarlar menüsünden firma bilgilerinizi girin
2. **Kullanıcı Oluşturma**: Yeni kullanıcılar ekleyin ve roller atayın
3. **Personel Kayıtları**: Departman ve çalışan bilgilerini girin
4. **Cihaz Kayıtları**: Demirbaş bilgilerini sisteme ekleyin

### Temel İşlemler
- **Zimmet Atama**: Cihazları personellere atayın
- **Bakım Kayıtları**: Periyodik bakım ve arıza kayıtları oluşturun
- **Raporlama**: Excel/CSV formatında raporlar alın
- **Veri Yedekleme**: Düzenli olarak verilerinizi yedekleyin

### Gelişmiş Özellikler
- **Toplu İşlemler**: Çoklu seçim ile hızlı işlemler
- **Filtreleme**: Gelişmiş arama ve filtreleme seçenekleri
- **Bildirimler**: Bakım hatırlatıcıları ve sistem bildirimleri

## 🛠️ Teknoloji Yığını

| Teknoloji | Versiyon | Açıklama |
|-----------|----------|----------|
| .NET | 8.0 | Ana framework |
| WPF | 8.0 | UI framework |
| Entity Framework Core | 8.0 | ORM |
| SQLite | 3.x | Veritabanı |
| BCrypt.NET | 4.x | Şifre hashleme |
| Microsoft.WindowsAPICodePack | 1.1 | Dosya dialog |

## 📁 Proje Yapısı

```
Zimmet_Bakim_Takip/
├── 📁 Database/           # Veritabanı yapılandırmaları
├── 📁 Models/            # Veri modelleri
├── 📁 Services/          # İş mantığı servisleri
├── 📁 Pages/             # UI sayfaları
├── 📁 Controls/          # Özel kontroller
├── 📁 Utilities/         # Yardımcı sınıflar
├── 📁 Converters/        # XAML dönüştürücüleri
├── 📁 Themes/            # Tema dosyaları
└── 📁 Images/            # Görsel kaynaklar
```

## 🤝 Katkıda Bulunma

Projeye katkıda bulunmak istiyorsanız:

1. **Fork** edin
2. **Feature branch** oluşturun (`git checkout -b feature/YeniOzellik`)
3. Değişikliklerinizi **commit** edin (`git commit -am 'Yeni özellik eklendi'`)
4. Branch'inizi **push** edin (`git push origin feature/YeniOzellik`)
5. **Pull Request** oluşturun

### Geliştirme Kuralları
- Clean Code prensiplerini takip edin
- Unit testler ekleyin
- Commit mesajlarını açıklayıcı yazın
- Code review sürecine katılın

## 📊 Sürüm Geçmişi

### v1.0.0 (2025-01-22)
- ✅ İlk kararlı sürüm
- ✅ Temel CRUD işlemleri
- ✅ Modern UI tasarımı
- ✅ Veri yedekleme sistemi
- ✅ Kullanıcı yönetimi
- ✅ Raporlama modülü

### Gelecek Sürümler
- 🔄 Web API entegrasyonu
- 🔄 Mobile uygulama desteği
- 🔄 Gelişmiş raporlama
- 🔄 QR kod desteği
- 🔄 E-posta bildirimleri

## 🔧 Sorun Giderme

### ❌ Program Açılmıyor

**🚀 ÖNERİLEN ÇÖZÜM**: Standalone Exe kullanın
- `Zimmet_Bakim_Takip_Standalone_v1.0.1.zip` dosyasını indirin
- **Hiçbir ek yükleme gerektirmez!**
- Direkt çalışır, .NET runtime'a ihtiyaç duymaz

**Çözüm 1**: Normal exe için .NET 8.0 Desktop Runtime'ı yükleyin
- [İndirme Linki](https://dotnet.microsoft.com/download/dotnet/8.0)
- "NET Desktop Runtime 8.0" seçin (Windows x64)

**Çözüm 2**: Windows Defender/Antivirüs
- Zimmet_Bakim_Takip.exe'yi güvenilir dosyalar listesine ekleyin

### ❌ "Veritabanı Hatası" Mesajı
**Çözüm**: 
- Program klasöründe yazma izni olduğundan emin olun
- Programı "Yönetici olarak çalıştır" seçeneği ile açın

### ❌ "Giriş Başarısız" Hatası
**Varsayılan bilgiler**:
- Kullanıcı Adı: `admin`
- Şifre: `admin123`

### ❌ Exe Dosyası Bulunamıyor
1. GitHub'da `Zimmet_Bakim_Takip_v1.0.0.zip` dosyasını arayın
2. Zip dosyasını tamamen çıkarın
3. `Zimmet_Bakim_Takip.exe` dosyasını bulun

## 🐛 Sorun Bildirimi

Sorun yaşıyorsanız:
1. **Issues** sekmesinden yeni bir issue açın
2. Sorunu detaylı şekilde açıklayın
3. Hata ekran görüntüleri ekleyin
4. Sistem bilgilerinizi belirtin

## 📄 Lisans

Bu proje MIT Lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.

## 👨‍💻 Geliştirici

**Muhammed Topuz**
- 📧 E-posta: [mametopuz1@gmail.com](mailto:mametopuz1@gmail.com)
- 💼 LinkedIn: [Profil Linki](https://linkedin.com/in/muhammed-topuz)
- 🐱 GitHub: [GitHub Profili](https://github.com/kullaniciadi)

---

## 🚨 Önemli Notlar

- ⚠️ İlk çalıştırmada örnek veriler otomatik yüklenir
- ⚠️ Düzenli veri yedeklemesi yapmanız önerilir
- ⚠️ Güvenlik için varsayılan şifreleri değiştirin
- ⚠️ Sistem yöneticisi hakları gerekebilir

## 📞 Destek

Teknik destek için:
- 📧 **E-posta**: [mametopuz1@gmail.com](mailto:mametopuz1@gmail.com)
- 🐛 **Bug Report**: GitHub Issues
- 💬 **Soru & Öneri**: GitHub Discussions

---

<div align="center">
  <p><strong>Zimmet Bakım Takip Sistemi</strong> ile işletmenizi dijitalleştirin! 🚀</p>
  <p>Made with ❤️ in Turkey</p>
</div> 