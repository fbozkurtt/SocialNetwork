﻿using Microsoft.Owin.Security.OAuth;
using SocialNetwork.Web.Service.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace SocialNetwork.Web.Service
{
    public class AuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            using (var ctx = new SocialNetworkContext())
            {
#if SECURE
                var user = ctx.Users.SingleOrDefault(w => w.Username == context.UserName && w.Password == context.Password);
#else
                //Broken Authentication
                var user = ctx.Users.FirstOrDefault(w => w.Password == context.Password);
#endif
                if (user == null)
                {
                    context.SetError("invalid_grant", "Provided username and/or password is incorrect");
                    return;
                }
                var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));
                identity.AddClaim(new Claim(ClaimTypes.Name, user.Username));
                context.Validated(identity);
            }
        }
    }

}