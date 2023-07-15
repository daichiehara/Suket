using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Suket.Data;
using Suket.Models;

namespace Suket.Controllers
{
    public class ConfirmsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ConfirmsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Confirms
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Confirm.Include(c => c.Post).Include(c => c.UserAccount);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Confirms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Confirm == null)
            {
                return NotFound();
            }

            var confirm = await _context.Confirm
                .Include(c => c.Post)
                .Include(c => c.UserAccount)
                .FirstOrDefaultAsync(m => m.ConfirmId == id);
            if (confirm == null)
            {
                return NotFound();
            }

            return View(confirm);
        }

        // GET: Confirms/Create
        public IActionResult Create()
        {
            ViewData["PostId"] = new SelectList(_context.Post, "PostId", "Item");
            ViewData["UserAccountId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Confirms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ConfirmId,UserAccountId,PostId")] Confirm confirm)
        {
            if (ModelState.IsValid)
            {
                _context.Add(confirm);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PostId"] = new SelectList(_context.Post, "PostId", "Item", confirm.PostId);
            ViewData["UserAccountId"] = new SelectList(_context.Users, "Id", "Id", confirm.UserAccountId);
            return View(confirm);
        }

        // GET: Confirms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Confirm == null)
            {
                return NotFound();
            }

            var confirm = await _context.Confirm.FindAsync(id);
            if (confirm == null)
            {
                return NotFound();
            }
            ViewData["PostId"] = new SelectList(_context.Post, "PostId", "Item", confirm.PostId);
            ViewData["UserAccountId"] = new SelectList(_context.Users, "Id", "Id", confirm.UserAccountId);
            return View(confirm);
        }

        // POST: Confirms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ConfirmId,UserAccountId,PostId")] Confirm confirm)
        {
            if (id != confirm.ConfirmId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(confirm);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConfirmExists(confirm.ConfirmId))
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
            ViewData["PostId"] = new SelectList(_context.Post, "PostId", "Item", confirm.PostId);
            ViewData["UserAccountId"] = new SelectList(_context.Users, "Id", "Id", confirm.UserAccountId);
            return View(confirm);
        }

        // GET: Confirms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Confirm == null)
            {
                return NotFound();
            }

            var confirm = await _context.Confirm
                .Include(c => c.Post)
                .Include(c => c.UserAccount)
                .FirstOrDefaultAsync(m => m.ConfirmId == id);
            if (confirm == null)
            {
                return NotFound();
            }

            return View(confirm);
        }

        // POST: Confirms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Confirm == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Confirm'  is null.");
            }
            var confirm = await _context.Confirm.FindAsync(id);
            if (confirm != null)
            {
                _context.Confirm.Remove(confirm);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConfirmExists(int id)
        {
          return (_context.Confirm?.Any(e => e.ConfirmId == id)).GetValueOrDefault();
        }
    }
}
