using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SurveyApp.Models;
using SurveyApp.Web.Helpers; // For SessionExtensions
using System.Linq; // Needed for Any()

namespace SurveyApp.Web.Pages.Survey
{
    public class Page4Model : PageModel
    {
        private readonly ILogger<Page4Model> _logger;

        [BindProperty]
        public SurveyResponse SurveyResponse { get; set; } = new SurveyResponse();

        // For progress bar
        public int CurrentPage { get; set; } = 4;
        public int TotalPages { get; set; } = 1; // Default, will be updated based on session

        // Options for Likert Scale
        public Dictionary<int, string> LikertScaleOptions => SurveyOptions.LikertScale;

        public Page4Model(ILogger<Page4Model> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            _logger.LogInformation("Page 4 (GET) accessed.");
            var sessionResponse = HttpContext.Session.GetObjectFromJson<SurveyResponse>("SurveyResponse");

            if (sessionResponse == null)
            {
                _logger.LogWarning("No survey data found in session on Page 4 GET. Redirecting to start.");
                return RedirectToPage("./Page1"); // Redirect to start if no data exists
            }

            // Check if user should be on this page based on SurveyLengthChoice
            if (sessionResponse.SurveyLengthChoice != "TwoMore" && sessionResponse.SurveyLengthChoice != "Full")
            {
                _logger.LogInformation("User did not choose 'TwoMore' or 'Full' on Page 2. Redirecting from Page 4 GET to Thank You page.");
                // Redirect to the page they should go to after Page 3, which is ThankYou
                return RedirectToPage("./ThankYou");
            }


            SurveyResponse = sessionResponse; // Load existing data from session

            // No complex options to re-populate on this page

            CalculateTotalPages(SurveyResponse.SurveyLengthChoice); // Calculate total pages

            return Page();
        }

