﻿using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc.Controllers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Toolbox.Auth.Authorization;
using Xunit;

namespace Toolbox.Auth.UnitTests.Authorization.ResolverTests
{
    public class CustomBasedTests
    {
        private readonly string _userId = "user123";

        [Fact]
        public void ResolveSingleCustomPermission()
        {
            var resolver = new RequiredPermissionsResolver();
            var context = CreateAuthorizationContext(typeof(CustomBasedResourceController), "GetSingle");

            var requiredPermissions = resolver.ResolveFromAttributeProperties(context);

            Assert.Equal(2, requiredPermissions.Count());
            Assert.Contains("controllerpermission", requiredPermissions);
            Assert.Contains("custompermission", requiredPermissions);
        }

        [Fact]
        public void ResolveMultipleCustomPermission()
        {
            var resolver = new RequiredPermissionsResolver();
            var context = CreateAuthorizationContext(typeof(CustomBasedResourceController), "GetMultiple");

            var requiredPermissions = resolver.ResolveFromAttributeProperties(context);

            Assert.Equal(3, requiredPermissions.Count());
            Assert.Contains("controllerpermission", requiredPermissions);
            Assert.Contains("permission1", requiredPermissions);
            Assert.Contains("permission2", requiredPermissions);
        }

        [Fact]
        public void ResolveControllerPermission()
        {
            var resolver = new RequiredPermissionsResolver();
            var context = CreateAuthorizationContext(typeof(CustomBasedResourceController), "Get");

            var requiredPermissions = resolver.ResolveFromAttributeProperties(context);

            Assert.Equal(1, requiredPermissions.Count());
            Assert.Contains("controllerpermission", requiredPermissions);
        }

        private AuthorizationContext CreateAuthorizationContext(Type controllerType, string action)
        {
            var actionContext = new Microsoft.AspNet.Mvc.ActionContext();

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.Request)
                .Returns(Mock.Of<HttpRequest>());

            actionContext.HttpContext = mockHttpContext.Object;
            actionContext.RouteData = new Microsoft.AspNet.Routing.RouteData();

            var actionDescriptor = new ControllerActionDescriptor
            {
                ControllerTypeInfo = controllerType.GetTypeInfo(),
                MethodInfo = controllerType.GetMethod(action)
            };
            actionContext.ActionDescriptor = actionDescriptor;

            var resource = new Microsoft.AspNet.Mvc.Filters.AuthorizationContext(actionContext, new List<Microsoft.AspNet.Mvc.Filters.IFilterMetadata>());

            var requirements = new IAuthorizationRequirement[] { new ConventionBasedRequirement() };

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, _userId),
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
            var context = new AuthorizationContext(requirements, user, resource);
            return context;
        }
    }
}