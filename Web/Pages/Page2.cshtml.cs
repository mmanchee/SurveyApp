using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SurveyApp.Models;
using SurveyApp.Web.Helpers; // For SessionExtensions
using System.Linq;

namespace SurveyApp.Web.Pages.Survey
{
    public class Page2Model : PageModel
    {
        private readonly ILogger<Page2Model> _logger;

        [BindProperty]
        public SurveyResponse SurveyResponse { get; set; } = new SurveyResponse();

        // For progress bar
        public int CurrentPage { get; set; } = 2;
        public int TotalPages { get; set; } = 1; // Default, will be updated based on session

        public Page2Model(ILogger<Page2Model> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            _logger.LogInformation("Page 2 (GET) accessed.");
            var sessionResponse = HttpContext.Session.GetObjectFromJson<SurveyResponse>("SurveyResponse");

            if (sessionResponse == null)
            {
                _logger.LogWarning("No survey data found in session on Page 2 GET. Redirecting to start.");
                return RedirectToPage("./Page1"); // Redirect to start if no data exists
            }

            SurveyResponse = sessionResponse; // Load existing data from session

            CalculateTotalPages(SurveyResponse.SurveyLengthChoice); // Calculate total pages based on existing choice

            return Page();
        }

        public IActionResult OnPost()
        {
            _logger.LogInformation("Page 2 (POST) submitted.");
            var sessionResponse = HttpContext.Session.GetObjectFromJson<SurveyResponse>("SurveyResponse");

            if (sessionResponse == null)
            {
                _logger.LogError("Page 2 POST called, but no survey data found in session. Redirecting to start.");
                return RedirectToPage("./Page1");
            }

            // Update only Page 2 field from the posted data
            // The model binder will bind SurveyResponse.SurveyLengthChoice from the form
            sessionResponse.SurveyLengthChoice = SurveyResponse.SurveyLengthChoice;

            // ** NEW: Remove validation errors for Page 1 fields on Page 2 POST **
            // These fields were required and validated on Page 1.
            // Remove their errors from ModelState so Page 2 validation only considers Page 2 fields.
            if (ModelState.ContainsKey("SurveyResponse.Name"))
            {
                ModelState.Remove("SurveyResponse.Name");
                _logger.LogDebug("Removed SurveyResponse.Name validation error for Page 2 POST.");
            }
            if (ModelState.ContainsKey("SurveyResponse.Industry"))
            {
                ModelState.Remove("SurveyResponse.Industry");
                _logger.LogDebug("Removed SurveyResponse.Industry validation error for Page 2 POST.");
            }
            if (ModelState.ContainsKey("SurveyResponse.Position"))
            {
                ModelState.Remove("SurveyResponse.Position");
                _logger.LogDebug("Removed SurveyResponse.Position validation error for Page 2 POST.");
            }
            // If you added IndustryOther and PositionOther and marked them as required,
            // you might need to remove them here too if they are only relevant for Page 1 validation.
            // Assuming they are not required or handled by CleanUpOtherFields later.


            CalculateTotalPages(sessionResponse.SurveyLengthChoice); // Recalculate based on the choice just made, needed for display if validation fails


            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Page 2 validation failed (after removing Page 1 fields errors).");
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
                _logger.LogInformation("Page 2 data saved to session. Survey length choice: {Choice}", sessionResponse.SurveyLengthChoice);

                // Determine next page based on the choice
                string nextPage = "./ThankYou"; // Default to ThankYou page if "EndHere" or no choice leads to further pages
                switch (sessionResponse.SurveyLengthChoice)
                {
                    case "OneMore":
                        nextPage = "./Page3";
                        break;
                    case "TwoMore":
                        nextPage = "./Page3"; // Start with Page 3
                        break;
                    case "Full":
                        nextPage = "./Page3"; // Start with Page 3
                        break;
                    case "EndHere":
                        nextPage = "./ThankYou";
                        break;
                    default:
                        _logger.LogWarning("Unexpected SurveyLengthChoice value: {Choice}", sessionResponse.SurveyLengthChoice);
                        nextPage = "./ThankYou"; // Fallback
                        break;
                }

                return RedirectToPage(nextPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Page 2 POST.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                SurveyResponse = sessionResponse; // Keep data for display after error
                return Page();
            }
        }

        // Handler for the Back button
        public IActionResult OnPostBack()
        {
            _logger.LogInformation("Page 2 (POST Back) triggered.");
            var sessionResponse = HttpContext.Session.GetObjectFromJson<SurveyResponse>("SurveyResponse");
            CalculateTotalPages(sessionResponse?.SurveyLengthChoice); // Needed for progress bar display
            return RedirectToPage("./Page1"); // Always go back to Page 1 from Page 2
        }

        private void CalculateTotalPages(string? surveyLengthChoice)
        {
            // Page 1 is always shown. Page 2 determines the rest.
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