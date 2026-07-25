using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ShopUIController — UI Tampilan Toko Item Boost di Main Menu.
/// 
/// Menyediakan UI untuk melihat saldo koin/booster, memilih item,
/// dan melakukan transaksi via Koin In-Game atau Uang Asli / Itch.io.
/// </summary>
public class ShopUIController : MonoBehaviour
{
    [Header("Panel Utama Toko")]
    [SerializeField] private GameObject panelToko;
    [SerializeField] private Button tombolTutupToko;

    [Header("Tampilan Saldo Top Bar")]
    [SerializeField] private TMP_Text teksSaldoKoin;
    [SerializeField] private TMP_Text teksJumlahBooster;

    // ─── STUKTUR ITEM TOKO ───────────────────────────────────────────
    [System.Serializable]
    public class ItemTokoUI
    {
        public string namaPaket = "Paket Booster Kecil";
        public int jumlahBooster = 3;
        public int bonusKoin = 0;

        [Header("Harga & Pembelian")]
        public int hargaKoin = 300;
        public string hargaUangAsliTeks = "Rp 5.000 / $0.99";

        [Header("Tombol UI")]
        public Button tombolBeliDenganKoin;
        public Button tombolBeliDenganUangAsli;
    }

    [Header("Daftar Paket Item Toko")]
    [SerializeField] private ItemTokoUI[] daftarItemToko;

    [Header("Panel Pop-up Notifikasi Transaksi")]
    [SerializeField] private GameObject panelNotifikasi;
    [SerializeField] private TMP_Text teksNotifikasiPesan;
    [SerializeField] private Button tombolTutupNotifikasi;

    // ─── Lifecycle ───────────────────────────────────────────────────
    private void OnEnable()
    {
        ShopManager.OnTransaksiSelesai += HandlerTransaksiSelesai;
        RefreshSaldoUI();
    }

    private void OnDisable()
    {
        ShopManager.OnTransaksiSelesai -= HandlerTransaksiSelesai;
    }

    private void Start()
    {
        // Listener Tombol Tutup
        tombolTutupToko?.onClick.AddListener(TutupToko);
        tombolTutupNotifikasi?.onClick.AddListener(TutupNotifikasi);

        // Setup Listener Tombol Pembelian Tiap Item
        if (daftarItemToko != null)
        {
            foreach (var item in daftarItemToko)
            {
                ItemTokoUI targetItem = item; // Capture variable

                targetItem.tombolBeliDenganKoin?.onClick.AddListener(() =>
                {
                    ShopManager.Instance?.BeliDenganKoin(targetItem.hargaKoin, targetItem.jumlahBooster, targetItem.namaPaket);
                });

                targetItem.tombolBeliDenganUangAsli?.onClick.AddListener(() =>
                {
                    ShopManager.Instance?.BeliDenganUangAsli(targetItem.jumlahBooster, targetItem.bonusKoin, targetItem.namaPaket);
                });
            }
        }

        SembunyikanNotifikasi();
    }

    // ─── Public API ──────────────────────────────────────────────────

    /// <summary>Tampilkan Panel Toko.</summary>
    public void BukaToko()
    {
        if (panelToko != null) panelToko.SetActive(true);
        RefreshSaldoUI();
    }

    /// <summary>Sembunyikan Panel Toko.</summary>
    public void TutupToko()
    {
        if (panelToko != null) panelToko.SetActive(false);
    }

    /// <summary>Refresh saldo koin dan booster dari SaveManager.</summary>
    public void RefreshSaldoUI()
    {
        int koin = SaveManager.Instance?.MuatCoin() ?? 0;
        int booster = SaveManager.Instance?.MuatJumlahBooster() ?? 0;

        if (teksSaldoKoin != null) teksSaldoKoin.text = koin.ToString("N0");
        if (teksJumlahBooster != null) teksJumlahBooster.text = $"x{booster}";
    }

    // ─── Event Handler ───────────────────────────────────────────────
    private void HandlerTransaksiSelesai(string pesan, bool sukses)
    {
        RefreshSaldoUI();
        TampilkanNotifikasi(pesan);
    }

    private void TampilkanNotifikasi(string pesan)
    {
        if (panelNotifikasi != null) panelNotifikasi.SetActive(true);
        if (teksNotifikasiPesan != null) teksNotifikasiPesan.text = pesan;
    }

    private void TutupNotifikasi() => SembunyikanNotifikasi();

    private void SembunyikanNotifikasi()
    {
        if (panelNotifikasi != null) panelNotifikasi.SetActive(false);
    }
}
