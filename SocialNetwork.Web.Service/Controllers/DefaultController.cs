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
#if SECURE
                    user.Role = "user";
#endif
                    if(ctx.Users.Where(w=>w.Email==user.Email).SingleOrDefault()!=null)
                        return Json(new
                        {
                            success = false,
                            error = "E-mail is in use"
                        });
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
#if SECURE
                user.Password = null;
#endif
                return Json(new
                {
                    success = true,
                    user,
                    follows = GetFollows(),
                    followers = GetFollowers()
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
                using (var ctx = new SocialNetworkContext())
                {
                    var users = ctx.Users.ToList();
#if SECURE
                    foreach(var u in users)
                    {
                        u.Password = null;
                    }
#endif
                    //Sensitive Data Exposure
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
                            User = user.Username,
                            Media = p.Media,
                            Id=p.Id
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
                var expires = resp.SelectToken("expires_in");
                var error = resp.SelectToken("error_description");
                return Json(new
                {
                    success = token != null ? true : false,
                    token,
                    expires,
                    error
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
                    ctx.Users.RemoveRange(ctx.Users.Where(w => w.Role.Equals("user")));
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
        [HttpGet]
        [Authorize(Roles = "user, admin, superadmin")]
        public IHttpActionResult Follow(int id)
        {
            try
            {
                var user = GetCurrentUser();
                using (var ctx = new SocialNetworkContext())
                {
                    if (id == user.Id)
                        throw new Exception("You can't follow yourself");
                    var f = ctx.Follows.Where(w => w.UserId == id && w.FollowerId == user.Id).SingleOrDefault();
                    if (f != null)
                    {
                        ctx.Follows.Remove(f);
                    }
                    else
                        ctx.Follows.Add(new Follow()
                        {
                            FollowerId = user.Id,
                            UserId = id
                        });
                    ctx.SaveChanges();
                    return Json(new
                    {
                        success = true,
                        message = f == null ? "Followed" : "Unfollowed"
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
#if SECURE
        [Authorize(Roles = "admin, superadmin")]
#else
        //Broken Access Control
        [Authorize(Roles = "user, admin, superadmin")]
#endif
        public IHttpActionResult DeletePost(int id)
        {
            try
            {
                using (var ctx = new SocialNetworkContext())
                {
                    var post = ctx.Posts.Where(w => w.Id == id).SingleOrDefault();
                    if (post != null)
                    {
                        ctx.Posts.Remove(post);
                    }
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
        [NonAction]
        public List<Int32> GetFollowers()
        {
            var user = GetCurrentUser();
            using (var ctx = new SocialNetworkContext())
            {
                var followers = ctx.Follows.Where(w => w.UserId == user.Id).Select(w => w.FollowerId).ToList();
                return ctx.Users.Where(w => followers.Contains(w.Id)).Select(w => w.Id).ToList();
            }
        }
        [NonAction]
        public List<Int32> GetFollows()
        {
            var user = GetCurrentUser();
            using (var ctx = new SocialNetworkContext())
            {
                var follows = ctx.Follows.Where(w => w.FollowerId == user.Id).Select(w => w.UserId).ToList();
                return ctx.Users.Where(w => follows.Contains(w.Id)).Select(w => w.Id).ToList();
            }
        }
    }
}
