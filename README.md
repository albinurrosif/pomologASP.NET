# 🍅 Pomolog API (Backend)

Pomolog adalah aplikasi _time-tracking_ berbasis teknik Pomodoro yang dirancang untuk membantu pengguna tetap fokus dan mengelola tugas harian mereka. Repositori ini adalah RESTful API Backend untuk Pomolog, dibangun dengan menggunakan .NET 10.

## 🚀 Tech Stack

- **Framework:** ASP.NET Core Web API (.NET 10)
- **Database:** PostgreSQL
- **ORM:** Entity Framework Core
- **Security:** JSON Web Tokens (JWT) & BCrypt Password Hashing
- **Documentation:** OpenAPI (Scalar UI)
- **CI/CD:** GitHub Actions

## ⚙️ Fitur Utama

1. **Authentication & Security:** Register, Login, dan pengamanan _endpoint_ menggunakan otorisasi JWT Berbasis Kepemilikan (_Ownership Authorization_).
2. **Task Management:** CRUD (Create, Read, Update, Delete) untuk daftar tugas.
3. **Strict Pomodoro Architecture:** Memisahkan entitas Tugas dan Sesi Waktu menggunakan Junction Table untuk pelacakan analitik yang akurat dan fleksibel.
4. **Analytics:** Perhitungan _real-time_ untuk total tugas selesai dan akumulasi waktu fokus pengguna.

## 📋 Prasyarat (Prerequisites)

Pastikan sistem Anda telah menginstal:

- .NET 10 SDK
- PostgreSQL

## 🛠️ Cara Menjalankan di Lokal (Local Setup)

**1. Clone repositori**

    git clone https://github.com/USERNAME_KAMU/PomologASP.NET.git
    cd PomologASP.NET

**2. Atur Konfigurasi Database & JWT**
Buat file appsettings.Development.json di dalam folder Pomolog.Api dan tambahkan kredensial database PostgreSQL serta kunci JWT Anda:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=PomologDb;Username=postgres;Password=YOUR_DB_PASSWORD"
  },
  "JwtSettings": {
    "Secret": "KunciRahasiaSuperPanjangMinimal32KarakterUntukPomolog!",
    "Issuer": "PomologApi",
    "Audience": "PomologClient"
  }
}
```

**3. Jalankan Migrasi Database**

    dotnet ef database update --project Pomolog.Api/Pomolog.Api.csproj

**4. Jalankan Aplikasi**

    dotnet run --project Pomolog.Api/Pomolog.Api.csproj

**5. Buka Dokumentasi API**
Akses API Contract dan _testing tool_ (Scalar) di browser:

```bash
http://localhost:5067/scalar/v1
```

_(Catatan: Port dapat berbeda, sesuaikan dengan output terminal Anda)._

---

_My first ASP.NET Web API project — built to learn, experiment, and grow as a backend developer.._
