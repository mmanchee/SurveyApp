using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; // Needed for SelectList
using Microsoft.Extensions.Logging;
using SurveyApp.Models;
using SurveyApp.Web.Helpers; // For SessionExtensions
using System.Collections.Generic; // Needed for List
using System.Linq; // Needed for Any()

namespace SurveyApp.Web.Pages.Survey
{
    public class Page3Model : PageModel
    {
        private readonly ILogger<Page3Model> _logger;

        [BindProperty]
        public SurveyResponse SurveyResponse { get; set; } = new SurveyResponse();

        // For progress bar
        public int CurrentPage { get; set; } = 3;
        public int TotalPages { get; set; } = 1; // Default, will be updated based on session

        // Options for dropdowns/radios/checkboxes
        public SelectList ColorOptions { get; set; } = new SelectList(SurveyOptions.Colors);
        // Remove [BindProperty] here as we are manually processing checkbox values in OnPost
        public List<SelectListItem> MusicGenreOptions { get; set; } // Using List<SelectListItem> for checkboxes
        public SelectList SeasonOptions { get; set; } = new SelectList(SurveyOptions.Seasons);


        public Page3Model(ILogger<Page3Model> logger)
        {
            _logger = logger;
            // Initialize MusicGenreOptions
            MusicGenreOptions = SurveyOptions.MusicGenres.Select(genre => new SelectListItem
            {
                Value = genre,
                Text = genre
            }).ToList();
        }

        public IActionResult OnGet()
        {
            _logger.LogInformation("Page 3 (GET) accessed.");
            var sessionResponse = HttpContext.Session.GetObjectFromJson<SurveyResponse>("SurveyResponse");

            if (sessionResponse == null)
            {
                _logger.LogWarning("No survey data found in session on Page 3 GET. Redirecting to start.");
                return RedirectToPage("./Page1"); // Redirect to start if no data exists
            }

            // Check if user should be on this page based on SurveyLengthChoice
            if (sessionResponse.SurveyLengthChoice == "EndHere")
            {
                _logger.LogInformation("User chose 'EndHere' on Page 2. Redirecting from Page 3 GET to Thank You page.");
                return RedirectToPage("./ThankYou");
            }


            SurveyResponse = sessionResponse; // Load existing data from session

            // Ensure options are re-populated correctly even if loaded from session
            ColorOptions = new SelectList(SurveyOptions.Colors, SurveyResponse.FavoriteColor);
            // For checkboxes, set the Selected property based on session data (needed for display logic if using asp-for="...Selected", but not needed with manual checked attribute)
            // Keeping this loop updated based on SurveyResponse.FavoriteMusicGenres for consistency if MusicGenreOptions is used elsewhere for display purposes
            if (MusicGenreOptions != null) // Add null check
            {
                foreach (var item in MusicGenreOptions)
                {
                    if (item != null)
                    {
                        item.Selected = SurveyResponse.FavoriteMusicGenres.Contains(item.Value);
                    }
                }
            }
            SeasonOptions = new SelectList(SurveyOptions.Seasons, SurveyResponse.FavoriteSeason);


            CalculateTotalPages(SurveyResponse.SurveyLengthChoice); // Calculate total pages

            return Page();
        }

