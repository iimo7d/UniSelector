using System.ComponentModel.DataAnnotations;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.ProgramManagement
{
    public class CreateBtecProgramViewModel
    {
        [Required(ErrorMessage = "Arabic name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        [Display(Name = "Program Name (Arabic)")]
        public string NameArabic { get; set; }

        [Required(ErrorMessage = "English name is required")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        [Display(Name = "Program Name (English)")]
        public string NameEnglish { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "BTEC level is required")]
        [Display(Name = "BTEC Level")]
        public BtecLevel Level { get; set; }

        [Required(ErrorMessage = "Technical field is required")]
        [StringLength(100, ErrorMessage = "Field name cannot exceed 100 characters")]
        [Display(Name = "Technical Field")]
        public string TechnicalField { get; set; }

        [Required(ErrorMessage = "Language is required")]
        [Display(Name = "Language of Instruction")]
        public LanguageCode Language { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 10, ErrorMessage = "Duration must be between 1 and 10 years")]
        [Display(Name = "Duration (Years)")]
        public int DurationInYears { get; set; }

        [Required(ErrorMessage = "Semester start date is required")]
        [Display(Name = "Semester Start Date")]
        public DateTime SemesterStartDate { get; set; }

        [Required(ErrorMessage = "Total credit hours is required")]
        [Range(30, 300, ErrorMessage = "Credit hours must be between 30 and 300")]
        [Display(Name = "Total Credit Hours")]
        public int TotalCreditHours { get; set; }

        [Required(ErrorMessage = "Hour price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Hour price must be greater than 0")]
        [Display(Name = "Hour Price (JOD)")]
        public decimal HourPriceBase { get; set; }

        [Required(ErrorMessage = "First semester registration fee is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fee must be greater than 0")]
        [Display(Name = "First Semester Registration Fee (JOD)")]
        public decimal RegistrationFeeFirstSemester { get; set; }

        [Required(ErrorMessage = "Regular semester registration fee is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fee must be greater than 0")]
        [Display(Name = "Regular Semester Registration Fee (JOD)")]
        public decimal RegistrationFeeRegularSemester { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(0, 1000, ErrorMessage = "Capacity must be between 0 and 1,000")]
        [Display(Name = "Student Capacity")]
        public int Capacity { get; set; }
    }
}
