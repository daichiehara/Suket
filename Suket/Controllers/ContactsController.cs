using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Suket.Data;
using Suket.Models;

namespace Suket.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISuketEmailSender _emailSender;

        public ContactsController(ApplicationDbContext context, ISuketEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET: Contacts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Contact.Include(c => c.UserAccount);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Contacts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Contact == null)
            {
                return NotFound();
            }

            var contact = await _context.Contact
                .Include(c => c.UserAccount)
                .FirstOrDefaultAsync(m => m.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // GET: Contacts/Create
        public IActionResult Create()
        {
            ViewData["UserAccountId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Contacts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ContactId,Message,Email,Created,UserAccountId")] Contact contact)
        {
            if (ModelState.IsValid)
            {
                contact.Created = DateTime.UtcNow;

                var emailAddress = contact.Email;
                var adminEmailAddress = "support@mintsports.net";
                //var selectName = contact.UserAccount.NickName ?? contact.UserAccount.UserName;

                // Get the email subject and body from the resource file
                var subject = Resources.ContactEmail.Subject;
                var body = string.Format(Resources.ContactEmail.Body, contact.Message);

                var adminSubject = Resources.ContactEmail.AdminSubject;
                var adminBody = string.Format(Resources.ContactEmail.AdminBody, contact.Created, contact.Message);

                // Send a notification to the poster
                await _emailSender.SendEmailAsync(emailAddress, subject, body);
                // 管理者に通知を送る
                await _emailSender.SendEmailAsync(adminEmailAddress, adminSubject, adminBody);

                _context.Add(contact);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserAccountId"] = new SelectList(_context.Users, "Id", "Id", contact.UserAccountId);
            return View(contact);
        }

        // GET: Contacts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Contact == null)
            {
                return NotFound();
            }

            var contact = await _context.Contact.FindAsync(id);
            if (contact == null)
            {
                return NotFound();
            }
            ViewData["UserAccountId"] = new SelectList(_context.Users, "Id", "Id", contact.UserAccountId);
            return View(contact);
        }

        // POST: Contacts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ContactId,Message,Email,UserAccountId")] Contact contact)
        {
            if (id != contact.ContactId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contact);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContactExists(contact.ContactId))
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
            ViewData["UserAccountId"] = new SelectList(_context.Users, "Id", "Id", contact.UserAccountId);
            return View(contact);
        }

        // GET: Contacts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Contact == null)
            {
                return NotFound();
            }

            var contact = await _context.Contact
                .Include(c => c.UserAccount)
                .FirstOrDefaultAsync(m => m.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }

            return View(contact);
        }

        // POST: Contacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Contact == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Contact'  is null.");
            }
            var contact = await _context.Contact.FindAsync(id);
            if (contact != null)
            {
                _context.Contact.Remove(contact);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContactExists(int id)
        {
          return (_context.Contact?.Any(e => e.ContactId == id)).GetValueOrDefault();
        }
    }
}
