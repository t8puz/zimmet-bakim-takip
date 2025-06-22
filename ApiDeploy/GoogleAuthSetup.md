# Google OAuth Entegrasyonu Kurulum Rehberi

Bu belge, Zimmet Bakım Takip uygulaması için Google OAuth kimlik doğrulama sisteminin nasıl kurulacağını açıklar.

## Adım 1: Google Cloud Console'da Proje Oluşturma

1. [Google Cloud Console](https://console.cloud.google.com/) adresine gidin ve Google hesabınızla giriş yapın.
2. Sağ üst köşedeki proje menüsünden "Yeni Proje" seçeneğini tıklayın.
3. Projenize bir isim verin (örn. "Zimmet Bakım Takip") ve "Oluştur" düğmesine tıklayın.

## Adım 2: OAuth Onayını Yapılandırma

1. Sol taraftaki menüden "API'ler ve Servisler" > "OAuth onay ekranı"nı seçin.
2. Kullanıcı türü olarak "Harici"i seçin ve "Oluştur" düğmesine tıklayın.
3. Aşağıdaki alanları doldurun:
   - Uygulama adı: "Zimmet Bakım Takip"
   - Kullanıcı destek e-postası: Kendi e-posta adresinizi girin
   - Geliştirici iletişim bilgileri: Kendi e-posta adresinizi girin
4. "Kaydet ve Devam Et" düğmesine tıklayın.
5. "Kapsamlar" bölümünde "Kapsamlar Ekle" düğmesine tıklayın ve aşağıdaki kapsamları ekleyin:
   - `openid`
   - `email`
   - `profile`
6. "Kaydet ve Devam Et" düğmesine tıklayın.
7. "Test kullanıcıları" ekranında "+ Kullanıcı Ekle" düğmesini tıklayarak test kullanıcılarını ekleyin.
8. "Kaydet ve Devam Et" düğmesine tıklayın.
9. Özeti gözden geçirin ve "Panoya Dön" düğmesine tıklayın.

## Adım 3: OAuth İstemci Kimlik Bilgilerini Oluşturma

1. Sol taraftaki menüden "API'ler ve Servisler" > "Kimlik Bilgileri"ni seçin.
2. Üstteki "Kimlik Bilgileri Oluştur" düğmesine tıklayın ve "OAuth istemci kimliği"ni seçin.
3. Uygulama türü olarak "Masaüstü uygulaması"nı seçin.
4. İstemci adı olarak "Zimmet Bakım Takip" girin.
5. Yeniden yönlendirme URI'si olarak `http://localhost:8080/callback` girin.
6. "Oluştur" düğmesine tıklayın.
7. İstemci kimliği (Client ID) ve istemci sırrı (Client Secret) bilgilerini not alın veya indirin.

## Adım 4: Uygulamayı Yapılandırma

1. `AuthService.cs` dosyasını açın ve aşağıdaki sabitleri güncelleyin:
   ```csharp
   private const string GoogleClientId = "YOUR_GOOGLE_CLIENT_ID"; // Google Cloud Console'dan alınan istemci kimliği
   private const string GoogleClientSecret = "YOUR_GOOGLE_CLIENT_SECRET"; // Google Cloud Console'dan alınan istemci sırrı
   ```

2. Bu değerleri, Google Cloud Console'dan aldığınız kimlik bilgileriyle değiştirin.

## Adım 5: Test Etme

1. Uygulamayı çalıştırın ve giriş ekranında "Google ile Giriş Yap" düğmesine tıklayın.
2. Bir Google hesabı seçin (OAuth onay ekranında eklediğiniz test kullanıcılarından biri olmalıdır).
3. İzin isteğini onaylayın.
4. Uygulama, Google kimlik doğrulamasını tamamlayıp ana ekrana yönlendirmelidir.

## Sorun Giderme

1. **URI uyumsuzluğu hatası**: Yeniden yönlendirme URI'sinin tam olarak `http://localhost:8080/callback` olarak yazıldığından emin olun.

2. **Yetkilendirme hatası**: OAuth onay ekranında doğru kapsamların eklendiğini ve uygulamanın "Harici" olarak yapılandırıldığını kontrol edin.

3. **Kullanıcı erişimi hatası**: Test modundayken, yalnızca test kullanıcıları olarak eklediğiniz e-posta adresleri uygulamaya giriş yapabilir. Daha fazla kullanıcının erişmesini istiyorsanız, onları test kullanıcıları olarak ekleyin veya uygulamayı yayınlayın.

## Üretim Ortamı İçin Notlar

Uygulamayı üretime taşımadan önce aşağıdaki adımları tamamlayın:

1. OAuth onay ekranını "Yayında" durumuna geçirin (bu, Google incelemesi gerektirebilir).
2. Uygulama kimlik doğrulama bilgilerini güvenli bir şekilde saklayın (örn. çevre değişkenleri veya şifreli yapılandırma dosyası). 