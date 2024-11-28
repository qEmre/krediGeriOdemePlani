using Microsoft.AspNetCore.Mvc;
using projectOne.Models;

namespace projetOne.Controllers
{
    public class HesaplamaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult hesaplama(KrediBilgileri k)
        {
            // kullanıcıdan gelen veriler
            DateTime krediTarihi = k.krediTarihi;
            double krediTutari = k.krediTutari;
            int taksitSayisi = k.taksitSayisi;

            // sabit veriler
            double yillikFaizOrani = 0.54;
            double aylikFaiz = yillikFaizOrani / 12;
            double vergiOrani = 0.05;

            // taksit tutarının bulunması için maximum ve minimum değerler
            double maxT = 999999999999;
            double minT = 0;
            double tolerans = 0.01;

            double KA;
            List<KrediBilgileri> hesaplananTaksitler = new List<KrediBilgileri>();
            int taksitGunu = krediTarihi.Day; // kullanıcının krediyi çektiği gün

            bool istenilenDeger = true;
            double T = 0; // İlk olarak T'nin başlangıç değerini belirliyoruz
            while (istenilenDeger)
            {
                hesaplananTaksitler.Clear(); // hesaplama başarısız ise listeyi temizle
                KA = krediTutari;
                T = (maxT + minT) / 2; // maximum ve minimum tutarı 2'ye böl hesaplama yap
                DateTime simdikiTarih = krediTarihi;

                for (int i = 1; i <= taksitSayisi; i++)
                {
                    DateTime taksitTarihi = simdikiTarih;
                    taksitTarihi = new DateTime(taksitTarihi.Year, taksitTarihi.Month, taksitGunu).AddMonths(1); // taksit tarihinden 1 ay sonrasını al

                    // hafta sonuna denk gelirse pazartesiye yuvarla
                    if (taksitTarihi.DayOfWeek == DayOfWeek.Saturday)
                    {
                        taksitTarihi = taksitTarihi.AddDays(2);
                    }
                    else if (taksitTarihi.DayOfWeek == DayOfWeek.Sunday)
                    {
                        taksitTarihi = taksitTarihi.AddDays(1);
                    }
                    TimeSpan gunFarki = taksitTarihi - simdikiTarih; // vadeler arası gün farkını hesaplıyoruz

                    double F = KA * yillikFaizOrani * gunFarki.Days / 360;
                    double BSMV = F * vergiOrani;
                    double A = T - (F + BSMV);

                    if (i == taksitSayisi && KA < T) // son takside geldiğimizde ya da kalan anapara, taksit tutarından küçük ise
                    {
                        A = KA;
                        T = A + F + BSMV;
                        KA = 0;
                    }
                    else
                    {
                        KA -= A;
                    }
                    simdikiTarih = taksitTarihi; // şimdiki taksit tarihini hesaplanan tarih yap yukarıda tekrar ileri alacağız

                    hesaplananTaksitler.Add(new KrediBilgileri
                    {
                        taksitTarihi = taksitTarihi,
                        gunAraligi = gunFarki.Days,
                        taksitTutari = T,
                        anaparaTutari = A,
                        faizTutari = F,
                        vergiTutari = BSMV,
                        kalanAnapara = KA
                    });

                    if (KA <= 0) break; // kalan anapara sıfırlandıysa döngüyü bitir
                }
                if (!istenilenDeger || KA > tolerans) // koşul sağlanmamışsa veya kalan anapara, tolerans değerimden büyük ise 
                {
                    minT = T; // T'yi minimum taksit tutarına ata
                }
                else if (KA < -tolerans)
                {
                    maxT = T; // T'yi maximum taksit tutarına ata
                }
                else
                {
                    break; // aksi durumda döngüyü bitir
                }
            }

            // son taksit tutarı ile birlikte normal taksit tutarını al
            double TT = hesaplananTaksitler.OrderBy(t => t.taksitTarihi).FirstOrDefault().taksitTutari;
            double sonT = hesaplananTaksitler.OrderByDescending(t => t.taksitTarihi).FirstOrDefault().taksitTutari;

            bool KAzero = true;
            while (KAzero)
            {
                KA = krediTutari;
                /*TT += sonT * 0.004451;*/ // doğru taksit tutarı yedirmesi
                TT += sonT * 0.005;  // dinamik kodum

                for (int i = 0; i < taksitSayisi; i++)
                {
                    double Faiz = KA * yillikFaizOrani * hesaplananTaksitler[i].gunAraligi / 360;
                    double BSMV1 = Faiz * vergiOrani;
                    double Anapara = TT - (Faiz + BSMV1);

                    // son taksit veya kalan anapara durumu
                    if (i == taksitSayisi - 1 || KA < TT)
                    {
                        Anapara = KA;
                        TT = Anapara + Faiz + BSMV1;
                        KA = 0;
                    }
                    else
                    {
                        KA -= Anapara;
                    }

                    hesaplananTaksitler[i].taksitTutari = TT;
                    hesaplananTaksitler[i].anaparaTutari = Anapara;
                    hesaplananTaksitler[i].faizTutari = Faiz;
                    hesaplananTaksitler[i].vergiTutari = BSMV1;
                    hesaplananTaksitler[i].kalanAnapara = KA;

                    if (KA <= tolerans) // kalan anapara tolerans değerine yakınsa döngüyü bitir
                    {
                        KAzero = false;
                        break;
                    }
                }
            }
            return View(hesaplananTaksitler);
        }
    }
}