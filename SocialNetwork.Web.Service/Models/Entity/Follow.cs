using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SocialNetwork.Web.Service.Models.Entity
{
    public class Follow
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FollowerId { get; set; }
    }
}