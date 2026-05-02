using Microsoft.AspNetCore.Identity;
using Uni_Selector.Models; // عدّل namespace حسب مشروعك

var hasher = new PasswordHasher<ApplicationUser>();

var users = new[]
{
    new ApplicationUser { Id = "std-001", UserName = "admin@uni-selector.jo" },
};

foreach (var u in users)
{
    var hash = hasher.HashPassword(u, "Student@123"); // عدّل الباسورد حسب المستخدم
    Console.WriteLine($"{u.Id} => {hash}");
} 