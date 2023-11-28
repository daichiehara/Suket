using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Suket.Data;
using Suket.Models;

namespace Suket.Controllers
{
    public class RollCallsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserAccount> _userManager;

        public RollCallsController(ApplicationDbContext context, UserManager<UserAccount> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: RollCalls
        public async Task<IActionResult> Index()
        {
              return _context.RollCall != null ? 
                          View(await _context.RollCall.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.RollCall'  is null.");
        }

        // GET: RollCalls/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.RollCall == null)
            {
                return NotFound();
            }

            var rollCall = await _context.RollCall
                .FirstOrDefaultAsync(m => m.RollCallId == id);
            if (rollCall == null)
            {
                return NotFound();
            }

            return View(rollCall);
        }

        // GET: RollCalls/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RollCalls/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RollCallId")] RollCall rollCall)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rollCall);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rollCall);
        }

        // GET: RollCalls/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.RollCall == null)
            {
                return NotFound();
            }

            var rollCall = await _context.RollCall.FindAsync(id);
            if (rollCall == null)
            {
                return NotFound();
            }
            return View(rollCall);
        }

        // POST: RollCalls/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RollCallId")] RollCall rollCall)
        {
            if (id != rollCall.RollCallId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rollCall);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RollCallExists(rollCall.RollCallId))
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
            return View(rollCall);
        }

        // GET: RollCalls/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.RollCall == null)
            {
                return NotFound();
            }

            var rollCall = await _context.RollCall
                .FirstOrDefaultAsync(m => m.RollCallId == id);
            if (rollCall == null)
            {
                return NotFound();
            }

            return View(rollCall);
        }

        // POST: RollCalls/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.RollCall == null)
            {
                return Problem("Entity set 'ApplicationDbContext.RollCall'  is null.");
            }
            var rollCall = await _context.RollCall.FindAsync(id);
            if (rollCall != null)
            {
                _context.RollCall.Remove(rollCall);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RollCallExists(int id)
        {
          return (_context.RollCall?.Any(e => e.RollCallId == id)).GetValueOrDefault();
        }

        [Authorize]
        public async Task<IActionResult> VerifyAttendance(int id)
        {
            var post = _context.Post.FirstOrDefault(p => p.PostId == id);
            if (post != null)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    var existingRollCall = _context.RollCall.FirstOrDefault(rc => rc.PostId == id && rc.UserAccountId == currentUser.Id);
                    if (existingRollCall != null)
                    {
                        return RedirectToAction("AttendanceConfirmed");
                    }
                }

                // UserAccountを取得
                var userAccount = await _context.Users.FindAsync(post.UserAccountId);

                var displayName = userAccount.NickName ?? userAccount.UserName;

                ViewData["PostId"] = id;
                ViewData["PostUserName"] = post.UserAccount.UserName;
                ViewData["PostDisplayName"] = displayName;
                ViewData["PostTitle"] = post.Title;
                ViewData["PostPlace"] = post.Place;
            }

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> VerifyAttendance(int postId, string userAccountId, int certificationCode)
        {
            try
            {
                var post = await _context.Post.FindAsync(postId);
                if (post == null)
                {
                    return NotFound();
                }

                if (post.Certification != certificationCode)
                {
                    ModelState.AddModelError("CertificationCode", "認証コードが違います。再度コードを確認してください。");
                    
                    ViewData["PostId"] = postId;
                    ViewData["PostTitle"] = post.Title;
                    ViewData["PostPlace"] = post.Place;

                    return View(new VerifyAttendanceViewModel { PostId = postId, UserAccountId = userAccountId }); // Pass a new RollCall as the model
                }

                var existingRollCall = await _context.RollCall.FirstOrDefaultAsync(rc => rc.PostId == postId && rc.UserAccountId == userAccountId);
                if (existingRollCall != null)
                {
                    return RedirectToAction("AttendanceConfirmed");
                }

                var rollCall = new RollCall
                {
                    UserAccountId = userAccountId,
                    PostId = postId
                };

                _context.RollCall.Add(rollCall);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "認証が成功しました！";

                return RedirectToAction("Details", "Posts", new { id = postId });
            }
            catch (Exception ex)
            {
                // Log the exception
                // You might use a logging framework like log4net, Serilog, or NLog here
                System.Diagnostics.Debug.WriteLine(ex);
                return View("Error"); // Or return an appropriate error view
            }
        }

        [Authorize]
        public IActionResult AttendanceConfirmed()
        {
            return View();
        }

    }

}
