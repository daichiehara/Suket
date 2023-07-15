using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Suket.Data;
using Suket.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Suket
{
    public class UpdatePostStatusBackgroundService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;

        public UpdatePostStatusBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Run the UpdatePostStatus method every 60 seconds
            _timer = new Timer(UpdatePostStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(60));
            return Task.CompletedTask;
        }

        private void UpdatePostStatus(object state)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var expiredPosts = context.Post.Where(p => p.Time < DateTimeOffset.UtcNow && p.State == State.Recruiting).ToList();

                foreach (var post in expiredPosts)
                {
                    post.State = State.End;
                }

                context.SaveChanges();
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
