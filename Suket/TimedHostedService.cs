using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Azure.Security.KeyVault.Secrets;
using Stripe;
using Suket.Controllers;
using Suket.Data;
using Suket.Models;

namespace Suket
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PostsController> _logger;

        public TimedHostedService(IServiceScopeFactory scopeFactory, ILogger<PostsController> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        private static string GetStripeAPIKeyFromAzureKeyVault()
        {
            var keyVaultUrl = "https://stripetestapikey.vault.azure.net/";
            var secretName = "StripeTestAPIKey";

            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
            KeyVaultSecret stripeAPIKeySecret = client.GetSecret(secretName);

            return stripeAPIKeySecret.Value;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // 現在の日時を取得
            var now = DateTime.UtcNow;

            /* 元
            // 次のam12:00の時間を取得
            var nextMidnight = now.Date.AddHours(24);

            // 現在から次のam12:00までの残り時間を計算
            var delay = nextMidnight - now;

            // タイマーを設定して、残りの時間だけ遅延した後、毎日am12:00に実行されるようにする
            _timer = new Timer(DoWork, null, delay, TimeSpan.FromHours(24));
            */

            // 次の15分単位の時刻を取得
            var nextQuarterHour = now.AddMinutes(15 - (now.Minute % 15));

            // 現在から次の15分単位の時刻までの残り時間を計算
            var delay = nextQuarterHour - now;

            // タイマーを設定して、残りの時間だけ遅延した後、毎15分に実行されるようにする
            _timer = new Timer(DoWork, null, delay, TimeSpan.FromMinutes(15));

            return Task.CompletedTask;
        }


        private void DoWork(object state)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            StripeConfiguration.ApiKey = GetStripeAPIKeyFromAzureKeyVault();

            // 直近24時間の開始と終了時刻を計算
            //var endTime = DateTime.UtcNow.Date; // 今日のam12:00
            //var startTime = endTime.AddDays(-1); // 昨日のam12:00
            var currentMoment = DateTime.UtcNow; // 現在の時間
            var startTime = currentMoment.AddMinutes(-15); // 15分前


            // 上記の時間帯で開催された投稿を取得
            var postsToProcess = context.Post
                                        .Where(p => p.Time >= startTime && p.Time < currentMoment)
                                        .ToList();

            foreach (var post in postsToProcess)
            {
                try
                {
                    // 既存の処理...
                    // RollCallテーブルから参加者情報を取得
                    var participants = context.RollCall
                                                .Where(rc => rc.PostId == post.PostId)
                                                .Include(rc => rc.UserAccount)  // UserAccountエンティティをロード
                                                .ToList();

                    // PaymentRecordテーブルからPaymentIntentIdを取得
                    var paymentRecords = context.PaymentRecord
                                                    .Where(pr => pr.PostId == post.PostId)
                                                    .ToList();

                    foreach (var record in paymentRecords)
                    {
                        try
                        {
                            var participant = participants.FirstOrDefault(p => p.UserAccountId == record.UserAccountId);
                            var paymentIntentService = new PaymentIntentService();
                            PaymentIntent paymentIntent = paymentIntentService.Get(record.PaymentIntentId);
                            string latestChargeId = paymentIntent.LatestChargeId;

                            if (participant != null)
                            {
                                // 参加者がRollCallに存在する場合、送金処理を行う
                                try
                                {
                                    // 送金処理...
                                    var destinationAccountId = participant.UserAccount.StripeAccountId; // 連結アカウントIDを適切に取得

                                    // まず、PaymentIntentの金額から手数料を差し引きます
                                    var totalAmountAfterFee = paymentIntent.Amount * 0.90m; // 90%を取得

                                    // 次に、同じPaymentIntentIdのレコードの数を取得
                                    var samePaymentIntentRecordCount = paymentRecords.Count(pr => pr.PaymentIntentId == record.PaymentIntentId);

                                    // それから、残った金額をレコード数で割ります
                                    var amountPerRecord = totalAmountAfterFee / samePaymentIntentRecordCount;

                                    // 金額を小数点以下を切り捨てた整数値に変換
                                    var amountPerRecordFloored = Math.Floor(amountPerRecord);

                                    var options = new TransferCreateOptions
                                    {
                                        Amount = (long)amountPerRecordFloored,
                                        Currency = "jpy",
                                        SourceTransaction = latestChargeId, // 事前に保存されたChargeIdを使用
                                        Destination = destinationAccountId
                                    };

                                    var service = new TransferService();
                                    var transfer = service.Create(options);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Error transferring to participant {participant.RollCallId}: {ex.Message}");
                                }
                            }
                            else
                            {
                                // 参加者がRollCallに存在しない場合、返金処理を行う
                                try
                                {
                                    // まず、PaymentIntentの金額から手数料を差し引きます
                                    var totalAmountAfterFee = paymentIntent.Amount;

                                    // 次に、同じPaymentIntentIdのレコードの数を取得
                                    var samePaymentIntentRecordCount = paymentRecords.Count(pr => pr.PaymentIntentId == record.PaymentIntentId);

                                    // それから、残った金額をレコード数で割ります
                                    var amountPerRecord = totalAmountAfterFee / samePaymentIntentRecordCount;

                                    var refundService = new RefundService();
                                    var refundOptions = new RefundCreateOptions
                                    {
                                        PaymentIntent = record.PaymentIntentId,
                                        Amount = (long)amountPerRecord,
                                        // 追加のオプションが必要な場合はここで設定します
                                    };
                                    var refund = refundService.Create(refundOptions);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Error refunding payment record {record.Id}: {ex.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error processing payment record {record.Id}: {ex.Message}");
                        }
                    }

                }
                catch (Exception ex)
                {
                    // Postの処理中のエラーハンドリング
                    _logger.LogError($"Error processing post {post.PostId}: {ex.Message}");
                }
            }
                
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
