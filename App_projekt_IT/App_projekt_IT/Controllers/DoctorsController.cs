using App_projekt_IT.Data;
using App_projekt_IT.Models;
using App_projekt_IT.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace App_projekt_IT.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DoctorsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DOCTORS
        public async Task<IActionResult> Index()
        {
            
            var doctors = _context.Doctors.Include(d => d.Clinic);
            return View(await doctors.ToListAsync());
        }

        // GET: DOCTORS/Details/
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var doctor = await _context.Doctors
                .Include(d => d.Clinic) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (doctor == null) return NotFound();

            return View(doctor);
        }

        // GET: DOCTORS/Create
        public IActionResult Create()
        {
            
            ViewData["ClinicId"] = new SelectList(_context.Clinics, "Id", "Name");
            return View();
        }

        // POST: DOCTORS/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Title,ClinicId")] Doctor doctor, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                
                if (imageFile != null && imageFile.Length > 0)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await imageFile.CopyToAsync(memoryStream);
                        doctor.ImageData = memoryStream.ToArray();
                        doctor.ImageContentType = imageFile.ContentType;
                    }
                }

                _context.Add(doctor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClinicId"] = new SelectList(_context.Clinics, "Id", "Name", doctor.ClinicId);
            return View(doctor);
        }

        // GET: DOCTORS/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            
            ViewData["ClinicId"] = new SelectList(_context.Clinics, "Id", "Name", doctor.ClinicId);
            return View(doctor);
        }
        // POST: DOCTORS/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,FirstName,LastName,Title,ClinicId,ImageData,ImageContentType")] Doctor doctor, IFormFile? imageFile)
        {
            if (id != doctor.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await imageFile.CopyToAsync(memoryStream);
                            doctor.ImageData = memoryStream.ToArray();
                            doctor.ImageContentType = imageFile.ContentType;
                        }
                    }

                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ClinicId"] = new SelectList(_context.Clinics, "Id", "Name", doctor.ClinicId);
            return View(doctor);
        }

        // GET: DOCTORS/Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var doctor = await _context.Doctors
                .Include(d => d.Clinic) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (doctor == null) return NotFound();

            return View(doctor);
        }

        // POST: DOCTORS/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DoctorExists(int? id)
        {
            return _context.Doctors.Any(e => e.Id == id);
        }

        // GET
        [HttpGet]
        public IActionResult GenerateSchedule()
        {
            
            var doctorsList = _context.Doctors.Select(d => new {
                Id = d.Id,
                FullName = d.FirstName + " " + d.LastName
            }).ToList();

            ViewBag.Doctors = new SelectList(doctorsList, "Id", "FullName");
            ViewBag.Services = new SelectList(_context.Services, "Id", "Name");

            return View();
        }

        // POST: 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateSchedule(ScheduleGeneratorViewModel model)
        {
            if (ModelState.IsValid)
            {
                
                if (model.EndTime <= model.StartTime)
                {
                    ModelState.AddModelError("", "Godzina zakoñczenia musi byæ póŸniejsza ni¿ godzina rozpoczêcia.");
                    RepopulateGeneratorViewBags();
                    return View(model);
                }

                DateTime currentSlotTime = model.Date.Date + model.StartTime;
                DateTime endSlotTime = model.Date.Date + model.EndTime;

                
                bool slotsExist = await _context.AppointmentSlots
                    .AnyAsync(a => a.DoctorId == model.DoctorId &&
                                   a.StartTime >= currentSlotTime &&
                                   a.StartTime <= endSlotTime);

                if (slotsExist)
                {
                    ModelState.AddModelError("", "W wybranym przedziale czasowym istniej¹ ju¿ wygenerowane terminy dla tego lekarza. Zmieñ datê lub godziny.");
                    RepopulateGeneratorViewBags();
                    return View(model);
                }

                var slotsToAdd = new List<AppointmentSlot>();

                while (currentSlotTime.AddMinutes(model.IntervalMinutes) <= endSlotTime)
                {
                    var slot = new AppointmentSlot
                    {
                        DoctorId = model.DoctorId,
                        ServiceId = model.ServiceId,
                        StartTime = currentSlotTime,
                        IsBooked = false
                    };

                    slotsToAdd.Add(slot);
                    currentSlotTime = currentSlotTime.AddMinutes(model.IntervalMinutes);
                }

                _context.AppointmentSlots.AddRange(slotsToAdd);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            RepopulateGeneratorViewBags();
            return View(model);
        }

        
        private void RepopulateGeneratorViewBags()
        {
            var doctorsList = _context.Doctors.Select(d => new {
                Id = d.Id,
                FullName = d.FirstName + " " + d.LastName
            }).ToList();
            ViewBag.Doctors = new SelectList(doctorsList, "Id", "FullName");
            ViewBag.Services = new SelectList(_context.Services, "Id", "Name");
        }
    }
}