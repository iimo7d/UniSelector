using System.ComponentModel.DataAnnotations;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.AdminProgram
{
    public class ProgramCreateEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Arabic name is required")]
        [StringLength(200, ErrorMessage = "Arabic name cannot exceed 200 characters")]
        [Display(Name = "Program Name (Arabic)")]
        public string NameArabic { get; set; }

        [Required(ErrorMessage = "English name is required")]
        [StringLength(200, ErrorMessage = "English name cannot exceed 200 characters")]
        [Display(Name = "Program Name (English)")]
        public string NameEnglish { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Degree is required")]
        [Display(Name = "Degree Level")]
        public Degree Degree { get; set; }

        [Required(ErrorMessage = "Language is required")]
        [Display(Name = "Teaching Language")]
        public LanguageCode Language { get; set; }

        [StringLength(100, ErrorMessage = "Academic classification cannot exceed 100 characters")]
        [Display(Name = "Academic Classification")]
        public string? AcademicClassification { get; set; }

        [Required(ErrorMessage = "Total credit hours is required")]
        [Range(30, 300, ErrorMessage = "Total credit hours must be between 30 and 300")]
        [Display(Name = "Total Credit Hours")]
        public int TotalCreditHours { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
