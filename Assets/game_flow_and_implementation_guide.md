# 📦 Panduan Setup Lengkap Per Scene — Heroes Game

Dokumen ini adalah **panduan setup lengkap & terurut** untuk setiap scene Unity di proyek Heroes Game.
Baca dari atas ke bawah. Jangan loncat-loncat.

---

## 🏛️ ARSITEKTUR SCENE (Baca Ini Dulu!)

Game ini menggunakan pola **Boot + Persistent Scene** yang dipakai oleh game-game profesional.

```
CoreScene  (Build Index 0 — Scene Pertama, TIDAK PERNAH Di-Unload)
    │
    │  [Saat Start, otomatis load MainMenu secara Additive]
    │
    ├── AudioManager       ← BGM/SFX tetap nyambung saat pindah level
    ├── SaveManager        ← Data player aman selama sesi game
    ├── DatabaseManager    ← Koneksi SQLite tidak dibuat ulang setiap scene
    ├── UIManager          ← Loading Screen, Fade, HUD Global
    ├── DialogueManager    ← State dialog tidak putus
    ├── ShopManager        ← Logika transaksi koin/booster
    └── JourneyBookManager ← Progress buku selalu tersinkronisasi
    
    └── [Scene Gameplay — berganti-ganti di atas CoreScene]
            ├── Main Menu
            ├── PilihCharacter
            ├── OpeningStory
            ├── LEVEL 1  (Boss ada DALAM scene ini, tidak pindah scene)
            ├── LEVEL 2  (Boss ada DALAM scene ini)
            ├── LEVEL 3
            ├── LEVEL 4
            ├── LEVEL 5
            └── LEVEL 6  (Boss Final ada DALAM scene ini)
```

### Bagaimana Perpindahan Scene Bekerja?

```
Pemain di Level 1 → Selesai → GameManager.MuatScene("LEVEL 2")
                                      │
                    ┌─────────────────┴──────────────────┐
                    │ 1. Fade Out (layar hitam)           │
                    │ 2. Tampilkan Loading Screen         │
                    │ 3. UnloadSceneAsync("LEVEL 1")      │  ← RAM bebas!
                    │ 4. System.GC.Collect()              │  ← Sampah dibersihkan
                    │ 5. LoadSceneAsync("LEVEL 2", Additive) ← CoreScene aman!
                    │ 6. SetActiveScene("LEVEL 2")        │
                    │ 7. Sembunyikan Loading Screen        │
                    │ 8. Fade In (layar terang kembali)   │
                    └────────────────────────────────────┘

    AudioManager ← TETAP ADA, BGM bisa cross-fade tanpa restart
    SaveManager  ← TETAP ADA, tidak perlu re-connect database
    UIManager    ← TETAP ADA, loading screen bisa ditampilkan kapan saja
```

### Apa Itu Boss Fight?

Boss berada **DALAM scene Level** yang sama (bukan scene terpisah baru).
Pemain berjalan ke ujung level → menyentuh `BossTriggerDoor` → 
Scene boss arena dimuat via `GameManager.MuatScene("Boss Arena Level 1")` →
Setelah boss kalah → Kembali ke level atau lanjut ke level berikutnya.
**State `GameState.BossFight` di-set saat masuk arena** sehingga sistem lain
(seperti BGM) bisa bereaksi terhadap perubahan state ini.

---

## ⚙️ LANGKAH 0 — SETUP CORESCENCE (WAJIB DILAKUKAN SEKALI)

### Langkah A: Buat Scene CoreScene

1. Di Unity, klik **File → New Scene → Empty**
2. Beri nama: **`CoreScene`** (atau nama file .unity: `CoreScene.unity`)
3. Simpan di folder: `Assets/Scenes/`

### Langkah B: Buat GameObject Manager di CoreScene

Di **Hierarchy** `CoreScene`, buat **7 GameObject kosong** berikut (masing-masing terpisah, sejajar di root):

---

### 📦 A. GameObject: `GameManager`
- **Script yang dipasang**: `GameManager.cs`
- **Inspector**:

  | Field | Isi |
  | :--- | :--- |
  | Persistent Scene Name | `CoreScene` |
  | Nama Scene Main Menu | `Main Menu` |
  | Nama Scene Character Select | `PilihCharacter` |
  | Nama Scene Opening Story | `OpeningStory` |
  | Nama Scene Level Pertama | `LEVEL 1` |

