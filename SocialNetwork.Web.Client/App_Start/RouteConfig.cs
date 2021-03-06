﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SocialNetwork.Web.Client
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{action}/{id}",
                defaults: new { controller = "Default", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "Main",
                url: "",
                defaults: new { controller = "Default", Action = "Login", id = UrlParameter.Optional }
            );
        }
    }
}
