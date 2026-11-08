using System.Text.RegularExpressions;
using GameStudioSite.Controllers;
using GameStudioSite.Models;
using HtmlAgilityPack;

namespace GameStudioSite.Services
{
    public static class SiteSearch
    {
        private const StringComparison OIC = StringComparison.OrdinalIgnoreCase;


        // Fixed pages that aren't in the seed data
        private static readonly (string Title, string Url, string Description, string Keywords)[] FixedPages =
        {
            ("Games", "/Games", "Play the games from JZB4 Studios: online multiplayer, racing, shooters, platformers and more. Made by our team.", "games play download itch"),
            ("Services", "/Services", "Game development, design, art, consultation and more. Tell us your idea and we’ll shape the team around it.", "work hire client"),
            ("About", "/About", "JZB4 Studios is a group of game developers and designers growing a community where developers and players make games together.", "team mission vision community"),
            ("Contact", "/Contact", "Questions, ideas or a project in mind? Get in touch with JZB4 Studios.", "email phone address message get in touch"),
            ("Updates", "/Updates", "Read the latest updates from JZB4 Studios: game releases, development news, and more.", "news blog releases"),
            ("Privacy Policy", "/Privacy", "Learn how JZB4 Studios collects, uses, and protects your personal data.", "data gdpr cookies"),
        };
        // Extra words people might type, keyed by the service's anchor (the part after #)
        private static readonly Dictionary<string, string> ServiceKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            ["development"] = "unity unreal programming physics ai animation pc console mobile",
            ["design"] = "mechanics concept story",
            ["multiplayer"] = "networking online photon playfab",
            ["art"] = "visuals models characters environments effects blender 3D 2D",
            ["gamification"] = "engagement retention rewards",
            ["consultation"] = "advice guidance guide",
        };

        private static IEnumerable<(string Title, string Url, string Description, string Keywords)> AllPages()
        {
            foreach (var p in FixedPages) yield return p;

            foreach (var s in ServiceSeedData.All)          
            {
                var anchor = s.Slug;                                  
                ServiceKeywords.TryGetValue(anchor, out var extra);
                yield return (s.Title, $"/Services#{anchor}", s.ShortDescription, extra ?? "");
            }
        }
        /*
        private static readonly (string Title, string Url, string Description, string Keywords)[] Pages =
        {
            ("Games", "/Games", "Play the games from JZB4 Studios: online multiplayer, racing, shooters, platformers and more. Made by our team.", "games play download itch"),
            ("Services", "/Services", "Game development, design, art, consultation and more. Tell us your idea and we’ll shape the team around it.", "work hire client"),
            ("Game Development", "/Services#development",  "Immersive worlds and engaging gameplay, tailored to your vision.", "unity unreal programming physics ai animation pc console mobile"),
            ("Game Design", "/Services#design", "Captivating mechanics and ideas that bring your concept to life.", "mechanics concept story"),
            ("Multiplayer Development", "/Services#multiplayer", "Local and online multiplayer that connects players worldwide.", "networking online photon playfab"),
            ("2D/3D Art", "/Services#art", "Characters, environments and effects, from concept art to 3D models.", "visuals models characters environments effects blender"),
            ("Gamification", "/Services#gamification", "Game mechanics that boost engagement and retention in applications.", "engagement retention rewards"),
            ("Consultation", "/Services#consultation", "Expert guidance and insights to elevate your project.", "advice guidance"),
            ("About", "/About", "Who we are, and our mission and vision.", "team mission vision community"),
            ("Contact", "/Contact", "Send us a message or find our details.", "email phone address message get in touch"),
            ("Privacy Policy", "/Privacy", "How we handle your personal data.", "data gdpr cookies"),
        };
        */
        public static List<SearchResult> Run(IEnumerable<BlogPost> posts, IEnumerable<Game> games, string[] terms)
        {
            var results = new List<SearchResult>();
            results.AddRange(FindPages(terms));
            results.AddRange(FindGames(games, terms));
            results.AddRange(FindPosts(posts, terms));

            return results
                .OrderByDescending(r => r.Score)
                .ThenByDescending(r => r.Date ?? DateTime.MinValue)
                .ToList();
        }

