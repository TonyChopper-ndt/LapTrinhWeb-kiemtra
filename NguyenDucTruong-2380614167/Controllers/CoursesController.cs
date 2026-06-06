using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenDucTruong_2380614167.Data;

namespace NguyenDucTruong_2380614167.Controllers
{
    [Route("courses")]
    public class CoursesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CoursesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("")]
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

            ViewBag.Keyword = keyword;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.EnrolledCourseIds = await GetEnrolledCourseIdsAsync();

            var data = await courses
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

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
    }
}
