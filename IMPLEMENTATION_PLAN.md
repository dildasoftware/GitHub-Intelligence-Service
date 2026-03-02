# Profesyonel .NET API Consumer - Uygulama Planı

Bu belge, **GitHub Intelligence Service** (GitHub İstihbarat Servisi) projesinin mimarisini, teknoloji yığınını ve adım adım geliştirme yol haritasını içerir.

## 1. Proje Özeti
**İsim:** GitHub Intelligence Service  
**Amaç:** GitHub API'sini tüketen, repo ve geliştirici analizleri yapan, kurumsal seviyede dayanıklı, güvenli ve ölçeklenebilir bir entegrasyon servisi geliştirmek.
**Farkı Nedir?:** Basit bir API çağırma uygulaması değildir. Hata toleransı (resilience), önbellekleme (caching), arka plan işlemleri ve yapılandırılmış loglama gibi "Enterprise" özellikler barındırır.

## 2. Teknoloji Yığını (Tech Stack)

| Bileşen | Teknoloji | Amaç |
|-----------|------------|---------|
| **Dil** | C# 12 / .NET 9 | Ana geliştirme platformu |
| **Mimari** | Clean Architecture | Sorumlulukların ayrılması (Domain, Application, Infra, API) |
| **Veritabanı** | PostgreSQL veya SQL Server | Analiz verilerini saklamak için |
| **ORM** | Entity Framework Core | Veritabanı işlemleri için |
| **Dayanıklılık** | Polly | Hata yönetimi (Retry, Circuit Breaker) |
| **Önbellek** | Redis | Yüksek performanslı dağıtık önbellekleme |
| **Loglama** | Serilog | Yapılandırılmış loglama (JSON formatında) |
| **Arka Plan** | Hangfire | Zamanlanmış görevler (örn: her gece veri çekme) |
| **Test** | xUnit, Moq | Birim ve entegrasyon testleri |
| **Dokümantasyon** | Swagger / OpenAPI | API dökümantasyonu |
| **Container** | Docker | Sorunsuz dağıtım (deployment) ortamı |

## 3. Mimari Tasarım (Clean Architecture)

Çözüm, bağımlılık kurallarına uymak için 4 ana katmana ayrılmıştır (Bağımlılıklar içe, Domain'e doğru bakar).

```
src/
├── 1. Domain/                  (Çekirdek Mantık - Dış bağımlılık YOK)
│   ├── Entities/               (Veritabanı Modelleri, örn: RepositoryAnalysis)
│   ├── Interfaces/             (Soyutlamalar, örn: IGitHubApiClient)
│   └── Exceptions/             (Özel hata sınıfları)
│
├── 2. Application/             (İş Mantığı - Domain'e bağımlı)
│   ├── DTOs/                   (Veri Transfer Objeleri)
│   ├── Interfaces/             (Servis sözleşmeleri)
│   ├── Services/               (İş mantığı uygulamaları)
│   └── Mappers/                (AutoMapper profilleri)
│
├── 3. Infrastructure/          (Dış Dünya - Application'a bağımlı)
│   ├── ExternalServices/       (GitHub API entegrasyonu)
│   ├── Persistence/            (Veritabanı erişimi, Repository'ler)
│   ├── Logging/                (Serilog konfigürasyonu)
│   └── Caching/                (Redis uygulaması)
│
└── 4. API/                     (Giriş Kapısı - App & Infra'ya bağımlı)
    ├── Controllers/            (REST Uç Noktaları)
    ├── Middlewares/            (Global Hata Yönetimi, Loglama)
    └── Program.cs              (Dependency Injection kurulumu)
```

## 4. Profesyonel Özellikler & SOLID Prensipleri

### SOLID Prensipleri Uygulaması
- **S (Single Responsibility):** API çağırma, Veritabanı kaydetme ve Loglama işlemleri ayrı sınıflarda olacak.
- **O (Open/Closed):** Dış servisler interface (arayüz) üzerinden kullanılacak, böylece kod değişimi yapmadan yeni özellik eklenebilecek.
- **L (Liskov Substitution):** GitHub servisi, genel bir `IExternalApiService` arayüzünü tam olarak karşılayacak.
- **I (Interface Segregation):** Tek devasa bir interface yerine, `IRepositoryAnalyzer`, `IUserFetcher` gibi küçük parçalı interface'ler kullanılacak.
- **D (Dependency Inversion):** Üst katmanlar (Application), somut sınıflara değil, soyutlamalara (Interface) bağımlı olacak.

### Kritik Profesyonel Özellikler
1.  **Resilience (Dayanıklılık - Polly):**
    -   **Retry Policy:** GitHub API 503 hatası verirse, sistem çökmez; 3 kez tekrar dener.
    -   **Circuit Breaker:** GitHub API tamamen çökerse, sistem sürekli istek atıp kaynak tüketmez; devreyi keser ve 30 sn bekler.
2.  **Caching Strategy (Redis):**
    -   Repo istatistiklerini 1 saat önbellekte tutar.
    -   Dış API'ye gitmeden önce *cache* kontrolü yapar.
3.  **Security (Güvenlik):**
    -   API Key'ler kod içinde saklanmaz (Vault veya User Secrets kullanılır).
    -   Kötü niyetli kullanımı engellemek için Rate Limiting uygulanır.
4.  **Observability (Gözlemlenebilirlik):**
    -   Her istek, bir `Correlation ID` ile loglanır. Böylece bir hatanın izi tüm sistemde sürülebilir.

## 5. Geliştirme Yol Haritası

### Faz 1: Temel (Tamamlandı)
- [x] Solution ve Proje Yapısının Kurulması (Clean Architecture).
- [x] Dependency Injection (Bağımlılık Enjeksiyonu) kurulumu.
- [x] Swagger ve Serilog entegrasyonu.

### Faz 2: Domain & Application Çekirdeği (Sıradaki Adım)
- [ ] Entity'lerin tanımlanması (Developer, Repository, CommitStats).
- [ ] Interface'lerin yazılması (IGitHubService).
- [ ] DTO'ların oluşturulması.

### Faz 3: Infrastructure - Dış API Entegrasyonu
- [ ] `HttpClient` ile `GitHubApiClient` kodlanması.
- [ ] Polly ile Retry ve Circuit Breaker mekanizmalarının eklenmesi.

### Faz 4: Veritabanı ve Önbellek
- [ ] Entity Framework Core ve Veritabanı kurulumu.
- [ ] Redis Caching mekanizmasının kodlanması.

### Faz 5: İyileştirme ve Test
- [ ] Global Exception Handler (Hata Yakalayıcı) eklenmesi.
- [ ] xUnit ile Birim Testleri (Unit Tests) yazılması.
- [ ] Uygulamanın Dockerize edilmesi.

---
**Sıradaki Adım:** Domain katmanı nesnelerini (Entity) oluşturmak.