        private static IEnumerable<SearchResult> FindPages(string[] terms)
        {
            foreach (var p in AllPages())
            {
                var hay = $"{p.Title} {p.Description} {p.Keywords}";
                if (!terms.All(t => hay.Contains(t, OIC))) continue;

                var score = terms.Sum(t => p.Title.Contains(t, OIC) ? 3 : p.Description.Contains(t, OIC) ? 2 : 1);
                yield return new SearchResult
                {
                    Kind = ResultKind.Page,
                    Title = p.Title,
                    Url = p.Url,
                    Label = "Page",
                    Description = p.Description,
                    Score = score
                };
            }
        }

        private static IEnumerable<SearchResult> FindGames(IEnumerable<Game> games, string[] terms)
        {
            foreach (var g in games)
            {
                // Games with an internal /Games/slug link are covered by their post
                if (!Uri.TryCreate(g.GameUrl?.Trim(), UriKind.Absolute, out var uri)) continue;
                if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) continue;

                var hay = $"{g.Title} {g.Description}";
                if (!terms.All(t => hay.Contains(t, OIC))) continue;

                var score = terms.Sum(t => g.Title.Contains(t, OIC) ? 3 : 2);
                var host = uri.Host.StartsWith("www.", OIC) ? uri.Host[4..] : uri.Host;

                yield return new SearchResult
                {
                    Kind = ResultKind.Game,
                    Title = g.Title,
                    Url = uri.AbsoluteUri,
                    External = true,
                    ImageUrl = g.ImageUrl,
                    ImageAlt = g.Title,
                    Label = "Game",
                    Host = host,
                    Description = g.Description,
                    Score = score
                };
            }
        }

        private static string PlainText(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return "";
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var parts = doc.DocumentNode.DescendantsAndSelf()
                .Where(n => n.NodeType == HtmlNodeType.Text
                            && n.ParentNode.Name is not ("script" or "style"))
                .Select(n => HtmlEntity.DeEntitize(n.InnerText));

            return Regex.Replace(string.Join(" ", parts), @"\s+", " ").Trim();
        }

        private static IEnumerable<SearchResult> FindPosts(IEnumerable<BlogPost> posts, string[] terms)
        {
            foreach (var p in posts)
            {
                var title = p.Title ?? "";
                var excerpt = p.Excerpt ?? "";
                var tags = (p.Tags ?? Enumerable.Empty<string>()).ToList();
                var tagText = string.Join(" ", tags);
                var body = PlainText(p.ContentHtml);

                if (!terms.All(t => $"{title} {tagText} {excerpt} {body}".Contains(t, OIC))) continue;

                var inTitle = terms.Any(t => title.Contains(t, OIC));
                var inTags = terms.Any(t => tagText.Contains(t, OIC));
                var inBody = terms.Any(t => body.Contains(t, OIC));

                var score = 0;
                foreach (var t in terms)
                {
                    if (title.Contains(t, OIC)) score += 3;
                    if (tagText.Contains(t, OIC)) score += 2;
                    if (excerpt.Contains(t, OIC)) score += 2;
                    if (body.Contains(t, OIC)) score += 1;
                }

                // Title or tag match: show the normal description.
                // Content match: show the matching passage instead.
                // Otherwise (the match was only in the excerpt): show the excerpt.
                string description;
                if (inTitle || inTags) description = excerpt;
                else if (inBody) description = Snippet(body, terms);
                else description = excerpt;

                if (string.IsNullOrWhiteSpace(description) && body.Length > 0)
                    description = body.Length > 160 ? body[..157] + "…" : body;

                yield return new SearchResult
                {
                    Kind = ResultKind.Post,
                    Title = title,
                    Url = p.Url,
                    ImageUrl = p.FeaturedImageUrl,
                    ImageAlt = p.ImageAltText,
                    Label = p.Category,
                    Date = p.PublishedDate,
                    Tags = tags,
                    Description = description,
                    Score = score
                };
            }
        }

        private static string Snippet(string body, string[] terms)
        {
            // Centre on whichever search word appears first in the content
            var i = terms.Select(t => body.IndexOf(t, OIC)).Where(x => x >= 0).DefaultIfEmpty(-1).Min();
            if (i < 0) return "";

            var start = Math.Max(0, i - 70);
            var end = Math.Min(body.Length, i + 90);

            // Snap to word boundaries so words aren't cut in half
            if (start > 0)
            {
                var s = body.IndexOf(' ', start);
                if (s >= 0 && s < i) start = s + 1;
            }
            if (end < body.Length)
            {
                var e = body.LastIndexOf(' ', end - 1);
                if (e > i) end = e;
            }

            return (start > 0 ? "… " : "") + body[start..end].Trim() + (end < body.Length ? " …" : "");
        }

    }
}