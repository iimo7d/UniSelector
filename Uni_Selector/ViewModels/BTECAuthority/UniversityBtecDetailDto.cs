using Uni_Selector.Models.Enums;

namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class UniversityBtecDetailDto
    {
        public int UniversityId { get; set; }
        public string NameEnglish { get; set; }
        public string NameArabic { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int TotalBtecPrograms { get; set; }
        public int ApprovedPrograms { get; set; }
        public int PendingPrograms { get; set; }
        public int TotalStudents { get; set; }
        public DateTime? FirstProgramDate { get; set; }
        public List<BtecLevel> LevelsOffered { get; set; } = new();
    }
}
