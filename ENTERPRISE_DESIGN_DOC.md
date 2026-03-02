# PROJEYİ CANAVARA DÖNÜŞTÜRME PLANI 🚀

Dilara, harika bir yere geldik. Sen şimdi "Bu sadece çalışsın yetmez, gerçek bir şirket bunu alıp kullansın, SaaS (Software as a Service) olsun" diyorsun. İşte gerçek mühendislik burada başlar.

Kendimi 15 yıllık bir .NET Mimarı gibi konumlandırıyorum ve sana **GitHub Intelligence SaaS** projesi için "olmazsa olmaz" kurumsal özellikleri ekliyorum. Sadece GitHub'dan veri çekmek yetmez; üzerine değer katmamız lazım.

## 1. KATMA DEĞER NEDİR? NEDEN BİR ŞİRKET BUNU KULLANSIN?
Şirketler ham veriyi (raw data) GitHub'dan zaten çeker. Bizim ürünümüzün değeri **ANALİZ ve ZEKA** olacak.

### 🌟 Yeni Ekleyeceğimiz Kurumsal Özellikler (Game Changer)

1.  **Developer Scoring System (Geliştirici Puanlama Motoru):**
    *   Sadece "Repo sayısı 5" demek yetmez.
    *   Mantık: Code kalitesi, PR (Pull Request) kabul oranı, commit sıklığı, kullandığı dillerin zorluğu... Bunların hepsini bir formülle hesaplayıp developera **"Seniority Score" (Kıdem Puanı)** vereceğiz. İK firmaları buna bayılır.

2.  **Team Compatibility Analysis (Ekip Uyumluluk Analizi):**
    *   İki developeri (örn: Dilara ve Ahmet) yan yana getirelim.
    *   Kod stilleri, çalışma saatleri (commit saatleri), kullandıkları teknolojiler uyuşuyor mu?
    *   Çıktı: "%85 Uyumlu - İkisi de C# ve Backend odaklı."

3.  **Technology Radar (Teknoloji Radarı):**
    *   Hangi kütüphaneleri kullanıyorlar?
    *   "Eski teknoloji mi kullanıyorlar, yoksa modern stack mi?" (Örn: .NET Framework vs .NET 8).
    *   Şirketler işe alımda buna bakar.

4.  **Export as PDF/Excel (Raporlama Hizmeti):**
    *   Analiz sonucunu güzel bir rapor halinde indirme.

## 2. SYSTEM ARCHITECTURE (MİMARİ TASARIM) - SAVAŞ ALANI

Mevcut Clean Architecture yapımızı, SaaS (Software as a Service) ihtiyaçlarına göre güncelliyoruz.

### 🛠️ Katman Sorumlulukları (Strict Rules)

#### 1. Domain Layer (Çekirdek - Kutsal Alan)
*   **Ne Yapar:** Sadece Kurallar (Rules) ve Varlıklar (Entities).
*   **Kural:** Dış dünyadan habersizdir. NuGet paketi bile yüklenmez (mümkünse).
*   **Örnek Logic:** "Bir developerın Senior olması için en az 5000 commit ve %90 PR acceptance gerekir." (Burası değişmez iş kuralıdır).

#### 2. Application Layer (Orkestra Şefi)
*   **Ne Yapar:** Emirleri yönetir.
*   **Kural:** Veritabanına nasıl bağlanacağını, API'ye nasıl gideceğini bilmez. Sadece arayüzleri (interface) kullanır.
*   **Örnek Logic:** `GetDeveloperScoreQuery` geldiğinde -> `IGitHubService`'e git -> veriyi al -> `IScoringEngine`'e ver -> hesapla -> `ICacheService`'e yaz -> DTO'ya çevirip dön.

#### 3. Infrastructure Layer (Dış İlişkiler Bakanlığı)
*   **Ne Yapar:** Kirli işler. API çağrıları, dosya okuma, mail atma.
*   **Resilience (Dayanıklılık) Merkezi:**
    *   GitHub yanıt vermedi mi? -> **Polly** ile 3 kere dene, bekle (Exponential Backoff).
    *   Sürekli mi hata veriyor? -> **Circuit Breaker** ile şalteri indir, sistemi koru.
    *   GitHub kotamız bitti mi? -> **Rate Limiter** devreye girsin, "Lütfen 1 saat sonra tekrar deneyin" desin.

#### 4. Persistence Layer (Hafıza Merkezi)
*   **Ne Yapar:** Veriyi saklar.
*   **Teknoloji:** Entity Framework Core (PostgreSQL veya SQL Server).
*   **Özellik:** Sadece "Create, Read, Update, Delete" yapmaz. **Unit of Work** deseni ile "Ya hepsi kaydolur ya hiçbiri" (Transaction) garantisi verir.

#### 5. Presentation / API Layer (Vitrin)
*   **Ne Yapar:** Müşteriyi karşılar, güvenliği sağlar.
*   **Özellik:**
    *   **Versioning:** `/api/v1/analysis` ve `/api/v2/analysis` destekler.
    *   **Auth:** JWT (JSON Web Token) ile "Kimsin?" kontrolü.
    *   **Documentation:** Swagger ile "Ben nasıl çalışırım?" rehberi.

## 3. Deployment & Cloud Strategy (Bulut Stratejisi)

Gerçek bir SaaS projesi "sunucuda bir klasörde" çalışmaz. Konteynerlerde çalışır.

1.  **Dockerization:**
    *   Uygulamayı küçük, taşınabilir bir kutuya (container) koyacağız.
    *   Avantaj: "Benim bilgisayarımda çalışıyordu" bahanesi biter. Her yerde aynı çalışır.

2.  **Health Checks (Sağlık Kontrolü):**
    *   `/health` endpoint'i ekleyeceğiz.
    *   Kubernetes veya Load Balancer buraya sürekli "Nasılsın?" diye sorar.
    *   Cevap: "Database OK, Redis OK, GitHub API OK". Biri bozuksa sistem uyarı verir.

3.  **Observability (Gözlemlenebilirlik):**
    *   **Log:** "Kullanıcı X hata aldı" (Serilog).
    *   **Metric:** "Şu an saniyede 50 istek geliyor, CPU %80" (Prometheus).
    *   **Trace:** "İstek API'ye girdi -> Database'e gitti -> Redis'e gitti -> Döndü" (OpenTelemetry).

## 4. Yol Haritası Güncellemesi (Sıradaki Hamleler)

Artık sadece "GitHub'dan veri çek" demiyoruz. Şunu diyoruz:

1.  **Domain'i Genişlet:** `ScoreCalculator` mantığını kur.
2.  **Application'a CQRS:** (Command Query Responsibility Segregation)
    *   Veri okuma (Query) ile veri değiştirme (Command) işlerini ayıracağız. Bu çok ileri seviye bir tekniktir.
3.  **Infrastructure'a Güç Kat:** Polly politikalarını (Policy) merkezi bir yerden yönet.

Bu vizyon seni heyecanlandırdı mı? "Basit öğrenci projesi"nden çıktık, şu an "Yatırımcıya sunum yapılacak proje" seviyesindeyiz.

**Onayınla birlikte Domain Entity'lerini ve puanlama mantığını kodlamaya başlıyorum.**
