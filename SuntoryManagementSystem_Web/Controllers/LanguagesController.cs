using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace SuntoryManagementSystem_Web.Controllers
{
    public class LanguagesController : Controller
    {
        /// <summary>
        /// Changes the application language and stores preference in cookie
        /// </summary>
        /// <param name="code">Language code (nl, en, fr)</param>
        /// <param name="returnUrl">URL to redirect back to</param>
        /// <returns>Redirect to the return URL</returns>
        public IActionResult ChangeLanguage(string code, string returnUrl)
        {
            // Sla taalvoorkeur op in cookie (geldig voor 1 maand)
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(code)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddMonths(1) }
            );

            // Redirect terug naar de pagina waar de gebruiker vandaan kwam
            return LocalRedirect(returnUrl);
        }
    }
}
