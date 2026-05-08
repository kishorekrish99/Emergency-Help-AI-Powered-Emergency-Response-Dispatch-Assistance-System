using System.ComponentModel.DataAnnotations;

namespace EmergencyHelp.Models.Entities
{
    public class Emergency
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [Phone]
        public string Phone { get; set; }

        [MaxLength(200)]
        public string Location { get; set; }

    }
}
