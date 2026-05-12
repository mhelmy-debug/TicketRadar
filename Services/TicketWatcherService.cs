// File: Services/TicketCheckerService.cs
using System.Text.Json;

namespace TicketRadar.Services
{
    public class TicketCheckerService
    {
        private static readonly HttpClient HttpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        public async Task CheckAsync(CancellationToken cancellationToken = default)
        {
            TicketStatusStore.LastStatus = "اختر النادي واضغط تشغيل الرادار";
            TicketStatusStore.LastCheckTime = DateTime.Now;
            TicketStatusStore.LastBookingUrl = "https://www.tazkarti.com/#/matches";

            var selectedTeam = TicketStatusStore.SelectedTeam?.Trim();

            if (string.IsNullOrWhiteSpace(selectedTeam))
            {
                return;
            }

            try
            {
                var url = $"https://www.tazkarti.com/data/matches-list-json.json?_={DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
                var json = await HttpClient.GetStringAsync(url, cancellationToken);

                var matches = JsonSerializer.Deserialize<List<TazkartiMatch>>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                var availableMatches = (matches ?? new List<TazkartiMatch>())
                    .Where(x =>
                        x.MatchStatus == 1 &&
                        x.MatchHasBookedTickets &&
                        (
                            ContainsText(x.TeamNameAr1, selectedTeam) ||
                            ContainsText(x.TeamNameAr2, selectedTeam) ||
                            ContainsText(x.TeamName1, selectedTeam) ||
                            ContainsText(x.TeamName2, selectedTeam)
                        ))
                    .OrderBy(x => x.KickOffTime ?? DateTime.MaxValue)
                    .ToList();

                if (availableMatches.Any())
                {
                    var messages = availableMatches.Select(match =>
                        $"🔥 {GetSafeValue(match.TeamNameAr1, match.TeamName1)} ضد {GetSafeValue(match.TeamNameAr2, match.TeamName2)} - {GetSafeValue(match.StadiumNameAr, "غير محدد")} - {(match.KickOffTime?.ToString("yyyy/MM/dd hh:mm tt") ?? "الوقت غير محدد")}"
                    );

                    TicketStatusStore.LastStatus = string.Join("<br>", messages);
                    TicketStatusStore.LastCheckTime = DateTime.Now;
                    TicketStatusStore.LastBookingUrl = "https://www.tazkarti.com/#/matches";
                }
                else
                {
                    TicketStatusStore.LastStatus = $"لا توجد تذاكر متاحة حالياً لـ: {selectedTeam}";
                    TicketStatusStore.LastCheckTime = DateTime.Now;
                    TicketStatusStore.LastBookingUrl = "https://www.tazkarti.com/#/matches";
                }
            }
            catch (Exception ex)
            {
                TicketStatusStore.LastStatus = "حدث خطأ أثناء الفحص: " + ex.Message;
                TicketStatusStore.LastCheckTime = DateTime.Now;
                TicketStatusStore.LastBookingUrl = "https://www.tazkarti.com/#/matches";
            }
        }

        private static bool ContainsText(string? source, string selectedTeam)
        {
            return !string.IsNullOrWhiteSpace(source) &&
                   source.Contains(selectedTeam, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetSafeValue(string? preferred, string? fallback)
        {
            if (!string.IsNullOrWhiteSpace(preferred))
            {
                return preferred;
            }

            if (!string.IsNullOrWhiteSpace(fallback))
            {
                return fallback;
            }

            return "غير محدد";
        }
    }

    public class TazkartiMatch
    {
        public int MatchId { get; set; }
        public int MatchStatus { get; set; }
        public string? TeamName1 { get; set; }
        public string? TeamName2 { get; set; }
        public string? TeamNameAr1 { get; set; }
        public string? TeamNameAr2 { get; set; }
        public string? StadiumNameAr { get; set; }
        public DateTime? KickOffTime { get; set; }
        public bool MatchHasBookedTickets { get; set; }
    }
}

