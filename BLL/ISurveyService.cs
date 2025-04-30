using SurveyApp.Models;
using System.Threading.Tasks;

namespace SurveyApp.BLL
{
    public interface ISurveyService
    {
        Task<bool> SubmitSurveyAsync(SurveyResponse response);
    }
}