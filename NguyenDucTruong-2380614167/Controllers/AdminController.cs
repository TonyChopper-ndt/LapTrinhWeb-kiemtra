using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NguyenDucTruong_2380614167.Data;
using NguyenDucTruong_2380614167.Models;

namespace NguyenDucTruong_2380614167.Controllers
{
    [Authorize(Roles = "ADMIN")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public AdminController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        [HttpGet("")]
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var students = await _userManager.GetUsersInRoleAsync("STUDENT");
            ViewBag.TotalCourses = await _context.Courses.CountAsync();
            ViewBag.TotalStudents = students.Count;
            ViewBag.TotalEnrollments = await _context.Enrollments.CountAsync();
            return View();
        }

        [HttpGet("courses")]
        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Courses
                .Include(c => c.Category)
                .OrderBy(c => c.Id)
                .ToListAsync();
            return View(courses);
        }

        [HttpGet("courses/create")]
        public IActionResult Create()
        {
            LoadCategories();
            return View();
        }

        [HttpPost("courses/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                course.Image = imageFile != null
                    ? await SaveImageAsync(imageFile)
                    : "/images/web.svg";

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Courses));
            }

            LoadCategories(course.CategoryId);
            return View(course);
        }

        [HttpGet("courses/edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            LoadCategories(course.CategoryId);
            return View(course);
        }

        [HttpPost("courses/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course, IFormFile? imageFile)
        {
            if (id != course.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    course.Image = await SaveImageAsync(imageFile);
                }

                _context.Courses.Update(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Courses));
            }

            LoadCategories(course.CategoryId);
            return View(course);
        }

        [HttpGet("courses/delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [HttpPost("courses/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Courses));
        }

        private void LoadCategories(int? selectedId = null)
        {
            ViewBag.CategoryId = new SelectList(_context.Categories.ToList(), "Id", "Name", selectedId);
        }

        private async Task<string> SaveImageAsync(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(imageFile.FileName);
            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return "/images/uploads/" + fileName;
        }
    }
}
