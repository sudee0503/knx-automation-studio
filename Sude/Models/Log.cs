using System;

namespace Sude.Models
{
    public class Log
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? TTypeId { get; set; }
        public DateTime Tarihi { get; set; }
        public string KullaniciAdi { get; set; }
        public string IslemAdi { get; set; }
        public string SecilenKullanici { get; set; }
        public DateTime? FiltreBaslangic { get; set; }
        public DateTime? FiltreBitis { get; set; }
        public int? SecilenIslemId { get; set; }

        public int? HedefUserId { get; set; }
        public int? HedefDeviceId { get; set; }
        public string HedefSeriNo { get; set; }
        public string HedefKullaniciAdi { get; set; }

        public string IslemAciklamasi
        {
            get
            {
                string islem = !string.IsNullOrEmpty(IslemAdi) ? IslemAdi : "İşlem Bilinmiyor";

                if (islem == "Sisteme Giriş" || islem == "Sistemden Çıkış")
                {
                    return $"{KullaniciAdi} {islem} Yaptı";
                }

                string metin = "";

                if (!string.IsNullOrEmpty(HedefSeriNo))
                {
                    metin = $" {HedefSeriNo} Numaralı ";
                }
                else if (HedefDeviceId.HasValue && HedefDeviceId.Value > 0)
                {
                    metin = $" {HedefDeviceId.Value} Model Kodlu ";
                }
                else if (!string.IsNullOrEmpty(HedefKullaniciAdi))
                {
                    metin = $" {HedefKullaniciAdi} İsimli ";
                }
                else if (HedefUserId.HasValue && HedefUserId.Value > 0)
                {
                    metin = $" {HedefUserId.Value} Numaralı'li ";
                }
                return metin + islem;
            }
        }
    }
}