        public IActionResult OnPost()
        {
            _logger.LogInformation("Page 4 (POST) submitted.");
            var sessionResponse = HttpContext.Session.GetObjectFromJson<SurveyResponse>("SurveyResponse");

            if (sessionResponse == null)
            {
                _logger.LogError("Page 4 POST called, but no survey data found in session. Redirecting to start.");
                return RedirectToPage("./Page1");
            }

            // Remove validation errors for fields from previous pages (Page 1, 2, and 3)
            if (ModelState.ContainsKey("SurveyResponse.Name")) ModelState.Remove("SurveyResponse.Name");
            if (ModelState.ContainsKey("SurveyResponse.Age")) ModelState.Remove("SurveyResponse.Age");
            if (ModelState.ContainsKey("SurveyResponse.Industry")) ModelState.Remove("SurveyResponse.Industry");
            if (ModelState.ContainsKey("SurveyResponse.IndustryOther")) ModelState.Remove("SurveyResponse.IndustryOther");
            if (ModelState.ContainsKey("SurveyResponse.Position")) ModelState.Remove("SurveyResponse.Position");
            if (ModelState.ContainsKey("SurveyResponse.PositionOther")) ModelState.Remove("SurveyResponse.PositionOther");
            if (ModelState.ContainsKey("SurveyResponse.SurveyLengthChoice")) ModelState.Remove("SurveyResponse.SurveyLengthChoice");
            if (ModelState.ContainsKey("SurveyResponse.Likes")) ModelState.Remove("SurveyResponse.Likes");
            if (ModelState.ContainsKey("SurveyResponse.FavoriteColor")) ModelState.Remove("SurveyResponse.FavoriteColor");
            if (ModelState.ContainsKey("SurveyResponse.FavoriteColorOther")) ModelState.Remove("SurveyResponse.FavoriteColorOther");
            if (ModelState.ContainsKey("SurveyResponse.FavoriteMusicGenres")) ModelState.Remove("SurveyResponse.FavoriteMusicGenres");
            if (ModelState.ContainsKey("SurveyResponse.FavoriteMusicGenresOther")) ModelState.Remove("SurveyResponse.FavoriteMusicGenresOther");
            if (ModelState.ContainsKey("SurveyResponse.FavoriteSeason")) ModelState.Remove("SurveyResponse.FavoriteSeason");
            if (ModelState.ContainsKey("SurveyResponse.FavoriteSeasonOther")) ModelState.Remove("SurveyResponse.FavoriteSeasonOther");
            if (ModelState.ContainsKey("SurveyResponse.FavoritePlaceWhy")) ModelState.Remove("SurveyResponse.FavoritePlaceWhy");


            // Update Page 4 fields - Model Binder will handle the integer values
            // Ensure other fields from previous pages are preserved
            sessionResponse.LikertJob = SurveyResponse.LikertJob;
            sessionResponse.LikertHome = SurveyResponse.LikertHome;
            sessionResponse.LikertFamily = SurveyResponse.LikertFamily;
            sessionResponse.LikertAnimals = SurveyResponse.LikertAnimals;
            sessionResponse.LikertFriends = SurveyResponse.LikertFriends;


            CalculateTotalPages(sessionResponse.SurveyLengthChoice); // Needed for display if validation fails

            // No complex options to re-populate in case of validation error

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Page 4 validation failed.");
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Any())
                    {
                        _logger.LogWarning("Validation Error - Key: {Key}, Errors: {Errors}", state.Key, string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage)));
                    }
                }
                SurveyResponse = sessionResponse; // Ensure bound property has updated value for displaying errors
                return Page(); // Stay on the same page
            }

            try
            {
                // Save updated data back to session
                HttpContext.Session.SetObjectAsJson("SurveyResponse", sessionResponse);
                _logger.LogInformation("Page 4 data saved to session.");

                // Determine next page based on the choice made on Page 2
                string nextPage = "./ThankYou"; // Default
                switch (sessionResponse.SurveyLengthChoice)
                {
                    case "OneMore": // Should not happen if OnGet check works
                    case "TwoMore":
                        nextPage = "./ThankYou"; // Page 4 is the last content page for "TwoMore"
                        break;
                    case "Full":
                        nextPage = "./Page5"; // Go to Page 5
                        break;
                    case "EndHere": // Should not happen if OnGet check works
                        nextPage = "./ThankYou";
                        break;
                    default:
                        _logger.LogWarning("Unexpected SurveyLengthChoice value after Page 4 submit: {Choice}", sessionResponse.SurveyLengthChoice);
                        nextPage = "./ThankYou"; // Fallback
                        break;
                }

                return RedirectToPage(nextPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Page 4 POST.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                SurveyResponse = sessionResponse; // Keep data for display after error
                return Page();
            }
        }

        // Handler for the Back button
        public IActionResult OnPostBack()
        {
            _logger.LogInformation("Page 4 (POST Back) triggered.");
            var sessionResponse = HttpContext.Session.GetObjectFromJson<SurveyResponse>("SurveyResponse");
            CalculateTotalPages(sessionResponse?.SurveyLengthChoice); // Needed for progress bar display
            return RedirectToPage("./Page3"); // Always go back to Page 3 from Page 4
        }

        private void CalculateTotalPages(string? surveyLengthChoice)
        {
            // Page 1 is always shown.
            // Page 2 determines the rest.
            // Total Pages = 1 (Page1) + 1 (Page2) + ? (determined by choice) + 1 (ThankYou)
            TotalPages = 2; // Start with Page 1 & 2
            if (!string.IsNullOrEmpty(surveyLengthChoice))
            {
                switch (surveyLengthChoice)
                {
                    case "OneMore": TotalPages += 1; break; // Page 3
                    case "TwoMore": TotalPages += 2; break; // Page 3, 4
                    case "Full": TotalPages += 3; break;    // Page 3, 4, 5
                    case "EndHere": break; // Only Page 1, 2
                }
            }
            else
            {
                // If choice not made yet, assume minimum until choice is made on Page 2.
                TotalPages = 2; // Default to Page 1 & 2 until choice made
            }
            TotalPages += 1; // Add the Thank You page
            _logger.LogDebug("Calculated TotalPages: {TotalPages} based on choice '{Choice}'", TotalPages, surveyLengthChoice ?? "N/A");
        }
    }
}