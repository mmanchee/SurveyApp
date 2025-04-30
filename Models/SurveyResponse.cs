using System.ComponentModel.DataAnnotations;

namespace SurveyApp.Models
{
    public class SurveyResponse
    {
        // Page 1
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100)]
        public string? Name { get; set; }

        [Range(1, 120, ErrorMessage = "Please enter a valid age.")]
        public int? Age { get; set; } // Nullable for potential initial state

        [Required(ErrorMessage = "Industry is required.")]
        public string? Industry { get; set; }
        public string? IndustryOther { get; set; }

        public DateTime SurveyDate { get; set; } = DateTime.Now; // Default to now

        [Required(ErrorMessage = "Position is required.")]
        public string? Position { get; set; }
        public string? PositionOther { get; set; }

        // Page 2
        [Required(ErrorMessage = "Please choose the survey length.")]
        public string? SurveyLengthChoice { get; set; } // e.g., "EndHere", "OneMore", "TwoMore", "Full"

        // Page 3 - Nullable as they might be skipped
        public string? Likes { get; set; } // No validation specified, assuming optional text

        public string? FavoriteColor { get; set; }
        public string? FavoriteColorOther { get; set; } // For write-in

        public List<string> FavoriteMusicGenres { get; set; } = new List<string>();
        public string? FavoriteMusicGenresOther { get; set; } // For write-in

        public string? FavoriteSeason { get; set; }
        public string? FavoriteSeasonOther { get; set; } // For write-in

        [StringLength(500)]
        public string? FavoritePlaceWhy { get; set; }

        // Page 4 - Nullable as they might be skipped
        // Likert scale values (1-5)
        [Range(1, 5, ErrorMessage = "Please rate 'I like my job'.")]
        public int? LikertJob { get; set; }

        [Range(1, 5, ErrorMessage = "Please rate 'I like my home'.")]
        public int? LikertHome { get; set; }

        [Range(1, 5, ErrorMessage = "Please rate 'I like my family'.")]
        public int? LikertFamily { get; set; }

        [Range(1, 5, ErrorMessage = "Please rate 'I like animals'.")]
        public int? LikertAnimals { get; set; }

        [Range(1, 5, ErrorMessage = "Please rate 'I like my friends'.")]
        public int? LikertFriends { get; set; }

        // Page 5 - Nullable as they might be skipped
        public string? WorstColor { get; set; }
        public string? WorstColorOther { get; set; } // For write-in

        public List<string> WorstMusicGenres { get; set; } = new List<string>();
        public string? WorstMusicGenresOther { get; set; } // For write-in

        public string? WorstSeason { get; set; }
        public string? WorstSeasonOther { get; set; } // For write-in

        [StringLength(500)]
        public string? WorstPlaceWhy { get; set; }

        // Helper properties (not saved to DB directly in this format)
        [System.Text.Json.Serialization.JsonIgnore] // Don't serialize this for session state directly if needed elsewhere
        public string FavoriteMusicGenresCsv => string.Join(",", FavoriteMusicGenres);

        [System.Text.Json.Serialization.JsonIgnore]
        public string WorstMusicGenresCsv => string.Join(",", WorstMusicGenres);
    }

    // --- Static lists for dropdowns/radios/checkboxes ---
    // (Could be moved to a separate helper class or loaded from config/DB)
    public static class SurveyOptions
    {
        public static List<string> Industries => new List<string>
        {
            "Technology", "Finance", "Healthcare", "Education", "Retail",
            "Manufacturing", "Hospitality", "Construction", "Transportation", "Other"
        };

        public static List<string> Positions => new List<string>
        {
            "Entry-Level", "Associate", "Specialist", "Analyst", "Coordinator",
            "Manager", "Senior Manager", "Director", "Senior Director", "Vice President",
            "Senior Vice President", "Executive", "Consultant", "Intern", "Other"
        };

        public static List<string> Colors => new List<string> { "Red", "Blue", "Green", "Yellow", "Purple", "Other" };
        public static List<string> MusicGenres => new List<string> { "Rock", "Pop", "Hip Hop", "Jazz", "Classical", "Electronic", "Other" };
        public static List<string> Seasons => new List<string> { "Spring", "Summer", "Autumn (Fall)", "Winter", "Other" };

        public static Dictionary<int, string> LikertScale => new Dictionary<int, string>
         {
             { 1, "Strongly Disagree" },
             { 2, "Disagree" },
             { 3, "Neutral/Ambivalent" },
             { 4, "Agree" },
             { 5, "Strongly Agree" }
         };
    }
}