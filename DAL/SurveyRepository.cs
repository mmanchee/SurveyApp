using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SurveyApp.Models;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace SurveyApp.DAL
{
    public class SurveyRepository : ISurveyRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<SurveyRepository> _logger;

        // Inject IConfiguration to get connection string and ILogger for logging
        public SurveyRepository(IConfiguration configuration, ILogger<SurveyRepository> logger)
        {
            // Get connection string from appsettings.json
            // Ensure you have a "ConnectionStrings" section with a "SurveyDbConnection" key
            _connectionString = configuration.GetConnectionString("SurveyDbConnection")
                ?? throw new InvalidOperationException("Connection string 'SurveyDbConnection' not found.");
            _logger = logger;
        }

        public async Task SaveSurveyResponseAsync(SurveyResponse response)
        {
            _logger.LogInformation("Attempting to save survey response for {Name}", response.Name);
            try
            {
                // Use 'using' statements for proper disposal of SqlConnection and SqlCommand
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand command = new SqlCommand("sp_InsertSurveyResponse", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters - IMPORTANT: Match parameter names and types with your Stored Procedure
                        // Use DBNull.Value for nullable types if they are null
                        command.Parameters.AddWithValue("@Name", response.Name);
                        command.Parameters.AddWithValue("@Age", (object?)response.Age ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Industry", response.Industry);
                        command.Parameters.AddWithValue("@IndustryOther", response.IndustryOther);
                        command.Parameters.AddWithValue("@SurveyDate", response.SurveyDate);
                        command.Parameters.AddWithValue("@Position", response.Position);
                        command.Parameters.AddWithValue("@PositionOther", response.PositionOther);
                        command.Parameters.AddWithValue("@SurveyLengthChoice", response.SurveyLengthChoice);

                        // Handle potentially skipped pages (nullable fields)
                        command.Parameters.AddWithValue("@Likes", (object?)response.Likes ?? DBNull.Value);
                        command.Parameters.AddWithValue("@FavoriteColor", (object?)response.FavoriteColor ?? DBNull.Value);
                        command.Parameters.AddWithValue("@FavoriteColorOther", (object?)response.FavoriteColorOther ?? DBNull.Value);
                        command.Parameters.AddWithValue("@FavoriteMusicGenresCsv", response.FavoriteMusicGenres.Any() ? response.FavoriteMusicGenresCsv : DBNull.Value); // Pass CSV
                        command.Parameters.AddWithValue("@FavoriteMusicGenresOther", (object?)response.FavoriteMusicGenresOther ?? DBNull.Value);
                        command.Parameters.AddWithValue("@FavoriteSeason", (object?)response.FavoriteSeason ?? DBNull.Value);
                        command.Parameters.AddWithValue("@FavoriteSeasonOther", (object?)response.FavoriteSeasonOther ?? DBNull.Value);
                        command.Parameters.AddWithValue("@FavoritePlaceWhy", (object?)response.FavoritePlaceWhy ?? DBNull.Value);

                        command.Parameters.AddWithValue("@LikertJob", (object?)response.LikertJob ?? DBNull.Value);
                        command.Parameters.AddWithValue("@LikertHome", (object?)response.LikertHome ?? DBNull.Value);
                        command.Parameters.AddWithValue("@LikertFamily", (object?)response.LikertFamily ?? DBNull.Value);
                        command.Parameters.AddWithValue("@LikertAnimals", (object?)response.LikertAnimals ?? DBNull.Value);
                        command.Parameters.AddWithValue("@LikertFriends", (object?)response.LikertFriends ?? DBNull.Value);

                        command.Parameters.AddWithValue("@WorstColor", (object?)response.WorstColor ?? DBNull.Value);
                        command.Parameters.AddWithValue("@WorstColorOther", (object?)response.WorstColorOther ?? DBNull.Value);
                        command.Parameters.AddWithValue("@WorstMusicGenresCsv", response.WorstMusicGenres.Any() ? response.WorstMusicGenresCsv : DBNull.Value); // Pass CSV
                        command.Parameters.AddWithValue("@WorstMusicGenresOther", (object?)response.WorstMusicGenresOther ?? DBNull.Value);
                        command.Parameters.AddWithValue("@WorstSeason", (object?)response.WorstSeason ?? DBNull.Value);
                        command.Parameters.AddWithValue("@WorstSeasonOther", (object?)response.WorstSeasonOther ?? DBNull.Value);
                        command.Parameters.AddWithValue("@WorstPlaceWhy", (object?)response.WorstPlaceWhy ?? DBNull.Value);

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                        _logger.LogInformation("Successfully saved survey response for {Name}", response.Name);
                    }
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Error saving survey response for {Name}. ConnectionString: {ConnString}", response.Name, _connectionString);
                // Consider re-throwing a custom exception or returning a failure status
                throw; // Re-throw for BLL/UI layer to handle
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Generic Error saving survey response for {Name}", response.Name);
                throw; // Re-throw
            }
        }
    }
}