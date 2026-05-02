namespace Uni_Selector.ViewModels.UserManagement
{
    public class UserListViewModel
    {
        public List<UserListItemViewModel> Users { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalUsers { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalUsers / PageSize);

        // Filters
        public string? SearchTerm { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }

        // Summary
        public int TotalActiveUsers { get; set; }
        public int TotalInactiveUsers { get; set; }
        public Dictionary<string, int> RoleCounts { get; set; } = new();

        // Filter options
        public List<string> AllRoles { get; set; } = new();
    }
}
