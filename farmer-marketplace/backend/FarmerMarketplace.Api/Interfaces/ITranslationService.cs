// backend/FarmerMarketplace.Api/Interfaces/ITranslationService.cs

namespace FarmerMarketplace.Api.Interfaces
{
    public interface ITranslationService
    {
        Task<string> TranslateAsync(string text, string targetLanguage);
        Task<Dictionary<string, string>> TranslateBatchAsync(IEnumerable<string> texts, string targetLanguage);
    }
}
