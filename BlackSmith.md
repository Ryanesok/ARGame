# BlackSmith - Game Design Document

## 1. Ringkasan Game

**BlackSmith** adalah game augmented reality (AR) Android bertema penempaan besi. Pemain menggunakan tiga marker fisik untuk memanaskan ingot, menempa ingot dengan palu, dan menyelesaikan proses hingga berubah menjadi pedang.

Game menggunakan pendekatan marker-based AR. Posisi dan jarak antar-marker menentukan perubahan state gameplay, sedangkan animasi, model 3D, UI, dan audio memberikan umpan balik kepada pemain.

## 2. Target Platform

- Platform utama: Android
- Minimum Android SDK: 29
- Target Android SDK: 31
- Target arsitektur: ARM64
- Application ID: `com.ryanesok.ARgame`
- Scene build order:
  1. `Assets/Scenes/MainMenu.unity`
  2. `Assets/Scenes/game.unity`

## 3. Tech Stack

| Komponen | Teknologi / Versi |
|---|---|
| Game engine | Unity 2023.1.0a21 |
| Rendering target | Unity 3D Android |
| AR framework | Vuforia Engine 11.4.4 |
| Scripting | C# |
| Android backend | IL2CPP |
| UI | Unity uGUI dan TextMesh Pro 3.0.6 |
| Animation | Unity Animator dan Timeline 1.8.0 |
| Input / package support | Unity Input System 1.11.2 melalui dependency Vuforia |
| Audio | Unity AudioSource, AudioClip, dan PlayerPrefs |
| Testing | Unity Test Framework 1.3.0, NUnit |
| Visual scripting package | Unity Visual Scripting 1.7.8 |

Package utama tercatat pada [Packages/manifest.json](Packages/manifest.json). Versi Editor proyek tercatat pada [ProjectSettings/ProjectVersion.txt](ProjectSettings/ProjectVersion.txt).

## 4. Struktur Project

- `Assets/Scenes`
  - `MainMenu.unity`: menu awal dan pengaturan.
  - `game.unity`: scene utama gameplay AR.
- `Assets/Scripts`
  - `CombineManager.cs`: deteksi marker, jarak, dan state kombinasi.
  - `ForgingManager.cs`: progres penempaan dan penyelesaian pedang.
  - `GamePlayUI.cs`: status proses dan kontrol UI gameplay.
  - `AnimationProgressUI.cs`: progress bar animasi dan orientasi UI terhadap kamera.
  - `audioManager.cs`: BGM, SFX, singleton audio, dan volume tersimpan.
  - `SettingsPanel.cs` dan `AudioSetting.cs`: konfigurasi volume.
- `Assets/Model`
  - Model palu, ingot, dan aset 3D pendukung.
- `Assets/sound`
  - BGM, suara api, suara palu, dan suara ingot.
- `Assets/Resources/AudioManager.prefab`
  - Prefab AudioManager yang dibuat sebelum scene dimulai.
- `Assets/Tests`
  - EditMode dan PlayMode tests untuk menjaga perilaku gameplay selama optimalisasi.
- `Assets/Scripts/ARGame.Runtime.asmdef`
  - Assembly runtime agar script produksi terpisah dari assembly test.

## 5. Konsep Gameplay

### Tujuan Pemain

Menyelesaikan penempaan ingot hingga menjadi pedang melalui urutan:

1. Menempatkan marker api dekat marker ingot.
2. Menunggu animasi pemanasan selesai.
3. Menempatkan marker palu dekat marker ingot panas.
4. Menunggu animasi penempaan selesai.
5. Mengulangi proses penempaan sampai lima tahap.

### Marker

- **Marker Api**: memanaskan ingot.
- **Marker Ingot**: objek utama yang diproses.
- **Marker Palu**: menempa ingot panas.

### Aturan Jarak

- Jarak kombinasi: kurang dari `0.21` satuan Unity.
- Jarak pemisahan: lebih dari `0.22` satuan Unity.
- Selisih jarak tersebut memberikan hysteresis agar state tidak mudah berkedip ketika marker bergerak di sekitar batas.

