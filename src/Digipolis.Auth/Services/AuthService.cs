﻿using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Digipolis.Auth.Services
{
    public class AuthService : IAuthService
    {
        private IHttpContextAccessor _httpContextAccessor;
        private readonly ITokenRefreshAgent _tokenRefreshAgent;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly AuthOptions _authOptions;

        public AuthService(IHttpContextAccessor httpContextAccessor,
            ITokenRefreshAgent tokenRefreshAgent,
            IUrlHelperFactory urlHelperFactory,
            IOptions<AuthOptions> options)
        {
            if (httpContextAccessor == null) throw new ArgumentNullException($"{nameof(httpContextAccessor)} cannot be null.");
            if (tokenRefreshAgent == null) throw new ArgumentNullException($"{nameof(tokenRefreshAgent)} cannot be null.");
            if (urlHelperFactory == null) throw new ArgumentNullException($"{nameof(urlHelperFactory)} cannot be null.");

            _httpContextAccessor = httpContextAccessor;
            _tokenRefreshAgent = tokenRefreshAgent;
            _urlHelperFactory = urlHelperFactory;
            _authOptions = options.Value;
        }

        public ClaimsPrincipal User
        {
            get
            {
                return _httpContextAccessor.HttpContext.User;
            }
        }

        public string UserToken
        {
            get
            {
                if (_authOptions.JwtTokenSource == "session")
                {
                    return _httpContextAccessor.HttpContext.Session.GetString("auth-jwt");
                }
                else if (_authOptions.JwtTokenSource == "header")
                {
                    return _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString().Substring(7);
                }
                throw new FormatException("AuthOption JwtTokenSource not in correct format.");
            }
        }

        public async Task<string> LogOutAsync(ControllerContext controllerContext, string redirectController, string redirectAction)
        { 
            await _httpContextAccessor.HttpContext.Authentication.SignOutAsync(AuthSchemes.CookieAuth);

            var urlHelper = _urlHelperFactory.GetUrlHelper(controllerContext);
            var returnUrl = urlHelper.Action(redirectAction, redirectController, null, _httpContextAccessor.HttpContext.Request.Scheme);

            var redirectUrl = await _tokenRefreshAgent.LogoutTokenAsync(User.Identity.Name, returnUrl);

            return redirectUrl;
        }
    }
}