---

### 📦 B. GameObject: `AudioManager`
- **Script yang dipasang**: `AudioManager.cs`
- Tambahkan **3 komponen `AudioSource`** pada GameObject ini:

  | Slot Inspector | Isi |
  | :--- | :--- |
  | Music Source | AudioSource ke-1 (Loop=true, untuk BGM) |
  | SFX Source | AudioSource ke-2 (Loop=false, untuk SFX) |
  | Voice Over Source | AudioSource ke-3 (untuk Suara Dialog/Narasi) |

---

### 📦 C. GameObject: `DatabaseManager`
- **Script yang dipasang**: `DatabaseManager.cs`
- Tidak ada konfigurasi tambahan — sudah auto-setup SQLite.

---

### 📦 D. GameObject: `SaveManager`
- **Script yang dipasang**: `SaveManager.cs`
- Tidak ada konfigurasi tambahan.

---

### 📦 E. GameObject: `DialogueManager`
- **Script yang dipasang**: `DialogueManager.cs`
- **Catatan**: Tidak perlu assign `DialogueUI` di sini! `DialogueUI` di tiap scene akan mendaftarkan dirinya sendiri (Self-Registration via `Awake()`).
- Slot Portrait yang perlu diisi:

  | Field | Isi |
  | :--- | :--- |
  | Portrait Awan (Fallback) | Sprite wajah Awan default |
  | Portrait Rena (Fallback) | Sprite wajah Rena default |
  | Portrait Awan Age Set — Remaja | Sprite Awan usia 18-24 |
  | Portrait Awan Age Set — Dewasa | Sprite Awan usia 25-34 |
  | Portrait Awan Age Set — Paruh Baya | Sprite Awan usia 35-44 |
  | Portrait Awan Age Set — Tua | Sprite Awan usia 45+ |
  | Portrait Rena Age Set — *(dst)* | *(idem)* |

---

### 📦 F. GameObject: `UIManager`
- **Script yang dipasang**: `UIManager.cs`
- Buat **Canvas Global** sebagai child dari `UIManager` yang berisi panel-panel berikut:

  | Field di Inspector UIManager | Isi |
  | :--- | :--- |
  | Panel HUD | Panel HUD Gameplay (HP bar, koin, dll) |
  | Panel Pause Menu | Panel Pause Menu |
  | Panel Journey Book | Panel Journey Book / Buku Pengetahuan |
  | Panel Settings | Panel Pengaturan Audio & Tampilan |
  | Panel Loading Screen | Panel Loading Screen (Slider + teks %) |
  | Slider Progress Loading | Slider di dalam Panel Loading Screen |
  | Teks Progress Loading | TMP_Text persentase di Loading Screen |
  | Image Overlay Fade | Image hitam full-screen (alpha=0 di awal) |

  > [!NOTE]
  > Canvas di UIManager menggunakan **Sort Order tinggi** (misal: 100) agar selalu tampil di depan semua UI scene lain.

---

### 📦 G. GameObject: `ShopManager`
- **Script yang dipasang**: `ShopManager.cs`
- **Inspector**:

  | Field | Isi |
  | :--- | :--- |
  | URL Toko Itch.io | URL halaman itch.io game kamu |
  | SFX Sukses Beli | AudioClip suara "beli sukses" |
  | SFX Gagal Beli | AudioClip suara "koin kurang" |

- **Catatan tentang Shop sebagai Prefab**: Panel UI toko dibuat sebagai Prefab terpisah. Instantiasi prefab ini cukup dari scene mana pun yang membutuhkannya (Main Menu atau Pause Menu). `ShopManager` di CoreScene hanya mengurus logika transaksi, bukan tampilannya.

---

### 📦 H. GameObject: `JourneyBookManager`
- **Script yang dipasang**: `JourneyBookManager.cs`
- Konfigurasi sesuai Inspector `JourneyBookManager`.

---

### Langkah C: Daftarkan Semua Scene di Build Settings

**File → Build Settings → Add Open Scenes** (atau drag dari folder):

