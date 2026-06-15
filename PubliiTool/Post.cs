using Dapper.Contrib.Extensions;
using System.ComponentModel.DataAnnotations;

namespace PubliiTool
{
    public class Post
    {
        [ExplicitKey]
        public int Id { get; set; }
        public string Slug;
        public string Text { get; set; }
        public string Title;
        public Post Parent;
        public bool Synced = true;
    }
}
