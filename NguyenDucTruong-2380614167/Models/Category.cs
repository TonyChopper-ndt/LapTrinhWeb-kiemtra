using System.ComponentModel.DataAnnotations;

namespace NguyenDucTruong_2380614167.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; } = string.Empty;

        public List<Course> Courses { get; set; } = new List<Course>();
    }
}