| Urutan | Nama Scene | Keterangan |
| :--- | :--- | :--- |
| **0** | `CoreScene` | ⚠️ Wajib pertama! Ini yang dijalankan saat game start |
| 1 | `Main Menu` | Scene Main Menu |
| 2 | `PilihCharacter` | Scene pemilihan karakter & usia |
| 3 | `OpeningStory` | Scene cerita pembuka |
| 4 | `LEVEL 1` | Level 1 (Boss Arena Level 1 ada di dalamnya) |
| 5 | `LEVEL 2` | Level 2 |
| 6 | `LEVEL 3` | Level 3 |
| 7 | `LEVEL 4` | Level 4 |
| 8 | `LEVEL 5` | Level 5 |
| 9 | `LEVEL 6` | Level 6 (Boss Final ada di dalamnya) |
| 10+ | `Boss Arena Level 1` | Scene arena boss Level 1 (jika terpisah) |
  | ... | *(dst)* | |

> [!IMPORTANT]
> `CoreScene` **HARUS di index 0**. Unity akan otomatis memuat scene index 0 pertama kali saat game dijalankan (baik di Editor maupun Build).

---

## 🎬 SCENE 1: `Main Menu.unity`

> Catatan: Scene ini akan **ditumpuk secara Additive di atas CoreScene**. 
> Jangan menaruh Manager apapun di sini — mereka sudah hidup di CoreScene.

### Cara Kerja Saat Game Dimulai

```
[Game Start] → CoreScene dimuat (index 0)
                    ↓
              GameManager.Start() berjalan
                    ↓
              Tidak ada scene gameplay lain?
                    ↓
              GameManager.MuatScene("Main Menu")
                    ↓
              [Main Menu tampil di atas CoreScene]
```

### GameObject yang Harus Ada di Hierarchy:

| GameObject | Script | Keterangan |
| :--- | :--- | :--- |
| `Canvas_MainMenu` | — | Canvas UI utama Main Menu |
| `_Bootstrapper` | `SceneBootstrapper.cs` | Untuk testing di Editor (lihat catatan) |

> [!TIP]
> **Cara Testing Tanpa Mulai dari CoreScene:**
> Pasang `SceneBootstrapper.cs` di GameObject `_Bootstrapper`. Lalu buka scene `Main Menu` di Editor dan tekan **Play** langsung. Bootstrapper akan otomatis memuat CoreScene di belakang layar agar Manager tersedia.

### Canvas UI Main Menu harus berisi:
- Tombol `New Game` → sambungkan ke `GameManager.MulaiGameBaru()`
- Tombol `Continue` → sambungkan ke `GameManager.Lanjutkan()`
- Tombol `Shop / Toko` → Instantiate prefab Shop UI
- Tombol `Settings` → sambungkan ke `UIManager.TampilkanSettings(true)`
- Tombol `Keluar` → sambungkan ke `GameManager.KeluarGame()`
- Panel Settings (bisa di UIManager Global atau local canvas)

---

## 🎭 SCENE 2: `PilihCharacter.unity`

> Persistent Manager dari CoreScene sudah otomatis ada, **tidak perlu dibuat ulang**.

### GameObject yang Harus Ada di Hierarchy:

| GameObject | Script | Keterangan |
| :--- | :--- | :--- |
| `RPGCharacterSelection` | `RPGCharacterSelection.cs` | Controller pemilihan karakter & usia |
| `Canvas_PilihCharacter` | — | Canvas UI pemilihan karakter |
| `_Bootstrapper` | `SceneBootstrapper.cs` | Untuk testing langsung dari scene ini |

### Inspector `RPGCharacterSelection`:

| Section | Field | Isi |
| :--- | :--- | :--- |
| Characters | Character Male | Data karakter Awan (Pria): nama, sprite, animator, dll. |
| Characters | Character Female | Data karakter Rena (Wanita): nama, sprite, animator, dll. |
| Age Range Selection UI | `ageRangePanel` | Panel tombol pilih rentang usia |
| Age Range Selection UI | `ageButton18_24` | Button usia 18-24 |
| Age Range Selection UI | `ageButton25_34` | Button usia 25-34 |
| Age Range Selection UI | `ageButton35_44` | Button usia 35-44 |
| Age Range Selection UI | `ageButton45_plus` | Button usia 45+ |
| Summary UI | `summaryPortrait` | Image portrait untuk rangkuman |
| Summary UI | `summaryNameText`, `summaryAgeText`, `summaryJobText` | Teks rangkuman |
| Summary UI | `summaryContinueButton` | Tombol lanjut → memicu muat OpeningStory |

