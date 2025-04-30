using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; // Needed for SelectList
using Microsoft.Extensions.Logging;
using SurveyApp.Models;
using SurveyApp.Web.Helpers; // For SessionExtensions
using System.Collections.Generic; // Needed for List
using System.Linq; // Needed for Any()
using System.Threading.Tasks; // Added for async

namespace SurveyApp.Web.Pages.Survey
{
    public class Page5Model : PageModel
    {
        private readonly ILogger<Page5Model> _logger;

        [BindProperty]
        public SurveyResponse SurveyResponse { get; set; } = new SurveyResponse();

        // For progress bar
        public int CurrentPage { get; set; } = 5;
        public int TotalPages { get; set; } = 1; // Default, will be updated based on session

        // Options for dropdowns/radios/checkboxes
        public SelectList ColorOptions { get; set; } = new SelectList(SurveyOptions.Colors);
        // Use a separate list for binding in Razor Page for Worst Music Genres
        [BindProperty]
        public List<SelectListItem> WorstMusicGenresOptions { get; set; }

        public SelectList SeasonOptions { get; set; } = new SelectList(SurveyOptions.Seasons);

        // Note: MusicGenreOptions is the static list of all genres, used here to initialize WorstMusicGenresOptions
        public List<SelectListItem> MusicGenreOptions { get; set; }


        public Page5Model(ILogger<Page5Model> logger)
        {
            _logger = logger;
            // Initialize options lists
            MusicGenreOptions = SurveyOptions.MusicGenres.Select(genre => new SelectListItem
            {
                Value = genre,
                Text = genre
            }).ToList();

            WorstMusicGenresOptions = SurveyOptions.MusicGenres.Select(genre => new SelectListItem
            {
                Value = genre,
                Text = genre
            }).ToList();

        }

        public IActionResult OnGet()
        {
            _logger.LogInformation("Page 5 (GET) accessed.");
            var sessionResponse = HttpContext.Session.GetObjectFromJson<SurveyResponse>("SurveyResponse");

            if (sessionResponse == null)
            {
                _logger.LogWarning("No survey data found in session on Page 5 GET. Redirecting to start.");
                return RedirectToPage("./Page1"); // Redirect to start if no data exists
            }

            // Check if user should be on this page based on SurveyLengthChoice
            if (sessionResponse.SurveyLengthChoice != "Full")
            {
                _logger.LogInformation("User did not choose 'Full' on Page 2. Redirecting from Page 5 GET to Thank You page.");
                // Redirect to the page they should go to after Page 4, which is ThankYou
                return RedirectToPage("./ThankYou");
            }


            SurveyResponse = sessionResponse; // Load existing data from session

            // Ensure options are re-populated correctly even if loaded from session
            ColorOptions = new SelectList(SurveyOptions.Colors, SurveyResponse.WorstColor);
            // For checkboxes, set the Selected property based on session data (using WorstMusicGenresOptions)
            if (WorstMusicGenresOptions != null) // Add null check
            {
                foreach (var item in WorstMusicGenresOptions)
                {
                    if (item != null && SurveyResponse.WorstMusicGenres.Contains(item.Value))
                    {
                        item.Selected = true;
                    }
                    else
                    {
                        if (item != null) item.Selected = false; // Explicitly set to false if item is not null
                    }
                }
            }
            SeasonOptions = new SelectList(SurveyOptions.Seasons, SurveyResponse.WorstSeason);


            CalculateTotalPages(SurveyResponse.SurveyLengthChoice); // Calculate total pages

            return Page();
        }

