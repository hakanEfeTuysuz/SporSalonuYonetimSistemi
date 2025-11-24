using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SporSalonuYonetimSistemi.Controllers
{
    [Authorize(Roles = "Uye")]
    public class YapayZekaController : Controller
    {
        // GÜNCELLENDİ: Senin listende bulunan 'gemini-2.0-flash' modelini kullanıyoruz
        private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

        // DİKKAT: Buraya API Key'ini yapıştırmayı unutma!
        private const string ApiKey = "";

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TavsiyeAl(string cinsiyet, double kilo, double boy, string hedef)
        {
            // 1. Soruyu Hazırla
            string soru = $"Spor salonu üyesiyim. Cinsiyet: {cinsiyet}, Kilo: {kilo}kg, Boy: {boy}cm. " +
                          $"Hedefim: {hedef}. " +
                          $"Bana maddeler halinde kısa, öz ve motive edici bir egzersiz ve beslenme tavsiyesi ver. " +
                          $"Lütfen cevabı HTML formatında (<b>, <br> kullanarak) ver.";

            // 2. İsteği Paketle
            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = soru } } }
                }
            };

            string jsonBody = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            using (var client = new HttpClient())
            {
                // 3. Google'a Gönder (gemini-2.0-flash modeline)
                var response = await client.PostAsync($"{ApiUrl}?key={ApiKey}", content);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Cevabı al ve ekrana gönder
                    var result = JsonSerializer.Deserialize<GeminiResponse>(jsonResponse);
                    ViewBag.Cevap = result?.Candidates?[0]?.Content?.Parts?[0]?.Text;
                }
                else
                {
                    // Hata olursa göster
                    ViewBag.Cevap = $"<div class='alert alert-danger'>Hata Oluştu: {jsonResponse}</div>";
                }
            }

            return View("Index");
        }

        // Google Cevap Modelleri
        public class GeminiResponse
        {
            [JsonPropertyName("candidates")]
            public Candidate[] Candidates { get; set; }
        }
        public class Candidate
        {
            [JsonPropertyName("content")]
            public Content Content { get; set; }
        }
        public class Content
        {
            [JsonPropertyName("parts")]
            public Part[] Parts { get; set; }
        }
        public class Part
        {
            [JsonPropertyName("text")]
            public string Text { get; set; }
        }
    }
}