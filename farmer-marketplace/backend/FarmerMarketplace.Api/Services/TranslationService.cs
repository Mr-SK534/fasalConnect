// backend/FarmerMarketplace.Api/Services/TranslationService.cs

using System.Text;
using System.Text.Json;
using FarmerMarketplace.Api.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace FarmerMarketplace.Api.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;
        private readonly ILogger<TranslationService> _logger;

        // Static built-in agricultural domain dictionary for zero-latency vernacular translation
        private static readonly Dictionary<string, Dictionary<string, string>> AgriculturalDictionary = new(StringComparer.OrdinalIgnoreCase)
        {
            // --- Crops ---
            ["Red Onion"] = new() { ["hi"] = "लाल प्याज", ["mr"] = "लाल कांदा", ["ta"] = "சிவப்பு வெங்காயம்" },
            ["Onion"] = new() { ["hi"] = "प्याज", ["mr"] = "कांदा", ["ta"] = "வெங்காயம்" },
            ["Tomato"] = new() { ["hi"] = "टमाटर", ["mr"] = "टोमॅटो", ["ta"] = "தக்காளி" },
            ["Potato"] = new() { ["hi"] = "आलू", ["mr"] = "बटाटा", ["ta"] = "உருளைக்கிழங்கு" },
            ["Wheat"] = new() { ["hi"] = "गेहूं", ["mr"] = "गहू", ["ta"] = "கோதுமை" },
            ["Rice"] = new() { ["hi"] = "चावल", ["mr"] = "तांदूळ", ["ta"] = "அரிசி" },
            ["Soybean"] = new() { ["hi"] = "सोयाबीन", ["mr"] = "सोयाबीन", ["ta"] = "சோயாபீன்" },
            ["Cotton"] = new() { ["hi"] = "कपास", ["mr"] = "कापूस", ["ta"] = "பருத்தி" },
            ["Garlic"] = new() { ["hi"] = "लहसुन", ["mr"] = "लसूण", ["ta"] = "பூண்டு" },
            ["Chilli"] = new() { ["hi"] = "मिर्च", ["mr"] = "मिरची", ["ta"] = "மிளகாய்" },
            ["Green Chilli"] = new() { ["hi"] = "हरी मिर्च", ["mr"] = "हिरवी मिरची", ["ta"] = "பச்சை மிளகாய்" },
            ["Maize"] = new() { ["hi"] = "मक्का", ["mr"] = "मका", ["ta"] = "மக்காச்சோளம்" },
            ["Sugarcane"] = new() { ["hi"] = "गन्ना", ["mr"] = "ऊस", ["ta"] = "கரும்பு" },
            ["Mango"] = new() { ["hi"] = "आम", ["mr"] = "आंबा", ["ta"] = "மாம்பழம்" },
            ["Ginger"] = new() { ["hi"] = "अदरक", ["mr"] = "आले", ["ta"] = "இஞ்சி" },
            ["Turmeric"] = new() { ["hi"] = "हल्दी", ["mr"] = "हळद", ["ta"] = "மஞ்சள்" },
            ["Mustard"] = new() { ["hi"] = "सरसों", ["mr"] = "मोहरी", ["ta"] = "கடுகு" },
            ["Dragon Fruit"] = new() { ["hi"] = "ड्रैगन फ्रूट", ["mr"] = "ड्रॅगन फ्रूट", ["ta"] = "டிராகன் பழம்" },

            // --- Market Signals ---
            ["HIGH_DEMAND"] = new() { ["hi"] = "🟢 उच्च मांग (High Demand)", ["mr"] = "🟢 भरपूर मागणी (High Demand)", ["ta"] = "🟢 அதிக தேவை" },
            ["STABLE_DEMAND"] = new() { ["hi"] = "🟡 स्थिर मांग (Stable Market)", ["mr"] = "🟡 स्थिर बाजार (Stable Market)", ["ta"] = "🟡 சீரான சந்தை" },
            ["EXCESS_SUPPLY"] = new() { ["hi"] = "🔴 अधिक आवक सतर्कता (Glut Warning)", ["mr"] = "🔴 जास्त आवक इशारा (Glut Warning)", ["ta"] = "🔴 அதிக வரத்து எச்சரிக்கை" },

            // --- Common Advisories ---
            ["Optimal Harvest Timing: Demand is surging by +20% next week. Harvest now for maximum price."] = new() {
                ["hi"] = "कटाई का सही समय: अगले सप्ताह मांग में +20% की वृद्धि। अधिकतम मूल्य के लिए अभी कटाई करें।",
                ["mr"] = "काढणीची योग्य वेळ: पुढील आठवड्यात मागणीत +20% वाढ. जास्तीत जास्त दरासाठी आताच काढणी करा.",
                ["ta"] = "அறுவடைக்கு உகந்த நேரம்: அடுத்த வாரம் தேவை 20% அதிகரிக்கும். அதிக லாபத்திற்கு இப்போதே அறுவடை செய்யுங்கள்."
            },
            ["Market Stable: Prices expected to remain steady. Target direct bulk buyers on FasalConnect."] = new() {
                ["hi"] = "बाजार स्थिर: कीमतें स्थिर रहने की संभावना। फसलकनेक्ट पर सीधे थोक खरीदारों को बेचें।",
                ["mr"] = "बाजार स्थिर: दर स्थिर राहण्याची शक्यता. फसलकनेक्टवर थेट घाऊक व्यापाऱ्यांना विका.",
                ["ta"] = "சந்தை சீராக உள்ளது: விலைகள் நிலையாக இருக்கும். நேரடியாக பெரிய கொள்முதல் செய்பவர்களுக்கு விற்கவும்."
            },
            ["Excess Mandi Arrival: Local mandis flooded. Hold harvest by 5-7 days or sell directly."] = new() {
                ["hi"] = "मंडी में अधिक आवक: स्थानीय मंडियों में माल ज्यादा। 5-7 दिन कटाई रोकें या सीधे फसलकनेक्ट पर बेचें।",
                ["mr"] = "मंडीत जास्त आवक: स्थानिक बाजार समित्यांमध्ये माल जास्त. 5-7 दिवस काढणी थांबवा किंवा थेट विका.",
                ["ta"] = "மண்டியில் அதிக வரத்து: அறுவடையை 5-7 நாட்கள் தாமதப்படுத்தவும் அல்லது நேரடியாக விற்கவும்."
            },

            // --- Sources ---
            ["Govt Agmarknet APMC Mandi Benchmark Feed"] = new() {
                ["hi"] = "सरकारी एगमार्कनेट एपीएमसी मंडी बेंचमार्क दर",
                ["mr"] = "शासकीय ॲगमार्कनेट एपीएमसी बाजार समिती बेंचमार्क दर",
                ["ta"] = "அரசு ஆக்மார்க்நெட் ஏபிஎம்சி சந்தை விலை குறிப்பு"
            }
        };

        public TranslationService(IMemoryCache cache, HttpClient httpClient, ILogger<TranslationService> logger)
        {
            _cache = cache;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> TranslateAsync(string text, string targetLanguage)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(targetLanguage))
                return text;

            string lang = targetLanguage.Trim().ToLowerInvariant();
            if (lang == "en" || lang.StartsWith("en-"))
                return text; // English original

            // Clean language code e.g. "hi-IN" -> "hi"
            if (lang.Contains('-')) lang = lang.Split('-')[0];

            string cacheKey = $"trans:{lang}:{text.Trim()}";
            if (_cache.TryGetValue(cacheKey, out string? cachedTranslation) && !string.IsNullOrEmpty(cachedTranslation))
            {
                return cachedTranslation;
            }

            // Step 1: Check Built-in Agricultural Dictionary
            if (AgriculturalDictionary.TryGetValue(text.Trim(), out var langDict))
            {
                if (langDict.TryGetValue(lang, out var dictTranslation))
                {
                    _cache.Set(cacheKey, dictTranslation, TimeSpan.FromDays(30));
                    return dictTranslation;
                }
            }

            // Step 2: Online Translation API Fallback (MyMemory Free API)
            try
            {
                string url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair=en|{lang}";
                using var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var responseData = doc.RootElement.GetProperty("responseData");
                    string translatedText = responseData.GetProperty("translatedText").GetString() ?? "";

                    if (!string.IsNullOrWhiteSpace(translatedText) && !translatedText.Equals(text, StringComparison.OrdinalIgnoreCase))
                    {
                        _cache.Set(cacheKey, translatedText, TimeSpan.FromDays(7));
                        return translatedText;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Online translation fallback failed for text '{Text}'. Returning original.", text);
            }

            // Step 3: Graceful Fallback (Return Original Text)
            return text;
        }

        public async Task<Dictionary<string, string>> TranslateBatchAsync(IEnumerable<string> texts, string targetLanguage)
        {
            var result = new Dictionary<string, string>();
            foreach (var text in texts.Distinct())
            {
                result[text] = await TranslateAsync(text, targetLanguage);
            }
            return result;
        }
    }
}
