using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Stripe;
using Suket.Data;
using Suket.Models;
using X.PagedList;

namespace Suket.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserAccount> _userManager;  // UserManagerの追加
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly ISuketEmailSender _emailSender;
        private readonly INotificationService _notificationService;


        public PostsController(ApplicationDbContext context, UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager, ISuketEmailSender emailSender, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _notificationService = notificationService;
        }

        // GET: Posts
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 9, Genre? genre = null, Prefecture? prefecture = null, string? searchString = null)
        {
            var model = await GetPosts(page, pageSize, genre, prefecture, searchString);
            return View(model);
        }

        private async Task<PostIndexViewModel> GetPosts(int page, int pageSize, Genre? genre, Prefecture? prefecture, string? searchString = null)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var postsQuery = _context.Post
                .Include(p => p.UserAccount)
                .Include(p => p.Subscriptions)
                .Where(p => p.State != State.End);

            // Filter posts by genre
            if (genre != null)
            {
                postsQuery = postsQuery.Where(p => p.Genre == genre);
            }

            // Filter posts by prefecture
            if (prefecture != null)
            {
                postsQuery = postsQuery.Where(p => p.Prefecture == prefecture);
            }

            // Full text search by searchString
            if (!string.IsNullOrEmpty(searchString))
            {
                postsQuery = postsQuery.Where(p => p.Title.Contains(searchString) || p.Message.Contains(searchString));
            }

            // ここで、ページングを適用します
            var posts = await postsQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            // 全投稿数を取得します
            var totalPosts = await postsQuery.CountAsync();
            

            // Like counts for each post
            var subscriptionCounts = posts.ToDictionary(
                p => p.PostId,
                p => p.Subscriptions.Count);

            var userSubscriptionPosts = new Dictionary<int, bool>();
            var userAdoptionPosts = new Dictionary<int, bool>();

            if (currentUser != null)
            {
                var userSubscriptions = _context.Subscription
                    .Where(s => s.UserAccountId == currentUser.Id)
                    .Select(s => s.PostId);

                var userAdoptions = _context.Adoption
                    .Where(a => a.UserAccountId == currentUser.Id)
                    .Select(a => a.PostId);

                userSubscriptionPosts = posts.ToDictionary(
                    p => p.PostId,
                    p => userSubscriptions.Contains(p.PostId));

                userAdoptionPosts = posts.ToDictionary(
                    p => p.PostId,
                    p => userAdoptions.Contains(p.PostId));
            }

            // In GetPosts
            ViewData["SelectedGenre"] = genre;
            // Save current prefecture in ViewData
            ViewData["CurrentPrefecture"] = prefecture;
            // Save searchString in ViewData
            ViewData["CurrentSearch"] = searchString;

            // Pass like counts and adoption user Ids to the view via ViewData
            ViewData["SubscriptionCounts"] = subscriptionCounts;
            ViewData["UserSubscriptionPosts"] = userSubscriptionPosts;
            ViewData["UserAdoptionPosts"] = userAdoptionPosts;

            //return posts;
            return new PostIndexViewModel
            {
                Posts = posts,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalPosts / (double)pageSize),
                // 他のプロパティもここで設定します
            };
        }




        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            // Only get posts where State != State.End
            var posts = await _context.Post
                .Include(p => p.UserAccount)
                .Include(p => p.Subscriptions)
                .Where(p => p.State != State.End)
                .ToListAsync();

            if (id == null || _context.Post == null)
            {
                return NotFound();
            }

            var post = await _context.Post
                .Include(p => p.UserAccount)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            // Get the replies for the post
            var replies = await _context.Reply
                .Where(r => r.PostId == id)
                .Include(r => r.UserAccount)  // If you want to display information about the user who made the reply
                .ToListAsync();

            // Store the replies in the ViewData
            ViewData["Replies"] = replies;

            var subscriptions = await _context.Subscription
                .Where(s => s.PostId == id)
                .ToListAsync();

            var adoptees = await _context.Adoption
                .Where(a => a.PostId == id)
                .ToListAsync();

            List<string> adoptedUsers = new List<string>();
            List<string> subscribedUsers = new List<string>();


            foreach (var adoption in adoptees)
            {
                adoptedUsers.Add(adoption.UserAccountId);
            }
            foreach (var subscription in subscriptions)
            {
                subscribedUsers.Add(subscription.UserAccountId);
            }

            // Add the lists to ViewData
            ViewData["AdoptedUsers"] = adoptedUsers;
            ViewData["SubscribedUsers"] = subscribedUsers;

            var userSubscriptionPosts = new Dictionary<int, bool>();
            var userAdoptionPosts = new Dictionary<int, bool>();

            if (currentUser != null)
            {
                var userSubscriptions = _context.Subscription
                    .Where(s => s.UserAccountId == currentUser.Id)
                    .Select(s => s.PostId);

                var userAdoptions = _context.Adoption
                    .Where(a => a.UserAccountId == currentUser.Id)
                .Select(a => a.PostId);

                userSubscriptionPosts = posts.ToDictionary(
                    p => p.PostId,
                    p => userSubscriptions.Contains(p.PostId));

                userAdoptionPosts = posts.ToDictionary(
                    p => p.PostId,
                    p => userAdoptions.Contains(p.PostId));
            }

            // Pass like counts and adoption user Ids to the view via ViewData
            ViewData["UserSubscriptionPosts"] = userSubscriptionPosts;
            ViewData["UserAdoptionPosts"] = userAdoptionPosts;


            return View(post);
        }

        // GET: Posts/Create
        public IActionResult Create()
        {
            ViewData["UserAccountId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostId,Title,PeopleCount,Prefecture,Place,Time,Item,Reward,Message,Created,Genre,State,Certification,UserAccountId")] Post post)
        {
            var currentUser = await _userManager.GetUserAsync(User); // 現在のユーザーを取得
            var currentUserId = currentUser?.Id;  // 現在のユーザーのIDを取得
            
            if (ModelState.IsValid)
            {
                post.Time = post.Time.ToUniversalTime();
                post.Created = DateTime.UtcNow;

                Random r1 = new Random();
                post.Certification = r1.Next(1000, 9999);


                _context.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
                return View(post);
            }
            //ViewData["UserAccountId"] = new SelectList(_context.Users, "Id", "Id", post.UserAccountId);
            return View(post);
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Post == null)
            {
                return NotFound();
            }

            var post = await _context.Post.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            ViewData["UserAccountId"] = new SelectList(_context.Users, "Id", "Id", post.UserAccountId);
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,PeopleCount,Prefecture,Place,Time,Item,Reward,Message,Created,Genre,State,UserAccountId")] Post post)
        {
            //データベースから元のエンティティを取得します
            var originalPost = await _context.Post.AsNoTracking().FirstOrDefaultAsync(m => m.PostId == id);
            //元のエンティティのUserAccountIdを新しいエンティティのUserAccountIdに設定します。
            post.UserAccountId = originalPost.UserAccountId;

            if (id != post.PostId)
            {
                return NotFound();
            }
            /*
            if (post.State == State.End)
            {
                return RedirectToAction(nameof(Index));
            }
            */
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
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
                return View(post);
            }
            ViewData["UserAccountId"] = new SelectList(_context.Users, "Id", "Id", post.UserAccountId);

            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Post == null)
            {
                return NotFound();
            }

            var post = await _context.Post
                .Include(p => p.UserAccount)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            var adoptionExists = await _context.Adoption.AnyAsync(a => a.PostId == id);
            ViewData["AdoptionExists"] = adoptionExists;

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var adoptionExists = await _context.Adoption.AnyAsync(a => a.PostId == id);
            if (adoptionExists)
            {
                return View("Error", new ErrorViewModel { RequestId = "採用者が存在するので投稿は削除できません" });
            }

            var post = await _context.Post.FindAsync(id);
            _context.Post.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }




        private bool PostExists(int id)
        {
          return (_context.Post?.Any(e => e.PostId == id)).GetValueOrDefault();
        }

        

        [HttpPost]
        public async Task<IActionResult> ToggleSubscription(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var subscription = await _context.Subscription.FirstOrDefaultAsync(l => l.PostId == id && l.UserAccountId == user.Id);
            if (subscription != null)
            {
                // If the user has already liked this post, remove the like
                _context.Subscription.Remove(subscription);
            }
            else
            {
                // If the user has not liked this post yet, add a new like
                subscription = new Models.Subscription
                {
                    PostId = id,
                    UserAccountId = user.Id
                };
                _context.Subscription.Add(subscription);
            }

            await _context.SaveChangesAsync();

            return Ok(); // the updated like count is no longer returned
        }

        // PostsController.cs
        public async Task<IActionResult> Subscriber(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Post.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            ViewData["PostId"] = id;

            ViewData["PostUserId"] = post.UserAccountId;

            var subscriptions = await _context.Subscription
                .Where(s => s.PostId == id)
                .ToListAsync();

            var adoptees = await _context.Adoption
                .Where(a => a.PostId == id)
                .ToListAsync();

            List<UserAccount> applicants = new List<UserAccount>();
            List<UserAccount> adopted = new List<UserAccount>();
            foreach (var subscription in subscriptions)
            {
                var user = await _userManager.FindByIdAsync(subscription.UserAccountId);
                if (user != null)
                {
                    applicants.Add(user);
                }
            }

            foreach (var adoption in adoptees)
            {
                var user = await _userManager.FindByIdAsync(adoption.UserAccountId);
                if (user != null)
                {
                    adopted.Add(user);
                }
            }

            return View((applicants as IEnumerable<UserAccount>, adopted as IEnumerable<UserAccount>));

        }


        
        [HttpPost]
        public async Task<IActionResult> Adopt(string userId, int postId)
        {

            // Check if an adoption record already exists
            var existingAdoption = await _context.Adoption
                .Where(a => a.UserAccountId == userId && a.PostId == postId)
                .FirstOrDefaultAsync();

            bool isAdopted;

            // If it does not exist, create it
            if (existingAdoption == null)
            {
                var adoption = new Adoption
                {
                    UserAccountId = userId,
                    PostId = postId
                };
                _context.Adoption.Add(adoption);
                isAdopted = true;  // A new Adoption record is created
            }
            else
            {
                isAdopted = true;  // An existing Adoption record is found
            }

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, isAdopted = isAdopted });
            }
            catch (Exception e)
            {
                return Json(new { success = false, error = e.Message });
            }
        }

        public async Task<IActionResult> MySubscribedPosts(bool showAdopted = false)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            var userSubscriptionsIds = await _context.Subscription
                .Where(s => s.UserAccountId == currentUser.Id)
                .Select(s => s.PostId)
                .ToListAsync();

            var userSubscriptions = await _context.Post
                .Where(p => userSubscriptionsIds.Contains(p.PostId))
                .Include(p => p.Subscriptions)
                .Include(p => p.UserAccount) // Add this line
                .ToListAsync();


            // Like counts for each post
            var subscriptionCounts = userSubscriptions.ToDictionary(
                p => p.PostId,
                p => p.Subscriptions.Count);

            var userSubscriptionPosts = userSubscriptions.ToDictionary(
                p => p.PostId,
                p => true);

            var userAdoptionPosts = await _context.Adoption
                .Where(a => a.UserAccountId == currentUser.Id)
                .ToDictionaryAsync(a => a.PostId, a => true);

            if (showAdopted)
            {
                userSubscriptions = userSubscriptions
                    .Where(p => userAdoptionPosts.ContainsKey(p.PostId))
                    .ToList();
            }

            // Pass like counts and adoption user Ids to the view via ViewData
            ViewData["SubscriptionCounts"] = subscriptionCounts;
            ViewData["UserSubscriptionPosts"] = userSubscriptionPosts;
            ViewData["UserAdoptionPosts"] = userAdoptionPosts;

            return View(userSubscriptions);
        }

        public async Task<IActionResult> MyPosts(int? page)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return NotFound();
            }

            var myPosts = await _context.Post
                .Where(p => p.UserAccountId == currentUser.Id)
                .Include(p => p.Subscriptions)
                .ToListAsync();

            // Like counts for each post
            var subscriptionCounts = myPosts.ToDictionary(
                p => p.PostId,
                p => p.Subscriptions.Count);

            var userSubscriptionPosts = new Dictionary<int, bool>();
            var userAdoptionPosts = new Dictionary<int, bool>();
            if (currentUser != null)
            {
                var userSubscriptions = _context.Subscription
                    .Where(s => s.UserAccountId == currentUser.Id)
                    .Select(s => s.PostId);

                var userAdoptions = _context.Adoption
                    .Where(a => a.UserAccountId == currentUser.Id)
                    .Select(a => a.PostId);

                userSubscriptionPosts = myPosts.ToDictionary(
                    p => p.PostId,
                    p => userSubscriptions.Contains(p.PostId));

                userAdoptionPosts = myPosts.ToDictionary(
                    p => p.PostId,
                    p => userAdoptions.Contains(p.PostId));
            }

            // Pass like counts and adoption user Ids to the view via ViewData
            ViewData["SubscriptionCounts"] = subscriptionCounts;
            ViewData["UserSubscriptionPosts"] = userSubscriptionPosts;
            ViewData["UserAdoptionPosts"] = userAdoptionPosts;

            // Using ToPagedList extension method from X.PagedList.Mvc.Core
            int pageNumber = (page ?? 1);
            var onePageOfPosts = myPosts.ToPagedList(pageNumber, 15); // 15 posts per page

            return View(onePageOfPosts);
        }



        [HttpPost]
        public async Task<IActionResult> CreateReply(int PostId, string Message)
        {
            var userId = _userManager.GetUserId(User);  // Get the ID of the currently logged in user
            var reply = new Suket.Models.Reply
            {
                Message = Message,
                Created = DateTime.UtcNow,
                PostId = PostId,
                UserAccountId = userId,  // Set the UserAccountId to the ID of the current user
                                         // Set other properties as necessary
            };
            _context.Reply.Add(reply);

            var post = await _context.Post.Include(p => p.UserAccount).FirstOrDefaultAsync(m => m.PostId == PostId);



            if (post != null)
            {
                if (post.UserAccountId == userId)
                {
                    // Get the email addresses and user names of all subscribers
                    var subscriberUsers = await _context.Subscription
                        .Where(s => s.PostId == PostId)
                        .Select(s => new { s.UserAccount.Email, s.UserAccount.NickName, s.UserAccount.UserName })
                        .ToListAsync();

                    var subject = Resources.Notification.NewCommentNotificationSubject;

                    // Send a notification to each subscriber
                    foreach (var user in subscriberUsers)
                    {
                        var usernameOrNickname = !string.IsNullOrEmpty(user.NickName) ? user.NickName : user.UserName;
                        var body = string.Format(Resources.Notification.NewCommentNotificationBody, usernameOrNickname, Message);

                        await _notificationService.SendEmailNotification(user.Email, subject, body);
                    }
                }
                // If the current user is not the poster, send notification to the poster
                else
                {
                    var posterEmail = post.UserAccount.Email;
                    var selectName = post.UserAccount.NickName ?? post.UserAccount.UserName;

                    // Get the email subject and body from the resource file
                    var subject = Resources.Notification.NewCommentNotificationSubject;
                    var body = string.Format(Resources.Notification.NewCommentNotificationBody, selectName, Message);

                    // Send a notification to the poster
                    await _emailSender.SendEmailAsync(posterEmail, subject, body);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = PostId });
        }


        [HttpPost]
        public async Task<IActionResult> SendGreeting()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // ユーザーが存在しない場合の処理
                return NotFound();
            }

            string email = user.Email;
            string subject = "Greetings from our app";
            string message = "Hello! This is a greeting message.";

            await _emailSender.SendEmailAsync(email, subject, message);

            // メール送信後の処理をここに書く
            return RedirectToAction("Index", "Home");
        }
    }

}
