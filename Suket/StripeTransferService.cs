using Amazon;
using Amazon.SecretsManager.Model;
using Amazon.SecretsManager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe;
using Suket.Data;
using Suket.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace Suket
{
    public class StripeTransferService : BackgroundService
    {
        //private readonly ApplicationDbContext _context;
        //private Timer _timer;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StripeTransferService> _logger;
        private readonly AWSSecretsManagerService _awsSecretsManagerService;

        public StripeTransferService(IServiceProvider serviceProvider, ILogger<StripeTransferService> logger, AWSSecretsManagerService awsSecretsManagerService)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _awsSecretsManagerService = awsSecretsManagerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        var stripeApiKey = await _awsSecretsManagerService.GetSecretAsync("MintSPORTS_secret");
                        StripeConfiguration.ApiKey = stripeApiKey;

                        var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);
                        
                        //test
                        //var ninetyDaysAgo = DateTime.UtcNow.AddMinutes(-10);

                        var transactionsToProcess = await context.TransactionRecord
                            .Where(tr => tr.TransactionDate <= ninetyDaysAgo
                                    && tr.Type == TransactionType.Receipt
                                    && !tr.IsTransferred) // まだ送金されていないトランザクションだけを取得
                            .Include(tr => tr.UserAccount)
                            .ToListAsync();

                        foreach (var transaction in transactionsToProcess)
                        {

                            

                            // 送金処理を試みる前に、StripeアカウントIDの有無を確認
                            if (transaction.UserAccount.DetailsSubmitted == false)
                            {   
                                // Stripeアカウントが未設定の場合、収益を没収
                                var userBalance = await context.UserBalance
                                    .FirstOrDefaultAsync(ub => ub.Id == transaction.UserAccountId);
                                if (userBalance != null)
                                {
                                    userBalance.Balance -= transaction.Amount;
                                    userBalance.LastUpdated = DateTimeOffset.UtcNow;
                                    context.Update(userBalance);
                                }

                                // 新しいTransactionRecordを作成して没収を記録
                                var lostTransaction = new TransactionRecord
                                {
                                    UserAccountId = transaction.UserAccountId,
                                    Type = TransactionType.Lost,
                                    Amount = transaction.Amount,
                                    PostId = transaction.PostId,
                                    TransactionDate = DateTimeOffset.UtcNow,
                                    IsTransferred = true
                                };

                                context.TransactionRecord.Add(lostTransaction);

                                // トランザクションのIsTransferredを更新
                                transaction.IsTransferred = true;
                                context.Update(transaction);

                                await context.SaveChangesAsync();

                                _logger.LogInformation($"Earnings forfeited for Transaction ID: {transaction.Id} due to no Stripe account.");
                            }
                            else
                            {

                                // 送金処理の実装
                                var transferOptions = new TransferCreateOptions
                                {
                                    Amount = (long)(transaction.Amount),
                                    Currency = "jpy",
                                    Destination = transaction.UserAccount.StripeAccountId,
                                    // TransferGroup, Descriptionなど、必要に応じて追加のオプションを設定
                                };

                                if (!string.IsNullOrWhiteSpace(transaction.PaymentIntentId))
                                {
                                    var paymentIntentService = new PaymentIntentService();
                                    PaymentIntent paymentIntent = paymentIntentService.Get(transaction.PaymentIntentId);
                                    string latestChargeId = paymentIntent.LatestChargeId;

                                    // SourceTransactionを設定
                                    transferOptions.SourceTransaction = latestChargeId;
                                }


                                try
                                {
                                    var transferService = new TransferService();
                                    Transfer transfer = transferService.Create(transferOptions);

                                    // 新しいTransactionRecordを作成して没収を記録
                                    var successTransaction = new TransactionRecord
                                    {
                                        UserAccountId = transaction.UserAccountId,
                                        Type = TransactionType.Transfer,
                                        Amount = transaction.Amount,
                                        PostId = transaction.PostId,
                                        TransactionDate = DateTimeOffset.UtcNow,
                                        IsTransferred = true
                                    };
                                    context.Update(successTransaction);

                                    // UserBalanceを取得し、残高を減算
                                    var userBalance = await context.UserBalance
                                        .FirstOrDefaultAsync(ub => ub.Id == transaction.UserAccountId);
                                    if (userBalance != null)
                                    {
                                        userBalance.Balance -= transaction.Amount;
                                        userBalance.LastUpdated = DateTimeOffset.UtcNow;
                                        context.Update(userBalance);
                                    }

                                    // トランザクションのIsTransferredを更新
                                    transaction.IsTransferred = true;
                                    context.Update(transaction);

                                    await context.SaveChangesAsync();

                                    _logger.LogInformation($"Transfer successful for Transaction ID: {transaction.Id}");
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"Error during transfer for Transaction ID: {transaction.Id}");
                                }
                            }
                        }

                        
                        // 次の実行まで一定時間待機（例えば、1日）
                        var now = DateTime.UtcNow;
                        var nextMidnight = now.Date.AddDays(1); // 次の日の0時（UTC時間）
                        var delay = nextMidnight - now;
                        // 次の日の0時まで待機
                        await Task.Delay(delay, stoppingToken);
                        

                        //test
                        // 次の実行まで10分待機
                        //await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing Stripe transfers.");
                }
            }
        }
    }
}
