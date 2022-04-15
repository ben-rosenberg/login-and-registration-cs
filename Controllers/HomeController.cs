using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using LoginAndRegistration.Models;

namespace LoginAndRegistration.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(ILogger<HomeController> logger, LoginAndRegistrationContext context)
        {
            _logger = logger;
            _db = context;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Dashboard");
            }

            return View("Index");
        }

        [HttpPost("register")]
        public IActionResult Register(User user)
        {
            if (!ModelState.IsValid)
            {
                return View("Index");
            }

            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            user.Password = passwordHasher.HashPassword(user, user.Password);

            _db.Add(user);
            _db.SaveChanges();

            HttpContext.Session.SetInt32("UserId", user.UserId);

            return RedirectToAction("Dashboard");
        }

        [HttpPost("login")]
        public IActionResult Login(LoginUser loginUser)
        {
            if (!ModelState.IsValid)
            {
                return View("Index");
            }

            User thisUser = _db.Users.FirstOrDefault(user => loginUser.LoginEmail == user.Email);

            if (thisUser == null)
            {
                ModelState.AddModelError("LoginEmail", "Incorrect email or password");
                return View("Index");
            }

            PasswordHasher<LoginUser> passwordHasher = new PasswordHasher<LoginUser>();
            PasswordVerificationResult isPasswordMatch = passwordHasher.VerifyHashedPassword(
                loginUser, thisUser.Password, loginUser.LoginPassword);

            if (isPasswordMatch == 0)
            {
                ModelState.AddModelError("LoginPassword", "Incorrect email or password");
                return View("Index");
            }

            HttpContext.Session.SetInt32("UserId", thisUser.UserId);

            return RedirectToAction("Dashboard");
        }

        // May need to check if thisUser was successfully constructed. If it
        // passes the first check, there shouldn't ever be a problem, but
        // probably not a bad idea still.
        [HttpGet("/dashboard")]
        public IActionResult Dashboard()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                Console.WriteLine("uid not in session");
                return RedirectToAction("Index");
            }

            User thisUser = _db.Users.FirstOrDefault(user => user.UserId == (int)userId);

            if (thisUser == null)
            {
                Console.WriteLine("user not found");
                return RedirectToAction("Index");
            }

            return View("Dashboard", thisUser);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UserId");
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private readonly ILogger<HomeController> _logger;
        private LoginAndRegistrationContext _db;
    }
}
