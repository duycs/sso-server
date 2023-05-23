using AuthServer.Infrastructure.Data.Identity;
using Microsoft.AspNetCore.Mvc;
using AuthServer.Models;
using AuthServer.Infrastructure.Constants;
using System.Threading.Tasks;
using IdentityServer4.Services;
using System.Linq;
using System;
using IdentityServer4.Stores;
using IdentityServer4.Models;
using IdentityServer4.Events;
using AuthServer.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using AuthServer.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthServer.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly IClientStore _clientStore;
        private readonly IEventService _events;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IStaffClientService _staffClientService;
        private readonly IConfiguration _configuration;
        private static ConsentResponse Denied = new ConsentResponse();

        public AccountController(ILogger<AccountController> logger, UserManager<AppUser> userManager, IIdentityServerInteractionService interaction, SignInManager<AppUser> signInManager,
            IAuthenticationSchemeProvider schemeProvider, IClientStore clientStore, IEventService events,
            IConfiguration configuration,
            IStaffClientService staffClientService)
        {
            _logger = logger;
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
            _interaction = interaction;
            _schemeProvider = schemeProvider;
            _clientStore = clientStore;
            _events = events;
            _staffClientService = staffClientService;
        }

        /// <summary>
        /// Entry point into the login workflow
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            // build a model so we know what to show on the login page
            var vm = await BuildLoginViewModelAsync(returnUrl);

            if (vm.IsExternalLoginOnly)
            {
                // we only have one option for logging in and it's an external provider
                return RedirectToAction("Challenge", "External", new { provider = vm.ExternalLoginScheme, returnUrl });
            }

            return View(vm);
        }


        /// <summary>
        /// Handle postback from username/password login
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginInputModel model, string button)
        {
            try
            {
                // check if we are in the context of an authorization request
                var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

                // the user clicked the "cancel" button
                if (button != "login")
                {
                    if (context != null)
                    {
                        // if the user cancels, send a result back into IdentityServer as if they 
                        // denied the consent (even if this client does not require consent).
                        // this will send back an access denied OIDC error response to the client.
                        await _interaction.GrantConsentAsync(context, Denied);

                        // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                        if (await _clientStore.IsPkceClientAsync(context?.Client.ClientId))
                        {
                            // if the client is PKCE then we assume it's native, so this change in how to
                            // return the response is for better UX for the end user.
                            return View(model.ReturnUrl);
                        }

                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        // since we don't have a valid context, then we just go back to the home page
                        return Redirect("~/");
                    }
                }

                if (ModelState.IsValid)
                {
                    // validate username/password
                    var user = await _userManager.FindByNameAsync(model.Username);
                    if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
                    {
                        await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.Name));

                        await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberLogin, false);

                        // only set explicit expiration here if user chooses "remember me". 
                        // otherwise we rely upon expiration configured in cookie middleware.
                        AuthenticationProperties props = null;
                        if (AccountOptions.AllowRememberLogin && model.RememberLogin)
                        {
                            props = new AuthenticationProperties
                            {
                                IsPersistent = true,
                                ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
                            };
                        };

                        //await HttpContext.SignInAsync(user.Id, user.UserName, props);

                        if (context != null)
                        {
                            if (await _clientStore.IsPkceClientAsync(context?.Client.ClientId))
                            {
                                // if the client is PKCE then we assume it's native, so this change in how to
                                // return the response is for better UX for the end user.
                                return View(model.ReturnUrl);
                            }

                            // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null

                            return Redirect(model.ReturnUrl);
                        }

                        // request for a local page
                        if (Url.IsLocalUrl(model.ReturnUrl))
                        {
                            return Redirect(model.ReturnUrl);
                        }
                        else if (string.IsNullOrEmpty(model.ReturnUrl))
                        {
                            return Redirect("~/");
                        }
                        else
                        {
                            // user might have clicked on a malicious link - should be logged
                            throw new Exception("invalid return URL");
                        }
                    }

                    await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials"));
                    ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);
                }

                // something went wrong, show form with error
                var vm = await BuildLoginViewModelAsync(model);

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError("Login Error", ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            await _signInManager.SignOutAsync();
            var context = await _interaction.GetLogoutContextAsync(logoutId);
            return Redirect(context.PostLogoutRedirectUri);
        }


        [HttpPost]
        [Route("api/[controller]")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // username is email name
            var username = model.Email.Split('@')[0];

            var user = new AppUser { UserName = username, Name = model.Name, Email = model.Email };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);

            try
            {
                var userExisting = await _userManager.FindByNameAsync(user.UserName);

                if (userExisting == null) throw new Exception("Register error");

                await _userManager.AddClaimAsync(user, new Claim("id", userExisting.Id));
                await _userManager.AddClaimAsync(user, new Claim("userName", user.UserName));
                await _userManager.AddClaimAsync(user, new Claim("account", user.UserName));
                await _userManager.AddClaimAsync(user, new Claim("name", user.Name));
                await _userManager.AddClaimAsync(user, new Claim("email", user.Email));
                await _userManager.AddClaimAsync(user, new Claim("role", Roles.Admin));

                // sync create new staff
                var createStaffVM = new CreateStaffVM { UserId = user.Id, FullName = user.Name, Account = user.UserName, Email = user.Email };

                //TODO:
                //string token = await _userManager.GenerateUserTokenAsync(user, "CreateStaff", "CreateStaff");
                //bool validatedToken = await _userManager.VerifyUserTokenAsync(user, "CreateStaff", "CreateStaff", token);
                var token = GetToken(userExisting, Config.GetClients(_configuration).First());

                try
                {
                    await _staffClientService.Create(createStaffVM, token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    _logger.LogInformation($"Error when created staff, remove staff created: {userExisting.Name}");

                    await _userManager.DeleteAsync(userExisting);

                    throw;
                }
            }
            catch (Exception ex)
            {
                // remove staff if have error
                return StatusCode(500, ex.Message);
            }

            return Ok(new RegisterResponseViewModel(user));
        }

        [HttpPost]
        [Route("api/[controller]/roles")]
        public async Task<IActionResult> UpdateRoles([FromBody] AddRolesViewModel model)
        {
            var userExisting = await _userManager.FindByNameAsync(model.UserName);

            if (userExisting is null)
            {
                return BadRequest("User can not be empty");
            }

            // remove all roles
            var roles = await _userManager.GetRolesAsync(userExisting);
            if (roles != null && roles.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(userExisting, roles);
            }

            // update new roles
            foreach (var role in model.Roles)
            {
                var isInRole = await _userManager.IsInRoleAsync(userExisting, role);
                if (!isInRole)
                {
                    await _userManager.AddToRoleAsync(userExisting, role);
                }
            }

            _logger.LogDebug($"Added roles for user {userExisting.UserName}");
            return Ok();
        }


        private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
        {
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context?.IdP != null)
            {
                var local = context.IdP == IdentityServer4.IdentityServerConstants.LocalIdentityProvider;

                // this is meant to short circuit the UI and only trigger the one external IdP
                var vm = new LoginViewModel
                {
                    EnableLocalLogin = local,
                    ReturnUrl = returnUrl,
                    Username = context?.LoginHint,
                };

                if (!local)
                {
                    vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context.IdP } };
                }

                return vm;
            }

            var schemes = await _schemeProvider.GetAllSchemesAsync();

            var providers = schemes.Where(x => x.DisplayName != null || (x.Name.Equals(AccountOptions.WindowsAuthenticationSchemeName, StringComparison.OrdinalIgnoreCase))
                )
                .Select(x => new ExternalProvider
                {
                    DisplayName = x.DisplayName,
                    AuthenticationScheme = x.Name
                }).ToList();

            var allowLocal = true;
            if (context?.Client.ClientId != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(context?.Client.ClientId);
                if (client != null)
                {
                    allowLocal = client.EnableLocalLogin;

                    if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                    {
                        providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                    }
                }
            }

            return new LoginViewModel
            {
                AllowRememberLogin = AccountOptions.AllowRememberLogin,
                EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
                ReturnUrl = returnUrl,
                Username = context?.LoginHint,
                ExternalProviders = providers.ToArray()
            };
        }

        private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
        {
            var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
            vm.Username = model.Username;
            vm.RememberLogin = model.RememberLogin;
            return vm;
        }

        private String GetToken(AppUser user, Client client)
        {
            var utcNow = DateTime.UtcNow;

            var claims = new Claim[]
            {
                new Claim("id", user.Id),
                new Claim("userName", user.UserName),
                new Claim("account", user.UserName),
                new Claim("name", user.Name),
                new Claim("email", user.Email),
                new Claim("role", Roles.Staff)
            };

            var jwt = new JwtSecurityToken(
                claims: claims,
                expires: utcNow.AddSeconds(client.AccessTokenLifetime)
                );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
    }
}
