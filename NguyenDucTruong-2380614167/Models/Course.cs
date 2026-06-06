using System.ComponentModel.DataAnnotations;

namespace NguyenDucTruong_2380614167.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên học phần")]
        [Display(Name = "Tên học phần")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Hình ảnh")]
        public string Image { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số tín chỉ")]
        [Range(1, 10, ErrorMessage = "Số tín chỉ từ 1 đến 10")]
        [Display(Name = "Số tín chỉ")]
        public int Credits { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giảng viên")]
        [Display(Name = "Giảng viên")]
        public string Lecturer { get; set; } = string.Empty;

        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public List<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
