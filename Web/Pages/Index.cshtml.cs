using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SurveyApp.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // You can add logic here for when the home page is accessed.
            // For example, checking if a survey is in progress in the session.
            _logger.LogInformation("Home page (Index) accessed.");
        }
    }
}