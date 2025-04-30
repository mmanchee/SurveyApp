using SurveyApp.Models;
using System.Threading.Tasks;

namespace SurveyApp.DAL
{
    public interface ISurveyRepository
    {
        Task SaveSurveyResponseAsync(SurveyResponse response);
    }
}