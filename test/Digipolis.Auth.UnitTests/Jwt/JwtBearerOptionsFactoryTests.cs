﻿using Digipolis.Auth.Jwt;
using Digipolis.Auth.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Digipolis.Auth.UnitTests.Jwt
{
    public class JwtBearerOptionsFactoryTests
    {
        IJwtSigningKeyResolver _signingKeyResolverMock = Mock.Of<IJwtSigningKeyResolver>();
        TestLogger<JwtBearerOptionsFactory> _testLogger = TestLogger<JwtBearerOptionsFactory>.CreateLogger();

        [Fact]
        public void ShouldSetTokenValidationParameters()
        {
            var authOptions = new AuthOptions
            {
               JwtIssuer = "jwtIssuer"
            };

            var tokenValidationParametersFactory = new Mock<ITokenValidationParametersFactory>();
            var tokenValidationParameters = new TokenValidationParameters();
            tokenValidationParametersFactory.Setup(f => f.Create())
                .Returns(tokenValidationParameters);

            var jwtBearerOptionsFactory = new JwtBearerOptionsFactory(tokenValidationParametersFactory.Object, _testLogger);

            var options = new JwtBearerOptions();
            jwtBearerOptionsFactory.Setup(options);

            tokenValidationParametersFactory.Verify(m => m.Create(), Times.Once);
            Assert.Same(tokenValidationParameters, options.TokenValidationParameters);
        }

        [Fact]
        public async Task LogWhenAuthenticationFailed()
        {
            var tokenValidationParametersFactory = new Mock<ITokenValidationParametersFactory>();
            var jwtBearerOptionsFactory = new JwtBearerOptionsFactory(tokenValidationParametersFactory.Object, _testLogger);

            var options = new JwtBearerOptions();
            jwtBearerOptionsFactory.Setup(options);

            var context = new AuthenticationFailedContext(null, new AuthenticationScheme("", "", null), options);
            context.Exception = new Exception("exceptiondetail");

            await options.Events.AuthenticationFailed(context);

            Assert.NotEmpty(_testLogger.LoggedMessages);
            Assert.Contains("exceptiondetail", _testLogger.LoggedMessages[0]);
        }

        //[Fact]
        //public async Task EmptyAuthenticationTicketIsSetWHenAuthenticationFailed()
        //{
        //    var tokenValidationParametersFactory = new Mock<ITokenValidationParametersFactory>();
        //    var jwtBearerOptionsFactory = new JwtBearerOptionsFactory(tokenValidationParametersFactory.Object, _testLogger);

        //    var options = new JwtBearerOptions();
        //    jwtBearerOptionsFactory.Setup(options);

        //    var context = new AuthenticationFailedContext(null, new AuthenticationScheme("", "", null), options);
        //    context.Exception = new Exception("exceptiondetail");

        //    await options.Events.AuthenticationFailed(context);

        //    Assert.True(context.HandledResponse);
        //    Assert.NotNull(context.Ticket);
        //}
    }
}   
