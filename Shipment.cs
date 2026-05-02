
using System.ComponentModel.DataAnnotations;

namespace LogiTech.Models
{
    public class Shipment
    {
        [Key]
        public int Id { get; set; }

        public string? TrackingNumber { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        public string CustomerName { get; set; } = "";

        [Required(ErrorMessage = "Delivery address is required")]
        public string DeliveryAddress { get; set; } = "";

        [Range(0.1, 9999, ErrorMessage = "Weight must be greater than 0")]
        public double WeightKg { get; set; }

        public string PriorityLevel { get; set; } = "Standard (3-5 days)";
        public string ShippingMethod { get; set; } = "Standard Delivery";
        public string Currency { get; set; } = "ZAR";

        [Range(0, 999999)]
        public decimal EstimatedValue { get; set; }

        public string Status { get; set; } = "Pending Dispatch";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}