namespace Uni_Selector.ViewModels.BTECAuthority
{
    public class BTECStudentStatisticsViewModel
    {
        // Summary
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public int StudentsWithLevel2 { get; set; }
        public int StudentsWithLevel3 { get; set; }
        public int EnrolledStudents { get; set; }

        // Demographics
        public List<GenderDistributionDto> GenderDistribution { get; set; } = new();
        public List<ProvinceDistributionDto> ProvinceDistribution { get; set; } = new();
        public List<LevelEnrollmentDto> EnrollmentByLevel { get; set; } = new();

        // Performance
        public double AverageGPA { get; set; }
        public List<GPARangeDto> GPADistribution { get; set; } = new();
    }
}
