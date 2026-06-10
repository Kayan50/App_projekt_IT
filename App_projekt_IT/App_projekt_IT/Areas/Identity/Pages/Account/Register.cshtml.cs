// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using App_projekt_IT.Data;
using App_projekt_IT.Models;
using App_projekt_IT.Services; 

namespace App_projekt_IT.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IEmailSender _emailSender;

    
    private readonly IEmailSenderQueue _emailQueue;

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender,
        IEmailSenderQueue emailQueue) 
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
        _emailQueue = emailQueue; 
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public string? ReturnUrl { get; set; }

    public IList<AuthenticationScheme>? ExternalLogins { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Imiê jest wymagane.")]
        [Display(Name = "Imiê")]
        public string FirstName { get; set; } = default!;

        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        [Display(Name = "Nazwisko")]
        public string LastName { get; set; } = default!;

        [Required(ErrorMessage = "PESEL jest wymagany.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "PESEL musi sk³adaæ siê z dok³adnie 11 cyfr.")]
        [Display(Name = "PESEL")]
        public string Pesel { get; set; } = default!;

        [Required(ErrorMessage = "Data urodzenia jest wymagana.")]
        [DataType(DataType.Date, ErrorMessage = "WprowadŸ prawid³ow¹ datê.")]
        [Display(Name = "Data urodzenia")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Email jest wymagany.")]
        [EmailAddress(ErrorMessage = "Nieprawid³owy format adresu email.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = default!;

        [Required(ErrorMessage = "Has³o jest wymagane.")]
        [StringLength(100, ErrorMessage = "{0} musi mieæ przynajmniej {2} i maksymalnie {1} znaków d³ugoœci.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Has³o")]
        public string Password { get; set; } = default!;

        [DataType(DataType.Password)]
        [Display(Name = "PotwierdŸ has³o")]
        [Compare("Password", ErrorMessage = "Has³a nie s¹ identyczne.")]
        public string? ConfirmPassword { get; set; }
    }

    public async Task OnGetAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (ModelState.IsValid)
        {
            var user = CreateUser();

            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.PESEL = Input.Pesel;
            user.DateOfBirth = Input.DateOfBirth;

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme)!;

                
                var emailMsg = new EmailMessage
                {
                    ToEmail = Input.Email,
                    Subject = "Witamy w Kliniki-Med - Potwierdzenie rejestracji",
                    Body = $@"
                        <div style='font-family: Arial, sans-serif; color: #333; max-width: 600px; margin: auto;'>
                            <h2 style='color: #2563eb;'>Witaj, {Input.FirstName}!</h2>
                            <p>Cieszymy siê, ¿e do³¹czy³eœ/aœ do grona pacjentów Kliniki-Med.</p>
                            <p>Aby w pe³ni korzystaæ z konta i móc umawiaæ wizyty, prosimy o potwierdzenie adresu e-mail, klikaj¹c w poni¿szy przycisk:</p>
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='background-color: #2563eb; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; font-weight: bold; display: inline-block;'>PotwierdŸ adres e-mail</a>
                            </div>
                            <p>Jeœli to nie Ty zak³ada³eœ/aœ konto, po prostu zignoruj tê wiadomoœæ.</p>
                            <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;' />
                            <p style='font-size: 0.8em; color: #94a2b8;'>Pozdrawiamy,<br/>Zespó³ Kliniki-Med</p>
                        </div>"
                };

                await _emailQueue.QueueEmailAsync(emailMsg);
                

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                }
                else
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }

    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<ApplicationUser>)_userStore;
    }
}