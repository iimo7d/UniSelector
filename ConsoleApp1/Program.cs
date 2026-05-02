using Microsoft.AspNetCore.Identity;
using Uni_Selector.Models;


    var hasher = new PasswordHasher<ApplicationUser>();
    var user = new ApplicationUser(); // empty user object

    string hash = hasher.HashPassword(user, "Admin@123");
    Console.WriteLine(hash);
