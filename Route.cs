using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogiTech.Models
{
    public class Route
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string VehicleId { get; set; } = "";
        public string VehicleStatus { get; set; } = "Standby";

        [Column(TypeName = "decimal(5,2)")]
        public decimal CapacityPercent { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal FuelSaved { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DistanceSaved { get; set; }

        public int ImprovementPercent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Navigation property — one route has many stops
        public List<DeliveryStop> Stops { get; set; } = new();
    }
}