using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SocialNetwork.Web.Service.ViewModels
{
    public class PostViewModel
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string User { get; set; }
        public string Date { get; set; }
        public string Media { get; set; }
    }
}