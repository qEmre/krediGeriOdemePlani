namespace projectOne.Models
{
    public class KrediBilgileri
    {
        public double krediTutari { get; set; }
        public int taksitSayisi { get; set; }
        public DateTime krediTarihi { get; set; }
        public DateTime taksitTarihi { get; set; }
        public int gunAraligi { get; set; }
        public double taksitTutari { get; set; }
        public double anaparaTutari { get; set; }
        public double faizTutari { get; set; }
        public double vergiTutari { get; set; }
        public double kalanAnapara { get; set; }
    }
}