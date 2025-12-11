# Spor Salonu Yönetim Sistemi

Bu proje, Sakarya Üniversitesi Bilgisayar Mühendisliği Bölümü Web Programlama dersi kapsamında geliştirilmiş; ASP.NET Core MVC mimarisi üzerine kurulu kapsamlı bir spor salonu yönetim ve randevu sistemidir.

Proje, üyelerin antrenörlerden randevu alabilmesini, yöneticilerin salon süreçlerini takip edebilmesini ve Yapay Zeka (AI) entegrasyonu ile kişiselleştirilmiş egzersiz/beslenme tavsiyeleri sunulmasını amaçlamaktadır.

## Proje Özellikleri

### Üye Paneli
* **Randevu Sistemi:** Üyeler, müsait antrenörleri ve hizmetleri görüntüleyerek randevu oluşturabilir.
* **Akıllı Filtreleme:** Seçilen hizmet türüne göre (Örn: Yoga, Fitness) sadece ilgili uzmanlığa sahip antrenörler listelenir.
* **AI Danışman:** Google Gemini yapay zeka entegrasyonu sayesinde kullanıcılar; boy, kilo ve hedef bilgilerini girerek kişiye özel beslenme ve antrenman programı önerisi alabilir.
* **Randevu Takibi:** Kullanıcılar, oluşturdukları randevuların onay durumlarını takip edebilir.

### Admin Paneli
* **Varlık Yönetimi (CRUD):** Antrenör ve Hizmet kayıtları için ekleme, silme, güncelleme ve listeleme işlemleri yapılabilir.
* **Mesai Yönetimi:** Antrenörlerin çalışma saatleri (Başlangıç-Bitiş) sistem üzerinden tanımlanabilir.
* **Randevu Onay Mekanizması:** Üyelerden gelen randevu talepleri yönetici tarafından onaylanabilir veya reddedilebilir.
* **Raporlama:** REST API ve LINQ sorguları kullanılarak hazırlanan panel üzerinden, salonun anlık durumu ve istatistikleri görüntülenebilir.

### Teknik Detaylar ve Kontroller
* **Çakışma Kontrolü:** Aynı antrenöre, aynı tarih ve saat aralığında mükerrer randevu alınması engellenmiştir.
* **Zaman Yönetimi:** Salonun kapalı olduğu saatlere (09:00 - 22:00 dışı) ve antrenörün mesai saatleri dışına randevu alınması engellenmiştir.
* **Veri Bütünlüğü:** İlişkisel veritabanı yapısı gereği, aktif randevusu bulunan antrenör veya hizmetlerin silinmesi kısıtlanmıştır.
* **Rol Bazlı Yetkilendirme:** Sistemde Admin ve Üye rolleri bulunmaktadır. Sayfa ve aksiyon bazlı yetkilendirme (Authorization) uygulanmıştır.
* **Veri Doğrulama:** Hem istemci (Client-side) hem de sunucu (Server-side) tarafında veri doğrulama işlemleri yapılmaktadır.

## Kullanılan Teknolojiler

* **Framework:** ASP.NET Core 8.0 MVC
* **Programlama Dili:** C#
* **Veritabanı:** SQL Server / Entity Framework Core (Code First)
* **Ön Yüz:** HTML5, CSS3, Bootstrap 5, JavaScript (AJAX/jQuery)
* **Yapay Zeka Servisi:** Google Gemini 2.0 Flash API
* **Kimlik Doğrulama:** ASP.NET Core Identity

## Kurulum Adımları

1. Projeyi yerel bilgisayarınıza klonlayın veya indirin.
2. `appsettings.json` dosyasında bulunan `ConnectionStrings` alanını, kendi SQL Server yapılandırmanıza göre düzenleyin.
3. Package Manager Console üzerinden aşağıdaki komutu çalıştırarak veritabanını oluşturun:
   `Update-Database`
4. Projeyi çalıştırın.

## Varsayılan Giriş Bilgileri (Admin)

Proje gereksinimleri doğrultusunda tanımlanan yönetici hesabı:

* **Email:** ogrencinumarasi@sakarya.edu.tr
* **Şifre:** sau

---

### Hazırlayan

**Adı Soyadı:** Hakan Efe Tüysüz
**Bölüm:** Bilgisayar Mühendisliği
