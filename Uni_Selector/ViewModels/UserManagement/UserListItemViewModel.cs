namespace Uni_Selector.ViewModels.UserManagement
{
    public class UserListItemViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public string PrimaryRole => Roles.FirstOrDefault() ?? "No Role";

        public bool IsLockedOut => LockoutEnd.HasValue && LockoutEnd.Value > DateTimeOffset.UtcNow;
    }
}