### State Gameplay

1. **Ingot Dingin**
   - Model api, ingot, dan palu tampil secara terpisah.
   - UI meminta pemain memanaskan ingot.

2. **Api + Ingot**
   - Model api dan ingot asli disembunyikan.
   - Model ingot panas ditampilkan.
   - Animasi pemanasan dan SFX heating dimainkan.

3. **Ingot Panas**
   - Setelah event animasi `FinishHeating`, ingot dianggap panas.
   - Warna ingot berubah menjadi warna panas.
   - Ingot siap diproses dengan palu.

4. **Palu + Ingot**
   - Model palu dan ingot asli disembunyikan.
   - Model palu besi ditampilkan.
   - Animasi penempaan dan SFX forging dimainkan jika state valid.

5. **Penempaan Selesai**
   - Setiap event `FinishForging` yang valid menambah hitungan penempaan.
   - Model ingot visual memanjang dan mengecil sesuai tahap.
   - Setelah lima tahap, model ingot asli disembunyikan dan model pedang ditampilkan.
   - SFX pedang dimainkan.

## 6. Petunjuk Gameplay untuk Pemain

1. Buka aplikasi dan tekan **Play** pada menu utama.
2. Izinkan kamera digunakan jika Android memintanya.
3. Arahkan kamera ke marker api, marker ingot, dan marker palu.
4. Dekatkan marker api dengan marker ingot sampai status berubah menjadi **Ingot Panas**.
5. Tunggu animasi pemanasan selesai.
6. Dekatkan marker palu dengan marker ingot panas.
7. Tunggu setiap animasi penempaan selesai.
8. Ulangi sampai indikator penempaan mencapai `5/5`.
9. Pedang akan muncul ketika proses selesai.
10. Gunakan menu pengaturan untuk mengubah volume BGM dan SFX.

Jika marker sulit terdeteksi:

- Gunakan pencahayaan yang cukup.
- Pastikan marker tidak terlipat atau tertutup.
- Jaga marker tetap berada dalam bidang pandang kamera.
- Hindari menggerakkan marker terlalu cepat.
- Pisahkan marker sedikit jika model terus berpindah antara state gabung dan terpisah.

## 7. Arsitektur Sistem

### CombineManager

`CombineManager` menjadi state controller untuk interaksi marker. Pemeriksaan jarak dilakukan setiap frame menggunakan `sqrMagnitude` agar tidak melakukan operasi akar kuadrat pada jalur normal. Logging jarak hanya aktif di Editor atau Development Build.

Audio fallback mengambil `AudioManager.Instance` ketika referensi Inspector kosong. Hal ini menjaga scene tetap dapat memainkan audio meskipun field `audioManager` belum diisi secara serialized.

### ForgingManager

`ForgingManager` menyimpan jumlah penempaan, mengatur perubahan scale ingot, dan mengaktifkan model pedang ketika jumlah tahap mencapai lima.

### GamePlayUI

UI hanya menulis ulang teks ketika count atau state gameplay berubah. Ini mengurangi assignment UI berulang pada setiap frame.

### AudioManager

AudioManager dibuat dari prefab di `Resources`, dipertahankan antar-scene dengan `DontDestroyOnLoad`, dan menyimpan volume dengan `PlayerPrefs`. Penyimpanan preference ditunda sampai aplikasi pause, quit, atau panel setting dihancurkan agar slider tidak memanggil operasi disk pada setiap perubahan nilai.

### Material Rendering

Perubahan warna ingot menggunakan `MaterialPropertyBlock`, sehingga tidak membuat material instance baru dan tidak memodifikasi shared material asset.

## 8. Testing dan Regression Safety

Test utama berada di [Assets/Tests/EditMode/GameplayLogicTests.cs](Assets/Tests/EditMode/GameplayLogicTests.cs). Cakupan utamanya:

- Inisialisasi `ForgingManager`.
- Perubahan scale ingot per tahap.
- Penyelesaian pedang pada tahap kelima.
- Guard agar hitungan tidak melewati maksimum.
- Clamp dan persistensi volume.
- State heating dan warna ingot.

