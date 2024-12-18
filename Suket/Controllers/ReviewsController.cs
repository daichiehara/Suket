﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
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

        [Authorize]
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
                            .Include(p => p.RollCalls)  // RollCall をロード
                            //.Where(p => p.Time < utcTime && p.RollCalls.Any(r => !p.Reviews.Any(rv => rv.ReviewedId == r.UserAccountId)))  // RollCall を基にレビューの条件を変更
                            .Where(p => p.Time < utcTime)
                            .ToList();

            var reviewablePostsList = new List<ReviewablePostsViewModel>();

            
            foreach (var post in reviewablePosts)
            {
                if (post.UserAccountId == currentUserId)
                {
                    foreach (var rollCall in post.RollCalls)
                    {
                        if (!post.Reviews.Any(r => r.ReviewedId == rollCall.UserAccountId))
                        {
                            reviewablePostsList.Add(new ReviewablePostsViewModel { Post = post, UserToReview = rollCall.UserAccount });
                        }
                    }
                }
                else if (post.RollCalls.Any(r => r.UserAccountId == currentUserId))
                {
                    if (!post.Reviews.Any(r => r.ReviewedId == post.UserAccountId))
                    {
                        reviewablePostsList.Add(new ReviewablePostsViewModel { Post = post, UserToReview = post.UserAccount });
                    }
                }
            }
            
            /*
            foreach (var post in reviewablePosts)
            {
                if (post.UserAccountId == currentUserId)
                {
                    foreach (var rollCall in post.RollCalls)
                    {
                        // 既にWhere句でフィルタリングしているため、ここでのチェックは不要
                        reviewablePostsList.Add(new ReviewablePostsViewModel { Post = post, UserToReview = rollCall.UserAccount });
                    }
                }
                else if (post.RollCalls.Any(r => r.UserAccountId == currentUserId))
                {
                    // こちらも同様に、追加のチェックは不要
                    reviewablePostsList.Add(new ReviewablePostsViewModel { Post = post, UserToReview = post.UserAccount });
                }
            }
            */
            return View(reviewablePostsList);
        }


        // GET: Reviews/CreateReview
        [Authorize]
        public IActionResult CreateReview(int postId, string userId)
        {
            var post = _context.Post.FirstOrDefault(p => p.PostId == postId);
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);

            ViewData["PostId"] = postId;
            ViewData["UserId"] = userId;

            string displayName = user.NickName ?? user.UserName;
            ViewData["ReviewedUserName"] = displayName;

            // Is the review being written by the post's author?
            bool isReviewByAuthor = post.UserAccountId == userId;
            ViewData["IsReviewByAuthor"] = isReviewByAuthor;


            if (post == null || user == null)
            {
                return NotFound();
            }

            var review = new Review
            {
                PostId = postId,
                ReviewedId = userId
            };

            return View(review);
        }

        // POST: Reviews/CreateReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CreateReview(Review review)
        {
            var post = _context.Post.FirstOrDefault(p => p.PostId == review.PostId);

            if (post == null)
            {
                return NotFound();
            }

            // Is the review being written by the post's author?
            bool isReviewByAuthor = post.UserAccountId == review.ReviewerId;

            /*
            if (isReviewByAuthor)
            {
                // If the review is being written by the post's author, set SkillLevel to null and remove it from model state
                review.SkillLevel = null;
                ModelState.Remove("SkillLevel");
            }
            */

            if (ModelState.IsValid)
            {
                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ReviewablePosts));
            }

            if (!ModelState.IsValid)
            {
                // Validation failed, print the errors
                List<string> errorMessages = new List<string>();
                foreach (var modelState in ViewData.ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        errorMessages.Add(error.ErrorMessage);
                    }
                }

                // Pass error messages to the view
                ViewData["Errors"] = errorMessages;

                // Return to the view with the model containing errors
                return View(review);
            }

            ViewData["PostId"] = new SelectList(_context.Post, "PostId", "Item", review.PostId);
            ViewData["ReviewedId"] = new SelectList(_context.Users, "Id", "Id", review.ReviewedId);

            return View(review);
        }

        /*
        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifySkillLevelRequirement(string reviewerId, int postId, int? skillLevel)
        {
            var reviewer = _context.Users.FirstOrDefault(u => u.Id == reviewerId);
            var post = _context.Post.FirstOrDefault(p => p.PostId == postId);

            if (reviewer == null || post == null)
            {
                return Json(false);
            }

            // Check if the review is being written by the post's author
            var isReviewByAuthor = reviewerId == post.UserAccountId;

            // If the review is being written by the post's author, SkillLevel is not required.
            if (isReviewByAuthor)
            {
                return Json(true);
            }

            
            if (!isReviewByAuthor && (skillLevel == null))
            {
                return Json($"スキルレベルの入力は必須です。");
            }
            

            // Otherwise, SkillLevel is required.
            return Json(false);
        }
        */
    }
}
