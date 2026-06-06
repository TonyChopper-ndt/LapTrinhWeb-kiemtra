using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using NguyenDucTruong_2380614167.Data;
using NguyenDucTruong_2380614167.Models;

namespace NguyenDucTruong_2380614167.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string keyword, int page = 1)
        {
            int pageSize = 5;
            var courses = _context.Courses
                .Include(c => c.Category)
                .OrderBy(c => c.Id)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                courses = courses.Where(c => c.Name.Contains(keyword));
            }

            int totalCourses = await courses.CountAsync();
            int totalPages = (int)Math.Ceiling(totalCourses / (double)pageSize);
            page = page < 1 ? 1 : page;

            var data = await courses
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Keyword = keyword;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.EnrolledCourseIds = await GetEnrolledCourseIdsAsync();

            return View(data);
        }

        private async Task<List<int>> GetEnrolledCourseIdsAsync()
        {
            if (!(User.Identity?.IsAuthenticated ?? false))
            {
                return new List<int>();
            }

            var userId = _userManager.GetUserId(User);
            return await _context.Enrollments
                .Where(e => e.UserId == userId)
                .Select(e => e.CourseId)
                .ToListAsync();
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
    }
}
