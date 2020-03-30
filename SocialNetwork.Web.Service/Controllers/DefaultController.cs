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
using SocialNetwork.Web.Service.ViewModels;

namespace SocialNetwork.Web.Service.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [Authorize]
    public class DefaultController : ApiController
    {
        private static readonly HttpClient Client = new HttpClient();

        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult Register([FromBody] User user)
        {
            if (user.Email == null || user.Name == null || user.Lastname == null || user.Password == null)
            {
                return Json(new
                {
                    success = false,
                    error = "Please fill al the fields"
                });
            }
            try
            {
                using (var ctx = new SocialNetworkContext())
                {
                    user.DateCreated = DateTime.Now;
                    ctx.Users.Add(user);
                    ctx.SaveChanges();
                    return Json(new
                    {
                        success = true
                    });
                }
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
        [Authorize(Roles = "admin, superadmin")]
        public IHttpActionResult GetAllUsers()
        {
            try
            {
                using (var ctx = new SocialNetworkContext())
                {
                    var users = ctx.Users.ToList();
                    return Json(new
                    {
                        success = true,
                        users
                    });
                }
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
                using (var ctx = new SocialNetworkContext())
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
                using (var ctx = new SocialNetworkContext())
                {
                    var posts = ctx.Posts.OrderByDescending(w => w.DateCreated).ToList();
                    List<PostViewModel> model = new List<PostViewModel>();
                    foreach (var p in posts)
                    {
                        var user = ctx.Users.Where(w => w.Id == p.UserId).FirstOrDefault();
                        model.Add(new PostViewModel()
                        {
                            Body = p.Body,
                            Title = p.Title,
                            Date = p.DateCreated.ToString("HH:mm:ss dddd MMMM yyyy"),
                            User = user.Username
                        });
                    }
                    ctx.SaveChanges();
                    return Json(new
                    {
                        success = true,
                        posts = model
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex
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
        [HttpGet]
        [Authorize(Roles = "superadmin")]
        public IHttpActionResult DeleteEverything()
        {
            try
            {
                using (var ctx = new SocialNetworkContext())
                {
                    ctx.Users.RemoveRange(ctx.Users.Where(w=>w.Role.Equals("user")));
                    ctx.Posts.RemoveRange(ctx.Posts);
                    ctx.SaveChanges();
                    return Json(new
                    {
                        success = true
                    });
                }
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
            using (var ctx = new SocialNetworkContext())
            {
                var identity = (ClaimsIdentity)User.Identity;
                var claims = identity.Claims.ToList();
                var username = claims[1].Value;
                var user = ctx.Users.SingleOrDefault(w => w.Username == username);
                return user;
            }
        }
    }
}
