namespace Uni_Selector.ViewModels.Admin
{
    public class UserSummaryDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public string StatusBadgeClass => IsActive
            ? "bg-success-focus text-success-main"
            : "bg-danger-focus text-danger-main";

        public string StatusText => IsActive ? "Active" : "Inactive";

        public string LastLoginText => LastLoginAt.HasValue
            ? LastLoginAt.Value.ToString("MMM dd, yyyy")
            : "Never";
    }
}
