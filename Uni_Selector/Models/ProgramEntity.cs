using System.ComponentModel.DataAnnotations;
using Uni_Selector.Models.Enums;
using Degree = Uni_Selector.Models.Enums.Degree;

namespace Uni_Selector.Models
{
    public class ProgramEntity
    {
        public int Id { get; set; }
        [Required, StringLength(200)]
        public string NameArabic { get; set; }
        [Required, StringLength(200)]
        public string NameEnglish { get; set; }
        [StringLength(1000)]
        public string? Description { get; set; }
        [Required] 
        public Degree Degree { get; set; }
        [Required] 
        public LanguageCode Language { get; set; }
        [StringLength(100)]
        public string? AcademicClassification { get; set; }
        [Required, Range(30, 300)]
        public int TotalCreditHours { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<UniversityProgram> UniversityPrograms { get; set; }
    }

}
