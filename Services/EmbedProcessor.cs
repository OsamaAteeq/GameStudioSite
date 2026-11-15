using HtmlAgilityPack;

namespace GameStudioSite.Services
{
    public static class EmbedProcessor
    {
        public static string Process(string? html, bool firstImageEager = false)
        {
            if (string.IsNullOrWhiteSpace(html)) return html ?? "";

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var images = doc.DocumentNode.SelectNodes("//img");
            var iframes = doc.DocumentNode.SelectNodes("//iframe");
            if (images is null && iframes is null) return html;

            if (images != null)
            {
                var firstBlock = doc.DocumentNode.ChildNodes.FirstOrDefault(n =>
                    n.NodeType == HtmlNodeType.Element || !string.IsNullOrWhiteSpace(n.InnerText));
                var isFirst = true;

                foreach (var img in images)
                {
                    var eager = firstImageEager && isFirst;
                    isFirst = false;

                    if (eager)
                    {
                        // Opens the article with no hero above it: it can be the LCP element, so don't lazy-load it.
                        if (firstBlock != null && (firstBlock == img || firstBlock.Descendants("img").Contains(img)))
                            img.SetAttributeValue("fetchpriority", "high");
                    }
                    else if (!img.Attributes.Contains("loading"))
                    {
                        img.SetAttributeValue("loading", "lazy");
                    }

                    if (!img.Attributes.Contains("decoding")) img.SetAttributeValue("decoding", "async");
                    if (!img.Attributes.Contains("alt")) img.SetAttributeValue("alt", "");
                }
            }
            if (iframes != null)
            {
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
            }

            return doc.DocumentNode.OuterHtml;
        }
    }
}