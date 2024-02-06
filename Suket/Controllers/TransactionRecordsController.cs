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
    public class TransactionRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TransactionRecords
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TransactionRecord.Include(t => t.Post).Include(t => t.UserAccount);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TransactionRecords/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TransactionRecord == null)
            {
                return NotFound();
            }

            var transactionRecord = await _context.TransactionRecord
                .Include(t => t.Post)
                .Include(t => t.UserAccount)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transactionRecord == null)
            {
                return NotFound();
            }

            return View(transactionRecord);
        }

        // GET: TransactionRecords/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["PostId"] = new SelectList(_context.Post, "PostId", "Place");
            ViewData["UserAccountId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: TransactionRecords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserAccountId,Type,Amount,PostId,PaymentIntentId,TransactionDate")] TransactionRecord transactionRecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(transactionRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PostId"] = new SelectList(_context.Post, "PostId", "Place", transactionRecord.PostId);
            ViewData["UserAccountId"] = new SelectList(_context.Users, "Id", "Id", transactionRecord.UserAccountId);
            return View(transactionRecord);
        }

        // GET: TransactionRecords/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TransactionRecord == null)
            {
                return NotFound();
            }

            var transactionRecord = await _context.TransactionRecord.FindAsync(id);
            if (transactionRecord == null)
            {
                return NotFound();
            }
            ViewData["PostId"] = new SelectList(_context.Post, "PostId", "Place", transactionRecord.PostId);
            ViewData["UserAccountId"] = new SelectList(_context.Users, "Id", "Id", transactionRecord.UserAccountId);
            return View(transactionRecord);
        }

        // POST: TransactionRecords/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserAccountId,Type,Amount,PostId,PaymentIntentId,TransactionDate")] TransactionRecord transactionRecord)
        {
            if (id != transactionRecord.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transactionRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionRecordExists(transactionRecord.Id))
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
            ViewData["PostId"] = new SelectList(_context.Post, "PostId", "Place", transactionRecord.PostId);
            ViewData["UserAccountId"] = new SelectList(_context.Users, "Id", "Id", transactionRecord.UserAccountId);
            return View(transactionRecord);
        }

        // GET: TransactionRecords/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TransactionRecord == null)
            {
                return NotFound();
            }

            var transactionRecord = await _context.TransactionRecord
                .Include(t => t.Post)
                .Include(t => t.UserAccount)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (transactionRecord == null)
            {
                return NotFound();
            }

            return View(transactionRecord);
        }

        // POST: TransactionRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TransactionRecord == null)
            {
                return Problem("Entity set 'ApplicationDbContext.TransactionRecord'  is null.");
            }
            var transactionRecord = await _context.TransactionRecord.FindAsync(id);
            if (transactionRecord != null)
            {
                _context.TransactionRecord.Remove(transactionRecord);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionRecordExists(int id)
        {
          return (_context.TransactionRecord?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