        public IActionResult OnPost() // Change return type to Task<IActionResult> for async if submitting to service here
        {
            _logger.LogInformation("Page 5 (POST) submitted.");
            var sessionResponse = HttpContext.Session.GetObjectFromJson<SurveyResponse>("SurveyResponse");

            if (sessionResponse == null)
            {
                _logger.LogError("Page 5 POST called, but no survey data found in session. Redirecting to start.");
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
            if (ModelState.ContainsKey("SurveyResponse.Likes")) ModelState.Remove("SurveyResponse.Likes");
            if (ModelState.ContainsKey("SurveyResponse.FavoriteColor")) ModelState.Remove("SurveyResponse.FavoriteColor");
            if (ModelState.ContainsKey("SurveyResponse.FavoriteColorOther")) ModelState.Remove("SurveyResponse.FavoriteColorOther");
            if (ModelState.ContainsKey("SurveyResponse.FavoriteMusicGenres")) ModelState.Remove("SurveyResponse.FavoriteMusicGenres");
            if (ModelState.ContainsKey("SurveyResponse.FavoriteMusicGenresOther")) ModelState.Remove("SurveyResponse.FavoriteMusicGenresOther");
            if (ModelState.ContainsKey("SurveyResponse.FavoriteSeason")) ModelState.Remove("SurveyResponse.FavoriteSeason");
            if (ModelState.ContainsKey("SurveyResponse.FavoriteSeasonOther")) ModelState.Remove("SurveyResponse.FavoriteSeasonOther");
            if (ModelState.ContainsKey("SurveyResponse.FavoritePlaceWhy")) ModelState.Remove("SurveyResponse.FavoritePlaceWhy");
            if (ModelState.ContainsKey("SurveyResponse.LikertJob")) ModelState.Remove("SurveyResponse.LikertJob");
            if (ModelState.ContainsKey("SurveyResponse.LikertHome")) ModelState.Remove("SurveyResponse.LikertHome");
            if (ModelState.ContainsKey("SurveyResponse.LikertFamily")) ModelState.Remove("SurveyResponse.LikertFamily");
            if (ModelState.ContainsKey("SurveyResponse.LikertAnimals")) ModelState.Remove("SurveyResponse.LikertAnimals");
            if (ModelState.ContainsKey("SurveyResponse.LikertFriends")) ModelState.Remove("SurveyResponse.LikertFriends");


            // Update Page 5 fields - Model Binder will handle simple properties and lists
            // SurveyResponse.WorstMusicGenres will be populated directly by the model binder
            // due to the manual naming of checkboxes in the .cshtml.
            // Ensure other fields from previous pages are preserved
            sessionResponse.WorstColor = SurveyResponse.WorstColor;
            sessionResponse.WorstColorOther = SurveyResponse.WorstColorOther;
            sessionResponse.WorstMusicGenres = SurveyResponse.WorstMusicGenres ?? new List<string>(); // Model binder updates this, handle potential null
            sessionResponse.WorstMusicGenresOther = SurveyResponse.WorstMusicGenresOther;
            sessionResponse.WorstSeason = SurveyResponse.WorstSeason;
            sessionResponse.WorstSeasonOther = SurveyResponse.WorstSeasonOther;
            sessionResponse.WorstPlaceWhy = SurveyResponse.WorstPlaceWhy;


            CalculateTotalPages(sessionResponse.SurveyLengthChoice); // Needed for display if validation fails

            // Re-populate SelectLists and checkbox options in case of validation error
            ColorOptions = new SelectList(SurveyOptions.Colors, sessionResponse.WorstColor);
            // For checkboxes, set the Selected property based on the updated session data (from model binding)
            if (WorstMusicGenresOptions != null) // Add null check
            {
                foreach (var item in WorstMusicGenresOptions)
                {
                    if (item != null)
                    {
                        item.Selected = sessionResponse.WorstMusicGenres.Contains(item.Value);
                    }
                }
            }
            SeasonOptions = new SelectList(SurveyOptions.Seasons, sessionResponse.WorstSeason);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Page 5 validation failed.");
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
                _logger.LogInformation("Page 5 data saved to session.");

                // From Page 5, the next page is always Thank You
                string nextPage = "./ThankYou";
                return RedirectToPage(nextPage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Page 5 POST.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                SurveyResponse = sessionResponse; // Keep data for display after error
                // Re-populate options after error
                ColorOptions = new SelectList(SurveyOptions.Colors, sessionResponse.WorstColor);
                if (WorstMusicGenresOptions != null) // Add null check
                {
                    foreach (var item in WorstMusicGenresOptions)
                    {
                        if (item != null)
                        {
                            item.Selected = sessionResponse.WorstMusicGenres.Contains(item.Value);
                        }
                    }
                }
                SeasonOptions = new SelectList(SurveyOptions.Seasons, sessionResponse.WorstSeason);
                return Page();
            }
        }

        // Handler for the Back button
        public IActionResult OnPostBack()
        {
            _logger.LogInformation("Page 5 (POST Back) triggered.");
            var sessionResponse = HttpContext.Session.GetObjectFromJson<SurveyResponse>("SurveyResponse");
            CalculateTotalPages(sessionResponse?.SurveyLengthChoice); // Needed for progress bar display
            return RedirectToPage("./Page4"); // Always go back to Page 4 from Page 5
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