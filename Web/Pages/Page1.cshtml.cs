using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SurveyApp.Models;
using SurveyApp.Web.Helpers; // For SessionExtensions
using System.Collections.Generic;
using System.Linq;

namespace SurveyApp.Web.Pages.Survey
{
    public class Page1Model : PageModel
    {
        private readonly ILogger<Page1Model> _logger;

        [BindProperty]
        public SurveyResponse SurveyResponse { get; set; } = new SurveyResponse();

        // For progress bar
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1; // Default, will be updated based on session

        // Options for dropdowns
        public SelectList IndustryOptions { get; set; }
        public SelectList PositionOptions { get; set; }

        public Page1Model(ILogger<Page1Model> logger)
        {
            _logger = logger;
            // Initialize options here as well, in case OnGet is skipped (e.g., direct POST)
            IndustryOptions = new SelectList(SurveyOptions.Industries);
            PositionOptions = new SelectList(SurveyOptions.Positions);
        }

        public IActionResult OnGet()
        {
            _logger.LogInformation("Page 1 (GET) accessed.");
            // Load existing data from session if user is navigating back
            var sessionResponse = HttpContext.Session.GetObjectFromJson<SurveyResponse>("SurveyResponse");
            if (sessionResponse != null)
            {
                _logger.LogInformation("Loading existing survey data from session.");
                SurveyResponse = sessionResponse;
            }
            else
            {
                _logger.LogInformation("No existing survey data in session, starting new.");
                // Ensure clean state if no session or starting new
                SurveyResponse = new SurveyResponse();
            }

            // Ensure options are populated correctly based on current SurveyResponse state for display
            IndustryOptions = new SelectList(SurveyOptions.Industries, SurveyResponse.Industry);
            PositionOptions = new SelectList(SurveyOptions.Positions, SurveyResponse.Position);


            // Calculate total pages based on session (if available)
            CalculateTotalPages(sessionResponse?.SurveyLengthChoice);
            return Page();
        }

        public IActionResult OnPost()
        {
            _logger.LogInformation("Page 1 (POST) submitted.");
            var sessionResponse = HttpContext.Session.GetObjectFromJson<SurveyResponse>("SurveyResponse");

            // Merge current page data with existing session data (if any)
            var updatedResponse = sessionResponse ?? new SurveyResponse();

            // Update Page 1 fields from the posted SurveyResponse
            updatedResponse.Name = SurveyResponse.Name;
            updatedResponse.Age = SurveyResponse.Age;
            updatedResponse.Industry = SurveyResponse.Industry;
            updatedResponse.IndustryOther = SurveyResponse.IndustryOther; // Assuming this property exists now
            updatedResponse.Position = SurveyResponse.Position;
            updatedResponse.PositionOther = SurveyResponse.PositionOther; // Assuming this property exists now

            updatedResponse.SurveyDate = SurveyResponse.SurveyDate;


            // ** NEW: Remove validation error for SurveyLengthChoice on Page 1 POST **
            // This field is required but set on a later page (Page 2).
            // Remove the error from ModelState so Page 1 validation passes if other fields are valid.
            if (ModelState.ContainsKey("SurveyResponse.SurveyLengthChoice"))
            {
                ModelState.Remove("SurveyResponse.SurveyLengthChoice");
                _logger.LogDebug("Removed SurveyLengthChoice validation error for Page 1 POST.");
            }


            // Calculate total pages based on session *before* validating, for progress bar display
            CalculateTotalPages(updatedResponse.SurveyLengthChoice);

            // Re-populate SelectLists in case of validation error (for Page 1 fields)
            IndustryOptions = new SelectList(SurveyOptions.Industries, updatedResponse.Industry);
            PositionOptions = new SelectList(SurveyOptions.Positions, updatedResponse.Position);


            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Page 1 validation failed (after removing SurveyLengthChoice error).");
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Any())
                    {
                        _logger.LogWarning("Validation Error - Key: {Key}, Errors: {Errors}", state.Key, string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage)));
                    }
                }
                // Ensure the Page model's SurveyResponse reflects the posted data for validation messages
                SurveyResponse = updatedResponse;
                return Page(); // Stay on the same page, validation messages will show
            }

            try
            {
                // Save updated data back to session
                HttpContext.Session.SetObjectAsJson("SurveyResponse", updatedResponse);
                _logger.LogInformation("Page 1 data saved to session for user: {Name}", updatedResponse.Name);

                // Navigate to the next page
                return RedirectToPage("./Page2");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Page 1 POST.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                // Re-populate options after error
                IndustryOptions = new SelectList(SurveyOptions.Industries, updatedResponse.Industry);
                PositionOptions = new SelectList(SurveyOptions.Positions, updatedResponse.Position);
                // Ensure the Page model's SurveyResponse reflects the posted data after error
                SurveyResponse = updatedResponse;
                return Page();
            }
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