Assembly test dipisahkan dari runtime melalui [GameplayEditModeTests.asmdef](Assets/Tests/EditMode/GameplayEditModeTests.asmdef). Assembly runtime menggunakan [ARGame.Runtime.asmdef](Assets/Scripts/ARGame.Runtime.asmdef). EditMode dan PlayMode test telah dijalankan melalui Unity Test Runner dan dinyatakan passed selama pengerjaan.

## 9. Kendala Selama Pengerjaan

### 9.1 Unity Editor Versi Alpha

Proyek menggunakan Unity `2023.1.0a21`, bukan versi LTS. Ini meningkatkan risiko perbedaan perilaku pada compiler, Test Runner, linker, dan Android build pipeline. Migrasi ke Unity LTS direkomendasikan sebelum rilis produksi, setelah kompatibilitas Vuforia diverifikasi.

### 9.2 Test NUnit Masuk ke Android Build

Awalnya file test berada di assembly default sehingga `nunit.framework` ikut diproses oleh UnityLinker ketika build Android. Error yang muncul:

```text
Failed to resolve assembly: 'nunit.framework'
```

Masalah diatasi dengan assembly definition Editor-only untuk test dan assembly runtime terpisah.

### 9.3 Referensi Assembly Package

Setelah runtime assembly dibuat, dependency TextMesh Pro, uGUI, dan Vuforia perlu dideklarasikan secara eksplisit. Tanpa referensi tersebut, `TMPro` dan tipe package lain tidak dapat ditemukan oleh compiler.

### 9.4 Referensi Audio Scene Kosong

Field `audioManager` pada `CombineManager` dan `ForgingManager` ditemukan kosong pada serialized scene. Fallback ke `AudioManager.Instance` ditambahkan agar audio tetap terhubung saat runtime.

### 9.5 Material Instantiation pada EditMode Test

Penggunaan `renderer.material` membuat material instance baru ketika test dijalankan dalam EditMode. Ini memunculkan warning material leak. Implementasi diganti ke `MaterialPropertyBlock`, dan test membaca property block tanpa membuat material instance baru.

### 9.6 Lifecycle MonoBehaviour pada EditMode

Pemanggilan `SendMessage("Start")` dalam EditMode menimbulkan assertion `ShouldRunBehaviour()`. Test kemudian memanggil lifecycle method secara terkontrol melalui helper refleksi agar tidak memaksa Unity menjalankan lifecycle runtime di luar konteksnya.

### 9.7 Batch Test Runner

Unity batch-mode runner sempat tidak menghasilkan `TestResults.xml` dan berhenti saat refresh/compile. Validasi yang digunakan adalah Unity Test Runner di Editor, yang berhasil menjalankan EditMode dan PlayMode tests dengan status passed.

## 10. Hasil Optimalisasi

- Perhitungan jarak marker menggunakan `sqrMagnitude`.
- Logging runtime jarak dibatasi untuk Editor/Development Build.
- UI gameplay tidak diperbarui ulang jika state tidak berubah.
- Progress bar tidak menulis nilai yang sama setiap frame.
- `PlayerPrefs.Save()` tidak dipanggil pada setiap gerakan slider.
- Audio reference memiliki fallback singleton.
- Material ingot tidak diinstansiasi ketika warna berubah.
- Debug overlay sementara telah dihapus dari project.
- Test dipisahkan dari assembly Android runtime.

## 11. Rekomendasi Tahap Berikutnya

1. Migrasikan project ke Unity LTS yang kompatibel dengan Vuforia.
2. Build dan uji pada perangkat Android target dengan Development Build.
3. Profiling menggunakan Unity Profiler dan Android Profiler.
4. Audit ukuran tekstur, mesh, animasi, dan audio.
5. Uji kehilangan tracking, pergantian scene, pause/resume aplikasi, dan perangkat dengan spesifikasi rendah.
6. Pastikan folder hasil test dan artefak `Library` tidak ikut didistribusikan sebagai source project.
7. Buat keystore Android produksi dan naikkan bundle version sebelum rilis.
