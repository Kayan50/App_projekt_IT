using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; 
using Microsoft.EntityFrameworkCore;
using App_projekt_IT.Models;
using App_projekt_IT.Data;

namespace App_projekt_IT.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClinicsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClinicsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CLINICS
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Clinics.Include(c => c.City);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: CLINICS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clinic = await _context.Clinics
                .Include(c => c.City) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (clinic == null)
            {
                return NotFound();
            }

            return View(clinic);
        }

        // GET: CLINICS/Create
        public IActionResult Create()
        {
            
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name");
            return View();
        }

        // POST: CLINICS/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Create([Bind("Id,Name,Phone,Email,Address,PostalCode,CityId")] Clinic clinic)
        {
            if (ModelState.IsValid)
            {
                _context.Add(clinic);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", clinic.CityId);
            return View(clinic);
        }

        // GET: CLINICS/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null)
            {
                return NotFound();
            }

            
            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", clinic.CityId);
            return View(clinic);
        }

        // POST: CLINICS/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
     
        public async Task<IActionResult> Edit(int? id, [Bind("Id,Name,Phone,Email,Address,PostalCode,CityId")] Clinic clinic)
        {
            if (id != clinic.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(clinic);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClinicExists(clinic.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CityId"] = new SelectList(_context.Cities, "Id", "Name", clinic.CityId);
            return View(clinic);
        }

        // GET: CLINICS/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clinic = await _context.Clinics
                .Include(c => c.City) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (clinic == null)
            {
                return NotFound();
            }

            return View(clinic);
        }

        // POST: CLINICS/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic != null)
            {
                _context.Clinics.Remove(clinic);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClinicExists(int? id)
        {
            return _context.Clinics.Any(e => e.Id == id);
        }
    }
}