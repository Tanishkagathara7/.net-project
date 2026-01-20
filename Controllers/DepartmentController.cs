using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MOM.Data;
using MOM.Models;

namespace MOM.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var departments = await _context.Departments.ToListAsync();
            return View(departments);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(DepartmentModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.Created = DateTime.Now;
                    model.Modified = DateTime.Now;
                    
                    _context.Departments.Add(model);
                    await _context.SaveChangesAsync();
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Database error: {ex.Message}");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
            {
                return NotFound();
            }
            return View(department);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DepartmentModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.Modified = DateTime.Now;
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Database error: {ex.Message}");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var department = await _context.Departments.FindAsync(id);
                if (department != null)
                {
                    _context.Departments.Remove(department);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Handle error - could add TempData message here
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
