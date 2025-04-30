using Microsoft.Extensions.Logging;
using SurveyApp.DAL;
using SurveyApp.Models;
using System;
using System.Threading.Tasks;

namespace SurveyApp.BLL
{
    public class SurveyService : ISurveyService
    {
        private readonly ISurveyRepository _surveyRepository;
        private readonly ILogger<SurveyService> _logger;

        public SurveyService(ISurveyRepository surveyRepository, ILogger<SurveyService> logger)
        {
            _surveyRepository = surveyRepository;
            _logger = logger;
        }

        public async Task<bool> SubmitSurveyAsync(SurveyResponse response)
        {
            // Add any business logic/validation here if needed,
            // beyond basic model validation which happens in Razor PageModel.
            // For example, ensuring consistency between answers if required.

            _logger.LogInformation("Submitting survey for user: {Name}", response?.Name ?? "Unknown");

            if (response == null)
            {
                _logger.LogWarning("SubmitSurveyAsync called with null response object.");
                return false;
            }

            try
            {
                // Pre-process data if necessary before sending to DAL
                // (e.g., ensuring 'Other' text is cleared if 'Other' option wasn't selected)
                CleanUpOtherFields(response);

                await _surveyRepository.SaveSurveyResponseAsync(response);
                _logger.LogInformation("Survey submitted successfully for {Name}", response.Name);
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception that bubbled up from the DAL
                _logger.LogError(ex, "Error submitting survey for {Name}", response.Name);
                return false; // Indicate failure
            }
        }

        private void CleanUpOtherFields(SurveyResponse response)
        {
            // Example: Clear 'Other' text if a non-'Other' option was chosen
            if (response.FavoriteColor != "Other") response.FavoriteColorOther = null;
            if (!response.FavoriteMusicGenres.Contains("Other")) response.FavoriteMusicGenresOther = null;
            if (response.FavoriteSeason != "Other") response.FavoriteSeasonOther = null;

            if (response.WorstColor != "Other") response.WorstColorOther = null;
            if (!response.WorstMusicGenres.Contains("Other")) response.WorstMusicGenresOther = null;
            if (response.WorstSeason != "Other") response.WorstSeasonOther = null;

            // Remove "Other" itself from the list if the text box is empty/null
            if (response.FavoriteMusicGenres.Contains("Other") && string.IsNullOrWhiteSpace(response.FavoriteMusicGenresOther))
            {
                response.FavoriteMusicGenres.Remove("Other");
            }
            if (response.WorstMusicGenres.Contains("Other") && string.IsNullOrWhiteSpace(response.WorstMusicGenresOther))
            {
                response.WorstMusicGenres.Remove("Other");
            }
        }
    }
}