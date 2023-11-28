using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using Stripe;
using Stripe.Checkout;
using Suket.Data;
using Suket.Models;
using X.PagedList;
using System.Text.Json;
using System.Collections.Generic;


namespace Suket.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<UserAccount> _userManager;  // UserManagerの追加
        private readonly SignInManager<UserAccount> _signInManager;
        private readonly ISuketEmailSender _emailSender;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PostsController> _logger;

        public PostsController(ApplicationDbContext context, UserManager<UserAccount> userManager, SignInManager<UserAccount> signInManager, ISuketEmailSender emailSender, INotificationService notificationService, ILogger<PostsController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _notificationService = notificationService;
            _logger = logger;
        }

        /*
        private static string GetStripeAPIKeyFromAzureKeyVault()
        {
            var keyVaultUrl = "https://stripetestapikey.vault.azure.net/";
            var secretName = "StripeTestAPIKey";

            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            KeyVaultSecret stripeAPIKeySecret = client.GetSecret(secretName);

            return stripeAPIKeySecret.Value;
        }
        */

        private static async Task<string> GetStripeAPIKeyFromAWSSecretsManager()
        {
            string secretName = "MintSPORTS_secret";  // シークレットの名前を変更
            string region = "ap-northeast-1";

            IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

            GetSecretValueRequest request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
            };

            GetSecretValueResponse response;

            try
            {
                response = await client.GetSecretValueAsync(request);
            }
            catch (Exception e)
            {
                // For a list of the exceptions thrown, see
                // https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
                throw e;
            }

            // JSONからStripeAPIKeyを取得
            var secrets = JsonSerializer.Deserialize<Dictionary<string, string>>(response.SecretString);
            if (secrets != null && secrets.ContainsKey("StripeAPIKey"))
            {
                return secrets["StripeAPIKey"];
            }

            throw new Exception("StripeAPIKey not found in the secret.");
        }


        // GET: Posts
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 30, Genre? genre = null, Prefecture? prefecture = null, string? searchString = null, DateTimeOffset? fromDateTime = null, bool sortByDateTime = false)
        {
            var model = await GetPosts(page, pageSize, genre, prefecture, searchString, fromDateTime, sortByDateTime);

            // Build the query string for the pagination links
            var queryString = new StringBuilder();
            if (genre != null)
            {
                queryString.Append($"genre={genre}&");
            }
            if (prefecture != null)
            {
                queryString.Append($"prefecture={prefecture}&");
            }
            if (!string.IsNullOrEmpty(searchString))
            {
                queryString.Append($"searchString={HttpUtility.UrlEncode(searchString)}&");
            }
            if (fromDateTime != null)
            {
                queryString.Append($"fromDateTime={HttpUtility.UrlEncode(fromDateTime.Value.ToString("yyyy-MM-ddTHH:mm"))}&");
            }
            if (sortByDateTime)
            {
                queryString.Append("sortByDateTime=true&");
            }

            // Save the query string in ViewData to pass it to the view
            ViewData["QueryString"] = queryString.ToString().TrimEnd('&');

            // Save fromDateTime in ViewData
            ViewData["FromDateTime"] = fromDateTime?.ToString("yyyy-MM-ddTHH:mm");

            return View(model);
        }

        private async Task<PostIndexViewModel> GetPosts(int page, int pageSize, Genre? genre, Prefecture? prefecture, string? searchString = null, DateTimeOffset? fromDateTime = null, bool sortByDateTime = false)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var postsQuery = _context.Post
                .Include(p => p.UserAccount)
                .Include(p => p.Subscriptions)
                .Where(p => (p.State != State.End) && (p.State != State.Cancel))
                .OrderByDescending(p => p.Created);

            // Filter posts by genre
            if (genre != null)
            {
                postsQuery = postsQuery.Where(p => p.Genre == genre)
                                       .OrderByDescending(p => p.Created);
            }

            // Filter posts by prefecture
            if (prefecture != null)
            {
                postsQuery = postsQuery.Where(p => p.Prefecture == prefecture)
                                       .OrderByDescending(p => p.Created);
            }

            // Full text search by searchString
            /*
            if (!string.IsNullOrEmpty(searchString))
            {
                postsQuery = postsQuery.Where(p => p.Title.Contains(searchString) || p.Message.Contains(searchString))
                                       .OrderBy(p => p.Created);
            }
            */
            // Full text search by searchString
            if (!string.IsNullOrEmpty(searchString))
            {
                var searchWords = searchString.Split(' ').Where(word => !string.IsNullOrEmpty(word)).ToList();

                var searchPredicate = PredicateBuilder.New<Post>(false);  // 'false' means start with OR condition
                foreach (var word in searchWords)
                {
                    string tempWord = word;  // necessary for correct closure capture
                    searchPredicate = searchPredicate.Or(p => p.Title.Contains(tempWord));
                    searchPredicate = searchPredicate.Or(p => p.Message.Contains(tempWord));
                }

                postsQuery = postsQuery.Where(searchPredicate)
                                       .OrderByDescending(p => p.Created);
            }


            if (fromDateTime != null)
            {
                // Convert to UTC before filtering
                var utcFromDateTime = fromDateTime.Value.ToUniversalTime();
                postsQuery = postsQuery.Where(p => p.Time >= utcFromDateTime)
                                       .OrderBy(p => p.Time);
            }

            if (sortByDateTime)
            {
                // Order the posts by post.Time in ascending order (earliest to latest)
                postsQuery = postsQuery.OrderBy(p => p.Time);
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

            // In GetPosts method
            ViewData["FromDateTime"] = fromDateTime?.ToString("yyyy/MM/ddTHH:mm");
            //ViewData["FromDateTime"] = fromDateTime;
            // Save the sortByDateTime value in ViewData to keep track of checkbox state
            ViewData["SortByDateTime"] = sortByDateTime;

            // Pass like counts and adoption user Ids to the view via ViewData
            ViewData["SubscriptionCounts"] = subscriptionCounts;
            ViewData["UserSubscriptionPosts"] = userSubscriptionPosts;
            ViewData["UserAdoptionPosts"] = userAdoptionPosts;
            ViewData["TotalPosts"] = totalPosts;

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
            if (TempData["ErrorMessage"] != null)
            {
                ModelState.AddModelError(string.Empty, TempData["ErrorMessage"].ToString());
            }

            var currentUser = await _userManager.GetUserAsync(User);
            // Only get posts where State != State.End
            //state.endにもんだいあり
            var posts = await _context.Post
                .Include(p => p.UserAccount)
                .Include(p => p.Subscriptions)
                //.Where(p => p.State != State.End)
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
                .OrderBy(r => r.Created)
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
        [Authorize]
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
                // 日本のタイムゾーンを取得
                TimeZoneInfo japanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

                // post.TimeがDateTimeOffset型である場合、DateTimeに変換
                DateTime localDateTime = post.Time.DateTime;

                // ユーザーが入力した日本時間（ローカルタイム）をUTCに変換
                post.Time = TimeZoneInfo.ConvertTimeToUtc(localDateTime, japanTimeZone);

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

        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyReward(int Reward)
        {
            if (Reward != 0 && Reward < 500)
            {
                return Json("報酬は0もしくは500円以上にしてください。");
            }

            return Json(true);
        }

        // GET: Posts/Edit/5
        [Authorize]
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

            var jstZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            post.Time = TimeZoneInfo.ConvertTimeFromUtc(post.Time.UtcDateTime, jstZone);

            ViewData["UserAccountId"] = new SelectList(_context.Users, "Id", "Id", post.UserAccountId);
            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,PeopleCount,Prefecture,Place,Time,Item,Reward,Message,Genre,State,Certification,UserAccountId")] Post post)
        {
            /*
            if (post.State == State.End)
            {
                return RedirectToAction(nameof(Index));
            }
            */
            //データベースから元のエンティティを取得します
            var originalPost = await _context.Post.AsNoTracking().FirstOrDefaultAsync(m => m.PostId == id);

            // Rewardが減らされていないことを確認
            if (originalPost.Reward > post.Reward)
            {
                ModelState.AddModelError("Reward", "報酬は増やすことはできますが、減らすことはできません。");
                return View(post);
            }

            //元のエンティティのUserAccountIdを新しいエンティティのUserAccountIdに設定します。
            post.UserAccountId = originalPost.UserAccountId;
            post.Created = originalPost.Created;
            post.Certification = originalPost.Certification;

            if (id != post.PostId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // 日本のタイムゾーンを取得
                TimeZoneInfo japanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

                // post.TimeがDateTimeOffset型である場合、DateTimeに変換
                DateTime localDateTime = post.Time.DateTime;

                // ユーザーが入力した日本時間（ローカルタイム）をUTCに変換
                post.Time = TimeZoneInfo.ConvertTimeToUtc(localDateTime, japanTimeZone);

                // StateがCancelに変更されたかどうかを確認
                if (originalPost.State != State.Cancel && post.State == State.Cancel)
                {
                    var paymentRecords = await _context.PaymentRecord
                                                       .Where(pr => pr.PostId == id)
                                                       .ToListAsync();

                    var uniquePaymentIntentIds = paymentRecords.Select(pr => pr.PaymentIntentId)
                                                               .Distinct()
                                                               .ToList();

                    StripeConfiguration.ApiKey = await GetStripeAPIKeyFromAWSSecretsManager();
                    foreach (var paymentIntentId in uniquePaymentIntentIds)
                    {
                        // Get the amount for this paymentIntentId to calculate the refund amount
                        var paymentIntentService = new PaymentIntentService();
                        var paymentIntent = paymentIntentService.Get(paymentIntentId);
                        long amountToRefund = paymentIntent.AmountReceived;

                        // Calculate the number of days between now and the post time
                        var daysBeforePostTime = (post.Time - DateTime.UtcNow).TotalDays;

                        // Determine the refund percentage based on the days before post time
                        double refundPercentage;
                        if (daysBeforePostTime > 5)
                        {
                            refundPercentage = 1.0; // 100% refund
                        }
                        else if (daysBeforePostTime > 2)
                        {
                            refundPercentage = 0.8; // 80% refund
                        }
                        else
                        {
                            refundPercentage = 0.5; // 50% refund
                        }

                        // Calculate the amount to refund
                        amountToRefund = (long)(amountToRefund * refundPercentage);

                        // 返金処理
                        try
                        {
                            // Stripe APIを使用して返金処理を行う（Stripe SDKの設定が必要）
                            // ここにStripeの返金APIを呼び出すコードを追加
                            var refundService = new RefundService();
                            var refundOptions = new RefundCreateOptions
                            {
                                PaymentIntent = paymentIntentId,
                                Amount = amountToRefund,
                                // 追加のオプションが必要な場合はここで設定します
                            };
                            var refund = refundService.Create(refundOptions);

                            // 返金が成功したら、PaymentRecordを更新
                            var recordsToUpdate = paymentRecords.Where(pr => pr.PaymentIntentId == paymentIntentId);
                            foreach (var record in recordsToUpdate)
                            {
                                record.Refunded = true;  // Refunded: 返金が行われたことを示す新しいプロパティ
                            }
                        }
                        catch (Exception ex)
                        {
                            // エラーハンドリング（ログの記録やユーザーへのフィードバック等）
                            _logger.LogError(ex, "Refund failed for PaymentIntentId: {PaymentIntentId}", paymentIntentId);
                        }
                    }

                    await _context.SaveChangesAsync();
                }

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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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
        [Authorize]
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

            ViewData["PostTime"] = post.Time;


            ViewData["PostId"] = id;

            ViewData["PostUserId"] = post.UserAccountId;

            ViewData["PostState"] = post.State;

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
        [Authorize]
        public async Task<IActionResult> Adopt(List<string> userIds, int postId)
        {
            bool isAdopted = false;
            List<string> errorMessages = new List<string>();

            foreach (var userId in userIds)
            {
                // Check if an adoption record already exists
                var existingAdoption = await _context.Adoption
                    .Where(a => a.UserAccountId == userId && a.PostId == postId)
                    .FirstOrDefaultAsync();

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

                    var user = await _context.Users.FindAsync(userId);
                    var emailAddress = user?.Email;  // Ensure the user was found before accessing the Email property

                    var post = await _context.Post.FindAsync(postId);

                    //var selectName = contact.UserAccount.NickName ?? contact.UserAccount.UserName;
                    string postTime = TimeZoneInfo.ConvertTimeFromUtc(post.Time.UtcDateTime, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time")).ToString("yyyy/MM/dd HH:mm");
                    // Get the email subject and body from the resource file
                    var subject = Resources.AdoptionEmail.Subject;
                    var body = string.Format(Resources.AdoptionEmail.Body, post.Title, post.Place, postTime, postId);
                    // Send a notification to the poster
                    if (!string.IsNullOrEmpty(emailAddress))
                    {
                        await _emailSender.SendEmailAsync(emailAddress, subject, body);
                    }
                }
                else
                {
                    isAdopted = true;  // An existing Adoption record is found
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException e)
                {
                    _logger.LogError(e, $"An error occurred while saving changes: {e.Message}");
                    _logger.LogError(e.InnerException, $"Inner exception: {e.InnerException?.Message}");
                    errorMessages.Add($"User ID {userId} Error: {e.Message}");
                }

            }

            if (errorMessages.Count > 0)
            {
                return Json(new { success = false, error = errorMessages });
            }
            else
            {
                return Json(new { success = true, isAdopted = isAdopted });
            }
        }

        // Controller
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateCheckoutSession(List<string> userIds, int postId)
        {
            try
            {
                _logger.LogInformation("Starting CreateCheckoutSession process.");

                StripeConfiguration.ApiKey = await GetStripeAPIKeyFromAWSSecretsManager();
                _logger.LogInformation("Stripe API Key retrieved from Azure Key Vault.");

                var post = await _context.Post.FindAsync(postId);
                if (post == null)
                {
                    _logger.LogWarning($"Post with ID {postId} not found.");
                    return NotFound();
                }

                // UserAccountを取得
                var userAccount = await _context.Users.FindAsync(post.UserAccountId);
                if (userAccount == null)
                {
                    _logger.LogWarning($"User account for post with ID {postId} not found.");
                    return NotFound();
                }

                if (post.Reward == 0)
                {
                    _logger.LogInformation("Post reward is zero.");
                    return Json(new { reward = 0 });
                }

                //var totalAmount = post.Reward * userIds.Count;
                var options = new SessionCreateOptions
                {
                    CustomerEmail = userAccount.Email,
                    Mode = "payment",
                    PaymentMethodTypes = new List<string> { "card" },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = post.Reward,
                                Currency = "jpy",
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = $"採用者({userIds.Count}人)への報酬",
                                },
                            },
                            Quantity = userIds.Count,
                        },
                    },
                    SuccessUrl = $"https://mintsports.net/Payment/Success?postId={postId}",
                    CancelUrl = $"https://mintsports.net/Posts/Subscriber/{post.PostId}",
                    Metadata = new Dictionary<string, string>
                    {
                        {"post_id", postId.ToString()},
                        {"user_ids", string.Join(",", userIds)}
                    }
                };

                var service = new SessionService();
                Session session = service.Create(options);
                _logger.LogInformation("Stripe session created successfully.");

                return Json(new { sessionId = session.Id, reward = post.Reward });
            }
            catch (StripeException stripeEx)
            {
                _logger.LogError(stripeEx, "Error occurred with Stripe.");
                return Json(new { error = stripeEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            var stripeEvent = EventUtility.ParseEvent(json);

            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Session;

                // Check if metadata exists and contains required keys
                if (session.Metadata == null || !session.Metadata.ContainsKey("user_ids") || !session.Metadata.ContainsKey("post_id"))
                {
                    _logger.LogWarning("Missing metadata in the Stripe webhook event.");
                    return BadRequest("Missing metadata.");
                }

                var userIds = session.Metadata["user_ids"].Split(',').ToList();
                var postId = int.Parse(session.Metadata["post_id"]);

                // New code: Create paymentRecord entries for each user based on the session's PaymentIntentId
                foreach (var userId in userIds)
                {
                    var paymentRecord = new PaymentRecord
                    {
                        PaymentIntentId = session.PaymentIntentId,
                        PostId = postId,
                        UserAccountId = userId
                    };
                    _context.Add(paymentRecord);
                }
                _logger.LogInformation("Payment records added to context.");

                await _context.SaveChangesAsync();
                _logger.LogInformation("Changes saved to database.");

                return await Adopt(userIds, postId);
            }

            return Ok();
        }




        [Authorize]
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
                .OrderByDescending(p => p.Created)
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

        [Authorize]
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
                .OrderByDescending(p => p.Created)
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
            int pageSize = 15;
            var onePageOfPosts = myPosts.ToPagedList(pageNumber, pageSize);

            // Calculate the total count of posts and total page count
            var totalCount = myPosts.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Pass the total count and total pages to the view
            ViewData["TotalCount"] = totalCount;
            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = pageNumber;

            return View(onePageOfPosts);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> CreateReply(int PostId, string Message)
        {

            if (string.IsNullOrWhiteSpace(Message))
            {
                TempData["ErrorMessage"] = "メッセージを入力してください。";
                return RedirectToAction(nameof(Details), new { id = PostId });
            }

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
                        var body = string.Format(Resources.Notification.NewCommentNotificationBody, usernameOrNickname, "応募","募集者", Message, PostId);

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
                    var body = string.Format(Resources.Notification.NewCommentNotificationBody, selectName,"募集","応募者", Message, PostId);

                    // Send a notification to the poster
                    await _emailSender.SendEmailAsync(posterEmail, subject, body);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = PostId });
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
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