        public IActionResult OnPost()
        {
            _logger.LogInformation("Page 3 (POST) submitted.");
            var sessionResponse = HttpContext.Session.GetObjectFromJson<SurveyResponse>("SurveyResponse");

            if (sessionResponse == null)
            {
                _logger.LogError("Page 3 POST called, but no survey data found in session. Redirecting to start.");
                return RedirectToPage("./Page1");
            }

            // Remove validation errors for fields from previous pages
            if (ModelState.ContainsKey("SurveyResponse.Name")) ModelState.Remove("SurveyResponse.Name");
            if (ModelState.ContainsKey("SurveyResponse.Age")) ModelState.Remove("SurveyResponse.Age");
            if (ModelState.ContainsKey("SurveyResponse.Industry")) ModelState.Remove("SurveyResponse.Industry");
            if (ModelState.ContainsKey("SurveyResponse.IndustryOther")) ModelState.Remove("SurveyResponse.IndustryOther");
            if (ModelState.ContainsKey("SurveyResponse.Position")) ModelState.Remove("SurveyResponse.Position");
            if (ModelState.ContainsKey("SurveyResponse.PositionOther")) ModelState.Remove("SurveyResponse.PositionOther");
            if (ModelState.ContainsKey("SurveyResponse.SurveyLengthChoice")) ModelState.Remove("SurveyResponse.SurveyLengthChoice");


            // Update Page 3 fields - Model Binder will handle simple properties and lists
            // SurveyResponse.FavoriteMusicGenres will be populated directly by the model binder
            // due to the manual naming of checkboxes in the .cshtml.
            // Ensure other fields from previous pages are preserved
            sessionResponse.Likes = SurveyResponse.Likes;
            sessionResponse.FavoriteColor = SurveyResponse.FavoriteColor;
            sessionResponse.FavoriteColorOther = SurveyResponse.FavoriteColorOther;
            sessionResponse.FavoriteMusicGenres = SurveyResponse.FavoriteMusicGenres ?? new List<string>(); // Model binder updates this, handle potential null
            sessionResponse.FavoriteMusicGenresOther = SurveyResponse.FavoriteMusicGenresOther;
            sessionResponse.FavoriteSeason = SurveyResponse.FavoriteSeason;
            sessionResponse.FavoriteSeasonOther = SurveyResponse.FavoriteSeasonOther;
            sessionResponse.FavoritePlaceWhy = SurveyResponse.FavoritePlaceWhy;


            CalculateTotalPages(sessionResponse.SurveyLengthChoice); // Needed for display if validation fails

            // Re-populate SelectLists and checkbox options in case of validation error
            ColorOptions = new SelectList(SurveyOptions.Colors, sessionResponse.FavoriteColor);
            // For checkboxes, set the Selected property based on the updated session data (from model binding)
            if (MusicGenreOptions != null) // Add null check
            {
                foreach (var item in MusicGenreOptions)
                {
                    if (item != null)
                    {
                        item.Selected = sessionResponse.FavoriteMusicGenres.Contains(item.Value);
                    }
                }
            }
            SeasonOptions = new SelectList(SurveyOptions.Seasons, sessionResponse.FavoriteSeason);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Page 3 validation failed.");
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
                _logger.LogInformation("Page 3 data saved to session.");

                // Determine next page based on the choice made on Page 2
                string nextPage = "./ThankYou"; // Default
                switch (sessionResponse.SurveyLengthChoice)
                {
                    case "OneMore":
                        nextPage = "./ThankYou"; // Page 3 is the last content page
                        break;
                    case "TwoMore":
                        nextPage = "./Page4"; // Go to Page 4
                        break;
                    case "Full":
                        nextPage = "./Page4"; // Go to Page 4
                        break;
                    case "EndHere": // Should not happen if OnGet check works, but as a safeguard
                        nextPage = "./ThankYou";
                        break;
                    default:
                        _logger.LogWarning("Unexpected SurveyLengthChoice value after Page 3 submit: {Choice}", sessionResponse.SurveyLengthChoice);
                        nextPage = "./ThankYou"; // Fallback
                        break;
                }

                return RedirectToPage(nextPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Page 3 POST.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                SurveyResponse = sessionResponse; // Keep data for display after error
                // Re-populate options after error
                ColorOptions = new SelectList(SurveyOptions.Colors, sessionResponse.FavoriteColor);
                if (MusicGenreOptions != null) // Add null check
                {
                    foreach (var item in MusicGenreOptions)
                    {
                        if (item != null)
                        {
                            item.Selected = sessionResponse.FavoriteMusicGenres.Contains(item.Value);
                        }
                    }
                }
                SeasonOptions = new SelectList(SurveyOptions.Seasons, sessionResponse.FavoriteSeason);
                return Page();
            }
        }

        // Handler for the Back button
        public IActionResult OnPostBack()
        {
            _logger.LogInformation("Page 3 (POST Back) triggered.");
            var sessionResponse = HttpContext.Session.GetObjectFromJson<SurveyResponse>("SurveyResponse");
            CalculateTotalPages(sessionResponse?.SurveyLengthChoice); // Needed for progress bar display
            return RedirectToPage("./Page2"); // Always go back to Page 2 from Page 3
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