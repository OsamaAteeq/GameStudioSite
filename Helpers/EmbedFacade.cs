using System.Text.RegularExpressions;

namespace GameStudioSite.Helpers
{
    public static class EmbedFacade
    {
        private static readonly Regex IframeTag =
            new(@"<iframe\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SrcAttribute =
            new(@"\ssrc\s*=\s*([""'])(?<url>.*?)\1", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        // Turns <iframe src="..."> into <iframe data-facade="true" data-src="...">
        // for itch.io embeds, so the browser doesn't start loading the game.
        public static string Apply(string html)
        {
            return IframeTag.Replace(html, match =>
            {
                var tag = match.Value;

                var src = SrcAttribute.Match(tag);
                if (!src.Success) return tag;

                if (!Uri.TryCreate(src.Groups["url"].Value, UriKind.Absolute, out var uri)) return tag;

                var host = uri.Host;
                var path = uri.AbsolutePath;

                // Only the playable HTML5 embeds get the click-to-play box.
                // The "Play on itch.io" widget (itch.io/embed/ID) is left alone.
                var isPlayableGame =
                    (host == "itch.io" && path.StartsWith("/embed-upload/", StringComparison.OrdinalIgnoreCase))
                    || host.EndsWith(".itch.zone");

                if (!isPlayableGame) return tag;

                tag = SrcAttribute.Replace(tag,
                    m => $" data-src={m.Groups[1].Value}{m.Groups["url"].Value}{m.Groups[1].Value}", 1);

                // tag starts with "<iframe" (7 characters)
                return "<iframe data-facade=\"true\"" + tag.Substring(7);
            });
        }
        private static readonly Regex TrailingEmptyParagraphs =
    new(@"(\s*<p>(<br\s*/?>|\s|&nbsp;)*</p>)+\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string TrimTrailingEmpty(string html)
        {
            return TrailingEmptyParagraphs.Replace(html, "");
        }
    }
}