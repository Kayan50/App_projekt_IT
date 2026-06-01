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
            return View(await _context.Doctors.ToListAsync());
        }

        // GET: DOCTORS/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // GET: DOCTORS/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DOCTORS/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,Title,ClinicId,Clinic,Services,AppointmentSlots")] Doctor doctor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(doctor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(doctor);
        }

        // GET: DOCTORS/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }
            return View(doctor);
        }

        // POST: DOCTORS/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,FirstName,LastName,Title,ClinicId,Clinic,Services,AppointmentSlots")] Doctor doctor)
        {
            if (id != doctor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(doctor.Id))
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
            return View(doctor);
        }

        // GET: DOCTORS/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(m => m.Id == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: DOCTORS/Delete/5
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

        // GET: Wyœwietla formularz generatora
        [HttpGet]
        public IActionResult GenerateSchedule()
        {
            
            ViewBag.Doctors = new SelectList(_context.Doctors, "Id", "LastName");
            ViewBag.Services = new SelectList(_context.Services, "Id", "Name");

            return View();
        }

        // POST: Przetwarza dane i generuje wizyty
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateSchedule(ScheduleGeneratorViewModel model)
        {
            if (ModelState.IsValid)
            {
                
                if (model.EndTime <= model.StartTime)
                {
                    ModelState.AddModelError("", "Godzina zakoñczenia musi byæ póŸniejsza ni¿ godzina rozpoczêcia.");
                    ViewBag.Doctors = new SelectList(_context.Doctors, "Id", "LastName");
                    ViewBag.Services = new SelectList(_context.Services, "Id", "Name");
                    return View(model);
                }

                
                DateTime currentSlotTime = model.Date.Date + model.StartTime;
                DateTime endSlotTime = model.Date.Date + model.EndTime;

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

            
            ViewBag.Doctors = new SelectList(_context.Doctors, "Id", "LastName");
            ViewBag.Services = new SelectList(_context.Services, "Id", "Name");
            return View(model);
        }
    }
}
