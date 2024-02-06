using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Suket.Data;
using Suket.Models;

namespace Suket.Controllers
{
    public class UserBalancesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserBalancesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserBalances
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.UserBalance.Include(u => u.UserAccount);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: UserBalances/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.UserBalance == null)
            {
                return NotFound();
            }

            var userBalance = await _context.UserBalance
                .Include(u => u.UserAccount)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userBalance == null)
            {
                return NotFound();
            }

            return View(userBalance);
        }

        // GET: UserBalances/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: UserBalances/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Balance,LastUpdated")] UserBalance userBalance)
        {
            if (ModelState.IsValid)
            {
                _context.Add(userBalance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Id", userBalance.Id);
            return View(userBalance);
        }

        // GET: UserBalances/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.UserBalance == null)
            {
                return NotFound();
            }

            var userBalance = await _context.UserBalance.FindAsync(id);
            if (userBalance == null)
            {
                return NotFound();
            }
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Id", userBalance.Id);
            return View(userBalance);
        }

        // POST: UserBalances/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Balance,LastUpdated")] UserBalance userBalance)
        {
            if (id != userBalance.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userBalance);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserBalanceExists(userBalance.Id))
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
            ViewData["Id"] = new SelectList(_context.Users, "Id", "Id", userBalance.Id);
            return View(userBalance);
        }

        // GET: UserBalances/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.UserBalance == null)
            {
                return NotFound();
            }

            var userBalance = await _context.UserBalance
                .Include(u => u.UserAccount)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userBalance == null)
            {
                return NotFound();
            }

            return View(userBalance);
        }

        // POST: UserBalances/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.UserBalance == null)
            {
                return Problem("Entity set 'ApplicationDbContext.UserBalance'  is null.");
            }
            var userBalance = await _context.UserBalance.FindAsync(id);
            if (userBalance != null)
            {
                _context.UserBalance.Remove(userBalance);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserBalanceExists(string id)
        {
          return (_context.UserBalance?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
