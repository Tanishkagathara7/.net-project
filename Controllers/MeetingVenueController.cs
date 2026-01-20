using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MOM.Data;
using MOM.Models;

namespace MOM.Controllers
{
    public class MeetingVenueController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MeetingVenueController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var venues = await _context.MeetingVenues.ToListAsync();
            return View(venues);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(MeetingVenueModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    model.Created = DateTime.Now;
                    model.Modified = DateTime.Now;
                    
                    _context.MeetingVenues.Add(model);
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
            var venue = await _context.MeetingVenues.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(MeetingVenueModel model)
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
                var venue = await _context.MeetingVenues.FindAsync(id);
                if (venue != null)
                {
                    _context.MeetingVenues.Remove(venue);
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
