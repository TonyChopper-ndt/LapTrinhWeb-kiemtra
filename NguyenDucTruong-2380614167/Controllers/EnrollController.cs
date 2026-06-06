using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NguyenDucTruong_2380614167.Data;
using NguyenDucTruong_2380614167.Models;

namespace NguyenDucTruong_2380614167.Controllers
{
    [Authorize(Roles = "STUDENT")]
    [Route("enroll")]
    public class EnrollController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EnrollController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("add/{courseId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            var exists = await _context.Enrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (!exists)
            {
                _context.Enrollments.Add(new Enrollment
                {
                    UserId = userId!,
                    CourseId = courseId,
                    EnrollDate = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }

            return RedirectBack();
        }

        [HttpPost("cancel/{courseId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int courseId)
        {
            var userId = _userManager.GetUserId(User);
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.UserId == userId && e.CourseId == courseId);

            if (enrollment != null)
            {
                _context.Enrollments.Remove(enrollment);
                await _context.SaveChangesAsync();
            }

            return RedirectBack();
        }

        [HttpGet("my-courses")]
        public async Task<IActionResult> MyCourses()
        {
            var userId = _userManager.GetUserId(User);
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c!.Category)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.EnrollDate)
                .ToListAsync();

            return View(enrollments);
        }

        private IActionResult RedirectBack()
        {
            var referer = Request.Headers.Referer.ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