---

## 🌅 SCENE 3: `OpeningStory.unity`

> Persistent Manager sudah otomatis ada, **tidak perlu dibuat ulang**.

### Langkah A: Siapkan Canvas Dialog

Buat Canvas UI dengan struktur berikut di Hierarchy:

```
Canvas_Dialog (Canvas)
└── DialogPanel (GameObject)
    ├── Background (Image — background chat box)
    ├── NamaBox (Image)
    │   └── TeksNama (TMP_Text)
    ├── TeksDialog (TMP_Text)
    ├── PortraitKiri (Image — untuk KU di kiri layar)
    ├── PortraitKanan (Image — untuk KP di kanan layar)
    ├── TombolLanjut (Button — klik untuk lanjut baris)
    └── TombolLewati (Button — klik untuk skip semua)
```

- Pasang script **`DialogueUI.cs`** pada `Canvas_Dialog` atau parent DialogPanel.
- Sambungkan semua komponen di atas ke Inspector `DialogueUI`.

---

### Langkah B: Sambungkan `DialogueManager` ke `DialogueUI`
- **TIDAK PERLU DRAG & DROP MANUAL!**
- `DialogueUI.cs` sudah dilengkapi `Awake()` yang memanggil `DialogueManager.Instance.SetDialogueUI(this)` secara otomatis saat scene ini dimuat.
- Pastikan saja komponen `DialogueUI.cs` sudah terpasang di Canvas ini.

---

### Langkah C: Siapkan Canvas Pilihan Interaktif

Buat Panel UI terpisah untuk Pop-up Pilihan:

```
Canvas_Dialog (sama)
└── ChoicePanel (GameObject — awalnya SetActive = false)
    ├── TeksPrompt (TMP_Text — "APAKAH KAMU INGIN MEMULAI PERJALANAN?")
    ├── ButtonYa (Button)
    └── ButtonTidak (Button)
```

---

### Langkah D: Pasang `OpeningStoryController`
- Buat **GameObject kosong** baru, beri nama `OpeningStoryController`.
- Pasang script **`OpeningStoryController.cs`**.
- Sambungkan slot Inspector:

  | Field | Isi |
  | :--- | :--- |
  | Choice Panel UI | Drag `ChoicePanel` dari Canvas |
  | Choice Prompt Text | Drag `TeksPrompt` TMP_Text |
  | Button Ya | Drag `ButtonYa` Button |
  | Button Tidak | Drag `ButtonTidak` Button |
  | Nama Scene Peta Perjalanan | *(tidak dipakai lagi, langsung ke Level 1)* |
  | Jumlah Booster Awal | `3` |

---

## ⚔️ SCENE 4 s/d 9: `LEVEL 1.unity` — `LEVEL 6.unity`

> Setiap level memiliki setup yang sama. Berikut adalah panduan untuk Level 1.
> Ulangi untuk Level 2–6 dengan `LevelDataSO` yang berbeda.

### Konsep Boss dalam Level

> [!IMPORTANT]
> **Boss berada DALAM scene Level yang sama!**
> Pemain tidak berpindah ke scene baru saat masuk boss. Boss Arena adalah **area berbeda di dalam peta Level yang sama**. Pintu masuknya diblokir oleh `BossTriggerDoor`.
> 
> Jika boss dikalahkan → `BossArenaController.WinArena()` → kembali ke scene Level lama atau muat Level berikutnya via `GameManager.MuatScene()`.

### GameObject yang Harus Ada di Hierarchy:

| GameObject | Script | Keterangan |
| :--- | :--- | :--- |
| `LevelManager` | `LevelManager.cs` | Mengatur alur level dari awal sampai selesai |
| `Player` | `playerJ.cs` | Karakter pemain yang bisa dikontrol |
| `InGamePageReader` | `InGamePageReader.cs` | Panel UI pembaca isian buku |
| `KnowledgeBarUI` | `KnowledgeBarUI.cs` | HUD bar progres halaman buku |
| `PintuBoss` | `BossTriggerDoor.cs` | Pintu masuk arena boss |
| `BossArena` | `BossArenaController.cs` | Controller pertarungan boss |
| Buku 1–10 | `BookCollectible.cs` | Item buku yang tersebar di level |
| Koin | `Coins_duit.cs` | Item koin yang tersebar di level |
| Bendera Finish | `Flag.cs` | Trigger akhir level |
| `_Bootstrapper` | `SceneBootstrapper.cs` | Untuk testing langsung dari scene ini |

