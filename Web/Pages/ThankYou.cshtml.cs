using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SurveyApp.BLL; // Use BLL service
using SurveyApp.Models;
using SurveyApp.Web.Helpers; // For SessionExtensions
using System;
using System.Threading.Tasks;

namespace SurveyApp.Web.Pages.Survey
{
    public class ThankYouModel : PageModel
    {
        private readonly ILogger<ThankYouModel> _logger;
        private readonly ISurveyService _surveyService; // Inject BLL service

        public SurveyResponse? SurveySummary { get; private set; } // To display summary if needed
        public bool SubmissionSuccessful { get; private set; } = false;
        public string PreviousPageUrl { get; private set; } = "./Page1"; // Default fallback

        // For Progress Bar
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        public ThankYouModel(ILogger<ThankYouModel> logger, ISurveyService surveyService)
        {
            _logger = logger;
            _surveyService = surveyService;
        }

        public IActionResult OnGet()
        {
            _logger.LogInformation("Thank You page (GET) accessed.");
            SurveySummary = HttpContext.Session.GetObjectFromJson<SurveyResponse>("SurveyResponse");

            if (SurveySummary == null)
            {
                _logger.LogWarning("No survey data found in session on Thank You page GET. Redirecting to start.");
                // Redirect to start if no data exists
                return RedirectToPage("./Page1");
            }

            CalculateProgressAndPreviousPage(SurveySummary.SurveyLengthChoice);

            return Page();
        }

        public async Task<IActionResult> OnPostSubmitAsync() // Handler named 'Submit'
        {
            _logger.LogInformation("Thank You page (POST Submit) triggered.");
            var surveyResponse = HttpContext.Session.GetObjectFromJson<SurveyResponse>("SurveyResponse");

            if (surveyResponse == null)
            {
                _logger.LogError("Submit POST called, but no survey data found in session.");
                ModelState.AddModelError(string.Empty, "Your session may have expired. Please start the survey again.");
                // Try to recalculate progress for display even on error
                CalculateProgressAndPreviousPage(null); // Pass null as choice is unknown now
                return Page();
            }

            CalculateProgressAndPreviousPage(surveyResponse.SurveyLengthChoice); // Recalculate for display

            try
            {
                // Call the BLL service to submit the data
                SubmissionSuccessful = await _surveyService.SubmitSurveyAsync(surveyResponse);

                if (SubmissionSuccessful)
                {
                    _logger.LogInformation("Survey submission successful for {Name}. Clearing session.", surveyResponse.Name);
                    // Clear the session ONLY on successful submission
                    HttpContext.Session.Remove("SurveyResponse");

                    // Keep the data for display on this request cycle
                    SurveySummary = surveyResponse;
                    ViewData["ResultMessage"] = "Thank you! Your survey has been submitted successfully.";
                    return Page(); // Show the Thank You page with success message
                }
                else
                {
                    _logger.LogWarning("Survey submission failed for {Name} (returned false from service).", surveyResponse.Name);
                    ModelState.AddModelError(string.Empty, "There was an issue submitting your survey. Please try again.");
                    SurveySummary = surveyResponse; // Keep data for potential retry/display
                    return Page(); // Show Thank You page with error
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during survey submission for {Name}.", surveyResponse.Name);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while submitting your survey. Please contact support or try again later.");
                SurveySummary = surveyResponse; // Keep data for display
                SubmissionSuccessful = false;
                return Page(); // Show Thank You page with error
            }
        }

        // Handler for the Back button on Thank You page
        public IActionResult OnPostBack()
        {
            _logger.LogInformation("Thank You page (POST Back) triggered.");
            var surveyResponse = HttpContext.Session.GetObjectFromJson<SurveyResponse>("SurveyResponse");
            CalculateProgressAndPreviousPage(surveyResponse?.SurveyLengthChoice); // Determine where to go back to
            return RedirectToPage(PreviousPageUrl); // Navigate back
        }

        private void CalculateProgressAndPreviousPage(string? surveyLengthChoice)
        {
            TotalPages = 2; // Min: Page 1, 2
            CurrentPage = 2; // Start calculation from end of mandatory pages
            PreviousPageUrl = "./Page2"; // Default back is Page 2

            if (!string.IsNullOrEmpty(surveyLengthChoice))
            {
                switch (surveyLengthChoice)
                {
                    case "OneMore":
                        TotalPages += 1; // Page 3
                        CurrentPage += 1;
                        PreviousPageUrl = "./Page3";
                        break;
                    case "TwoMore":
                        TotalPages += 2; // Page 3, 4
                        CurrentPage += 2;
                        PreviousPageUrl = "./Page4";
                        break;
                    case "Full":
                        TotalPages += 3; // Page 3, 4, 5
                        CurrentPage += 3;
                        PreviousPageUrl = "./Page5";
                        break;
                    case "EndHere":
                        // TotalPages and CurrentPage remain 2
                        PreviousPageUrl = "./Page2";
                        break;
                }
            }
            else
            {
                // If choice somehow missing, default to minimum or an error state might be better
                _logger.LogWarning("SurveyLengthChoice was null/empty when calculating progress on Thank You page.");
                PreviousPageUrl = "./Page2"; // Default back
            }

            TotalPages += 1; // Add Thank You page itself
            CurrentPage += 1; // Thank You is the last page

            _logger.LogDebug("ThankYou Page - CurrentPage: {Current}, TotalPages: {Total}, PreviousPageUrl: {BackUrl}", CurrentPage, TotalPages, PreviousPageUrl);
        }
    }
}