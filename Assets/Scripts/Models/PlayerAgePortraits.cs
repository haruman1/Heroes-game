using UnityEngine;

/// <summary>
/// Menampung set sprite portrait karakter berdasarkan 4 kelompok usia:
/// 1. Remaja / Dewasa Awal (Usia 18 - 24)
/// 2. Dewasa / Fokus Karier (Usia 25 - 34)
/// 3. Paruh Baya / Karier & Keluarga (Usia 35 - 44)
/// 4. Tua / Senior / Dewasa Madya (Usia 45+)
/// </summary>
[System.Serializable]
public struct PlayerAgePortraits
{
    [Header("Sprite per Rentang Usia")]
    [Tooltip("Sprite portrait untuk usia Remaja / Dewasa Awal (18–24 tahun)")]
    public Sprite portraitRemaja;

    [Tooltip("Sprite portrait untuk usia Dewasa / Fokus Karier (25–34 tahun)")]
    public Sprite portraitDewasa;

    [Tooltip("Sprite portrait untuk usia Paruh Baya / Karier & Keluarga (35–44 tahun)")]
    public Sprite portraitParuhBaya;

    [Tooltip("Sprite portrait untuk usia Tua / Senior / Dewasa Madya (45+ tahun)")]
    public Sprite portraitTua;

    /// <summary>
    /// Mengambil sprite yang sesuai dengan usia pemain.
    /// Jika sprite khusus usia tertentu belum diisi di Inspector, akan otomatis menggunakan fallback yang tersedia.
    /// </summary>
    public Sprite GetSpriteForAge(int age, Sprite fallbackDefault = null)
    {
        Sprite selected = null;

        if (age >= 18 && age <= 24)
            selected = portraitRemaja;
        else if (age >= 25 && age <= 34)
            selected = portraitDewasa;
        else if (age >= 35 && age <= 44)
            selected = portraitParuhBaya;
        else if (age >= 45)
            selected = portraitTua;
        else
            selected = portraitRemaja;

        // Fallback urutan pencarian jika sprite rentang usia tertentu kosong
        if (selected == null) selected = portraitRemaja;
        if (selected == null) selected = portraitDewasa;
        if (selected == null) selected = portraitParuhBaya;
        if (selected == null) selected = portraitTua;
        if (selected == null) selected = fallbackDefault;

        return selected;
    }
}
