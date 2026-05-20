using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sude.Models
{
    public enum IslemTuru
    {
        SistemeGiris = 1,
        SistemdenCikis = 2,
        CihazKontrolBasarili = 3,
        CihazKontrolHatali = 4,
        CihazIdAtandi = 5,
        CihazIdAtamaHatasi = 6,
        YeniKullaniciEklendi = 7,
        KullaniciGuncellendi = 8,
        KullaniciSilindi = 9,
        YeniCihazEklendi = 10,
        CihazGuncellendi = 11
    }
}
