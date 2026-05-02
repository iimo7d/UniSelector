using System.ComponentModel.DataAnnotations;
using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.ProgramManagement
{
    public class CreateProgramViewModel
    {
        [Required(ErrorMessage = "Please select a program")]
        [Display(Name = "Program")]
        public int ProgramId { get; set; }

        [Required(ErrorMessage = "Study system is required")]
        [Display(Name = "Study System")]
        public StudySystem StudySystem { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        [Range(1, 10, ErrorMessage = "Duration must be between 1 and 10 years")]
        [Display(Name = "Duration (Years)")]
        public int DurationInYears { get; set; }

        [Required(ErrorMessage = "Semester start date is required")]
        [Display(Name = "Semester Start Date")]
        public DateTime SemesterStartDate { get; set; }

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
        [Range(0, 10000, ErrorMessage = "Capacity must be between 0 and 10,000")]
        [Display(Name = "Student Capacity")]
        public int Capacity { get; set; }
    }
}
