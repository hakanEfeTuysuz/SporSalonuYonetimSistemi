using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SporSalonuYonetimSistemi.Models
{
    public class AntrenorHizmet
    {
        // Hangi Antrenör?
        public int AntrenorId { get; set; }
        public Antrenor Antrenor { get; set; }

        // Hangi Hizmeti Verebiliyor?
        public int HizmetId { get; set; }
        public Hizmet Hizmet { get; set; }
    }
}