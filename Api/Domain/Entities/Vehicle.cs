using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace minimal_api.Domain.Entities
{
    public class Vehicle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } = default!;
        [Required]
        [StringLength(255)]
        public string Model { get; set; } = default!;
        [Required]
        [StringLength(50)]
        public string Brand { get; set; } = default!;
        [Required]
        public int Year { get; set; } = default!;
    }
}
