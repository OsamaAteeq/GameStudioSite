using HtmlAgilityPack;

namespace GameStudioSite.Services
{
    public static class EmbedProcessor
    {
        public static string Process(string? html)
        {
            if (string.IsNullOrWhiteSpace(html)) return html ?? "";

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var iframes = doc.DocumentNode.SelectNodes("//iframe");
            if (iframes is null) return html;

            foreach (var f in iframes.ToList())
            {
                // The facade may already have moved the address to data-src
                var attr = f.Attributes.Contains("data-src") ? "data-src" : "src";
                var src = f.GetAttributeValue(attr, "").Trim();
                if (src.StartsWith("//")) src = "https:" + src;
                if (!Uri.TryCreate(src, UriKind.Absolute, out var uri)) continue;

                var host = uri.Host.ToLowerInvariant();
                string defaultTitle = "Embedded content";

                if ((host == "youtube.com" || host == "www.youtube.com" || host == "m.youtube.com")
                    && uri.AbsolutePath.StartsWith("/embed/", StringComparison.OrdinalIgnoreCase))
                {
                    var b = new UriBuilder(uri) { Scheme = "https", Port = -1, Host = "www.youtube-nocookie.com" };
                    f.SetAttributeValue(attr, b.Uri.AbsoluteUri);
                    defaultTitle = "YouTube video player";
                }
                else if (host == "www.youtube-nocookie.com" || host == "youtube-nocookie.com")
                {
                    defaultTitle = "YouTube video player";
                }
                else if (host == "itch.io" || host.EndsWith(".itch.io") || host.EndsWith(".itch.zone"))
                {
                    defaultTitle = uri.AbsolutePath.StartsWith("/embed-upload/", StringComparison.OrdinalIgnoreCase)
                        ? "Play the game" : "Game on itch.io";
                }

                // Applied to every embed
                if (!f.Attributes.Contains("loading"))
                    f.SetAttributeValue("loading", "lazy");
                if (!f.Attributes.Contains("referrerpolicy"))
                    f.SetAttributeValue("referrerpolicy", "strict-origin-when-cross-origin");
                if (string.IsNullOrWhiteSpace(f.GetAttributeValue("title", "")))
                    f.SetAttributeValue("title", defaultTitle);
            }

            return doc.DocumentNode.OuterHtml;
        }
    }
}