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

        public string IslemAciklamasi
        {
            get
            {
                string metin = !string.IsNullOrEmpty(IslemAdi) ? IslemAdi : "İşlem Bilinmiyor";

                if (!string.IsNullOrEmpty(HedefSeriNo))
                {
                    metin += $" (Seri No: {HedefSeriNo})";
                }
                else if (HedefDeviceId.HasValue && HedefDeviceId.Value > 0)
                {
                    metin += $" (Cihaz ID: {HedefDeviceId.Value})";
                }
                else if (HedefUserId.HasValue && HedefUserId.Value > 0)
                {
                    metin += $" (Kullanıcı ID: {HedefUserId.Value})";
                }
                return metin;
            }
        }
    }
}