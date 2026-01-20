using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace MOM.Models
{
    public class StaffListVM
    {
        public List<StaffDto> StaffList { get; set; } = new List<StaffDto>();
        
        // Filters
        public int? DepartmentID { get; set; }
        public string? SearchTerm { get; set; }
        
        // Dropdown Data
        public SelectList Departments { get; set; }
    }
}
