namespace Suket.Models
{
    public class MyDashboardViewModel
    {
        public IEnumerable<Post> MyPosts { get; set; }
        public IEnumerable<Post> SubscribedPosts { get; set; }
        // その他のプロパティ...
    }
}
