using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Suket.Data;
using Suket.Models;

namespace Suket.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserAccount> _userManager;

        public ReviewsController(ApplicationDbContext context, UserManager<UserAccount> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reviews
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Review.Include(r => r.Post).Include(r => r.Reviewed).Include(r => r.Reviewer);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Review == null)
            {
                return NotFound();
            }

            var review = await _context.Review
                .Include(r => r.Post)
                .Include(r => r.Reviewed)
                .Include(r => r.Reviewer)
                .FirstOrDefaultAsync(m => m.ReviewId == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // GET: Reviews/Create
        public IActionResult Create()
        {
            ViewData["PostId"] = new SelectList(_context.Post, "PostId", "Item");
            ViewData["ReviewedId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ReviewerId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Reviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ReviewId,MannerLevel,SkillLevel,ReviewerId,ReviewedId,PostId")] Review review)
        {
            if (ModelState.IsValid)
            {
                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PostId"] = new SelectList(_context.Post, "PostId", "Item", review.PostId);
            ViewData["ReviewedId"] = new SelectList(_context.Users, "Id", "Id", review.ReviewedId);
            ViewData["ReviewerId"] = new SelectList(_context.Users, "Id", "Id", review.ReviewerId);
            return View(review);
        }

        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Review == null)
            {
                return NotFound();
            }

            var review = await _context.Review.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            ViewData["PostId"] = new SelectList(_context.Post, "PostId", "Item", review.PostId);
            ViewData["ReviewedId"] = new SelectList(_context.Users, "Id", "Id", review.ReviewedId);
            ViewData["ReviewerId"] = new SelectList(_context.Users, "Id", "Id", review.ReviewerId);
            return View(review);
        }

        // POST: Reviews/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReviewId,MannerLevel,SkillLevel,ReviewerId,ReviewedId,PostId")] Review review)
        {
            if (id != review.ReviewId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.ReviewId))
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
            ViewData["PostId"] = new SelectList(_context.Post, "PostId", "Item", review.PostId);
            ViewData["ReviewedId"] = new SelectList(_context.Users, "Id", "Id", review.ReviewedId);
            ViewData["ReviewerId"] = new SelectList(_context.Users, "Id", "Id", review.ReviewerId);
            return View(review);
        }

        // GET: Reviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Review == null)
            {
                return NotFound();
            }

            var review = await _context.Review
                .Include(r => r.Post)
                .Include(r => r.Reviewed)
                .Include(r => r.Reviewer)
                .FirstOrDefaultAsync(m => m.ReviewId == id);
            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Review == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Review'  is null.");
            }
            var review = await _context.Review.FindAsync(id);
            if (review != null)
            {
                _context.Review.Remove(review);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReviewExists(int id)
        {
          return (_context.Review?.Any(e => e.ReviewId == id)).GetValueOrDefault();
        }

        public IActionResult ReviewablePosts()
        {
            var now = DateTimeOffset.Now;

            // 現在のユーザーIDを取得
            var currentUserId = _userManager.GetUserId(User);

            // 開催日時をUTC時間に変換
            DateTime utcTime = DateTimeOffset.UtcNow.UtcDateTime;

            var reviewablePosts = _context.Post
                            .Include(p => p.UserAccount)
                            .Include(p => p.Reviews)
                            .Include(p => p.Adoptions)
                            .ThenInclude(a => a.UserAccount)
                            .Where(p => p.Time < utcTime && !p.Reviews.Any())  // now -> utcTime
                            .ToList();


            // 現在のユーザーが募集者のPostと、採用者のPostを分けて管理
            var reviewablePostsList = new List<ReviewablePostsViewModel>();

            foreach (var post in reviewablePosts)
            {
                if (post.UserAccountId == currentUserId)
                {
                    foreach (var adoption in post.Adoptions)
                    {
                        reviewablePostsList.Add(new ReviewablePostsViewModel { Post = post, UserToReview = adoption.UserAccount });
                    }
                }
                else if (post.Adoptions.Any(a => a.UserAccountId == currentUserId))
                {
                    reviewablePostsList.Add(new ReviewablePostsViewModel { Post = post, UserToReview = post.UserAccount });
                }
            }

            return View(reviewablePostsList);
        }

    }
}
