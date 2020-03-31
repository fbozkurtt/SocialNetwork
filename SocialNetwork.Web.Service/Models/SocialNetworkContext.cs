using SocialNetwork.Web.Service.Models.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SocialNetwork.Web.Service.Models
{
    public class SocialNetworkContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public SocialNetworkContext() : base("sclcnn")
        {

        }
    }
}