---

### Inspector `LevelManager`:

| Field | Isi |
| :--- | :--- |
| Data Level | Drag asset `LevelData_Level1.asset` (ScriptableObject) |
| Player | Drag GameObject Player (atau biarkan kosong, auto-detect) |
| Knowledge Bar UI | Drag komponen `KnowledgeBarUI` |

---

### Cara Membuat Asset `LevelDataSO` untuk Level 1:
1. Di Project Window, klik kanan → **Create → Heroes Game → Level Data**.
2. Beri nama: `LevelData_Level1`.
3. Isi Inspector asset tersebut:

   | Field | Isi |
   | :--- | :--- |
   | Nomor Level | `1` |
   | Nama Scene | `LEVEL 1` |
   | Dialog Intro Awan | Drag asset `DialogueDataSO` berisi dialog intro Awan |
   | Dialog Intro Rena | Drag asset `DialogueDataSO` berisi dialog intro Rena |
   | Jumlah Halaman Dibutuhkan | `10` |
   | Level Berikutnya | Drag asset `LevelData_Level2` |
   | BGM Level | Drag file musik `.mp3` untuk level ini |

---

### Setup `BossTriggerDoor` (Pintu Masuk Arena Boss):
1. Buat GameObject pintu masuk area boss di ujung level.
2. Pasang `BoxCollider2D` → centang **Is Trigger = true**.
3. Pasang `BossTriggerDoor.cs`.
4. Isi Inspector:

   | Field | Isi |
   | :--- | :--- |
   | Arena Scene Name | Nama scene boss arena (misal: `Boss Arena Level 1`) ATAU kosong jika boss ada di scene yang sama |
   | Is Boss Mode | ✅ Centang |
   | Prompt Panel | Panel pop-up "Masuk?" |

5. Tombol **"Masuk"** di prompt → sambungkan ke `BossTriggerDoor.OnConfirmEnter()`
6. Tombol **"Batal"** → sambungkan ke `BossTriggerDoor.OnCancelEnter()`

> [!NOTE]
> `BossTriggerDoor.OnConfirmEnter()` sudah diperbarui untuk menggunakan `GameManager.Instance.MuatScene()` agar CoreScene tetap aman saat berpindah ke arena boss.

---

### Setup `BossArenaController`:
1. Buat area bos dengan Generator platform.
2. Pasang `BossArenaController.cs` dan `BossArenaGenerator.cs`.
3. Isi Inspector:

   | Field | Isi |
   | :--- | :--- |
   | Arena Name | Nama arena (misal: "Goa Kegelapan") |
   | Level Number | Nomor level (1, 2, 3, ...) |
   | Boss Prefab | Prefab bos yang akan di-spawn |
   | Player Prefab | Prefab player (atau kosong jika sudah ada di scene) |
   | Return Scene Name | Nama scene level (misal: `LEVEL 1`) |
   | Win Scene Name | Nama scene setelah menang (misal: `LEVEL 2`) atau kosong |

---

## 📋 RINGKASAN CHECKLIST SETUP PER SCENE

| Scene | Wajib Ada | Aset Yang Perlu Dibuat |
| :--- | :--- | :--- |
| `CoreScene.unity` | GameManager, AudioManager, DatabaseManager, SaveManager, UIManager, DialogueManager, ShopManager, JourneyBookManager | Canvas Global UI |
| `Main Menu.unity` | Canvas UI, Bootstrapper | — |
| `PilihCharacter.unity` | RPGCharacterSelection, Canvas UI, Bootstrapper | — |
| `OpeningStory.unity` | OpeningStoryController, Canvas Dialog + DialogueUI, Canvas ChoicePanel, Bootstrapper | *(opsional)* VideoSequenceSO, Voice Audio clips |
| `LEVEL 1.unity` | LevelManager, Player, InGamePageReader, BookCollectible x10, Flag, KnowledgeBarUI, PintuBoss, BossArena, Bootstrapper | `LevelDataSO`, `DialogueDataSO` Intro & Outro, `BookData` x10 |
| `LEVEL 2.unity` s/d `LEVEL 6.unity` | *(sama dengan Level 1)* | `LevelDataSO` baru per level |
