﻿namespace Suket.Models
{
    public class PostIndexViewModel
    {
        public List<Post> Posts { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
