using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogiTech.Models
{
    public class DeliveryStop
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = "";

        // Filled automatically by geocoding API
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        public string Priority { get; set; } = "MEDIUM"; // HIGH, MEDIUM, LOW

        public string CustomerName { get; set; } = "";
        public string TrackingNumber { get; set; } = "";
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Filled after optimisation runs
        public int OptimalOrder { get; set; }
        public string? ETA { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DistanceFromPrevKm { get; set; }

        // Optional foreign key to a Route
        public int? RouteId { get; set; }

        [ForeignKey("RouteId")]
        public Route? Route { get; set; }
    }
}