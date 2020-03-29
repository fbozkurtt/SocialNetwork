using SocialNetwork.Web.Service.Models.Entity;
using SocialNetwork.Web.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Security.Claims;
using System.Web.Http.Cors;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;

namespace SocialNetwork.Web.Service.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Authorize]
    public class DefaultController : ApiController
    {
        public static readonly SocialNetworkContext ctx = new SocialNetworkContext();
        private static readonly HttpClient Client = new HttpClient();

        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult Register([FromBody] User user)
        {
            try
            {
                user.DateCreated = DateTime.Now;
                ctx.Users.Add(user);
                ctx.SaveChanges();
                return Json(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
        [HttpGet]
        [Authorize(Roles = "user, admin, superadmin")]
        public IHttpActionResult GetProfile()
        {
            try
            {
                var user = GetCurrentUser();
                return Json(new
                {
                    success = true,
                    user
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
        [HttpGet]
        [Authorize(Roles = "user, admin, superadmin")]
        public IHttpActionResult GetAllUsers()
        {
            try
            {
                var users = ctx.Users.ToList();
                return Json(new
                {
                    users
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
        [HttpPost]
        [Authorize(Roles = "user, admin, superadmin")]
        public IHttpActionResult CreatePost([FromBody] Post post)
        {
            try
            {
                post.DateCreated = DateTime.Now;
                post.UserId = GetCurrentUser().Id;
                ctx.Posts.Add(post);
                ctx.SaveChanges();
                return Json(new
                {
                    success = true
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
        [HttpGet]
        [Authorize(Roles = "user, admin, superadmin")]
        public IHttpActionResult GetPosts()
        {
            try
            {
                var posts = ctx.Posts.ToList();
                ctx.SaveChanges();
                return Json(new
                {
                    success = true,
                    posts
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Login(string username, string password)
        {
            try
            {
                var uri = HttpContext.Current.Request.Url.AbsoluteUri.Replace("api/Login", "token");
                var values = new Dictionary<string, string>
                {
                    { "grant_type", "password" },
                    { "username", username },
                    { "password", password }
                };
                var content = new FormUrlEncodedContent(values);
                var response = await Client.PostAsync(uri, content);
                var responseString = await response.Content.ReadAsStringAsync();
                var resp = JObject.Parse(responseString);
                var token = resp.SelectToken("access_token");
                return Json(new
                {
                    success = token != null ? true : false,
                    token
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
        [NonAction]
        public User GetCurrentUser()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var claims = identity.Claims.ToList();
            var username = claims[1].Value;
            var user = ctx.Users.SingleOrDefault(w => w.Username == username);
            return user;
        }
    }
}
