using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace MYAcademy_IdentityEmailProject.Helpers
{
    public static class MessageHelper
    {
        private static readonly CultureInfo Tr = new CultureInfo("tr-TR");
        private static readonly string[] AvatarColors = { "av-lav", "av-blue", "av-mint", "av-rose", "av-peach", "av-sun" };
        private static readonly string[] CategoryColors = { "#8B7FD6", "#5CC09A", "#EBA27A", "#6FA8E8", "#E07597", "#E0B23F", "#4FB3BF", "#A77BD8", "#7FA35B", "#D9825B" };

        public static string Initials(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "?";
            }

            var text = value.Contains('@') ? value.Split('@')[0].Replace(".", " ").Replace("_", " ") : value;
            var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return "?";
            }

            return string.Concat(parts.Take(2).Select(x => x[0])).ToUpper(Tr);
        }

        public static string AvatarColor(string? value)
        {
            var hash = 0;
            foreach (var item in value ?? "")
            {
                hash = (hash * 31 + item) & 0x7fffffff;
            }
            return AvatarColors[hash % AvatarColors.Length];
        }

        public static string Snippet(string? html, int max = 140)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return "";
            }

            var text = WebUtility.HtmlDecode(Regex.Replace(html, "<[^>]+>", " "));
            text = Regex.Replace(text, @"\s+", " ").Trim();
            return text.Length > max ? text.Substring(0, max).TrimEnd() + "…" : text;
        }

        public static string ShortTime(DateTime date)
        {
            var today = DateTime.Today;
            if (date.Date == today)
            {
                return date.ToString("HH:mm", Tr);
            }
            if (date.Date == today.AddDays(-1))
            {
                return "Dün";
            }
            if (date.Year == today.Year)
            {
                return date.ToString("d MMM", Tr);
            }
            return date.ToString("dd.MM.yyyy", Tr);
        }

        public static string LongTime(DateTime date)
        {
            return date.ToString("d MMMM yyyy, dddd HH:mm", Tr);
        }

        public static string CategoryColor(int? categoryId)
        {
            if (categoryId == null)
            {
                return "#85899B";
            }
            return CategoryColors[(categoryId.Value - 1 + CategoryColors.Length) % CategoryColors.Length];
        }

        public static string CategoryStyle(int? categoryId)
        {
            var color = CategoryColor(categoryId);
            return $"background:{color}1F;color:{color}";
        }
    }
}
