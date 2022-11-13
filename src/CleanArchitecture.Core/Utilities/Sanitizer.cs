using Ganss.XSS;
using System.Net;
using System.Text;

namespace CleanArchitecture.Core.Utilities
{
    public static class Sanitizer
    {
        public static string StripHtml(string html)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            if (doc.DocumentNode == null || doc.DocumentNode.ChildNodes == null)
            {
                return WebUtility.HtmlDecode(html);
            }

            var sb = new StringBuilder();
            var i = 0;

            foreach (var node in doc.DocumentNode.ChildNodes)
            {
                var text = node.InnerText?.Trim();

                if (!string.IsNullOrEmpty(text))
                {
                    sb.Append(text);

                    if (i < doc.DocumentNode.ChildNodes.Count - 1)
                    {
                        sb.Append(Environment.NewLine);
                    }
                }

                i++;
            }

            return WebUtility.HtmlDecode(sb.ToString());
        }

        public static string WrapHtml(string text)
        {
            text = WebUtility.HtmlEncode(text);
            text = text.Replace("\r\n", "\r");
            text = text.Replace("\n", "\r");
            text = text.Replace("\r", "<br>\r\n");
            text = text.Replace("  ", " &nbsp;");
            return $"<p>{text.Trim()}</p>";
        }

        public static string SanitizeHtml(string html)
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedAttributes.Add("class");
            sanitizer.AllowDataAttributes = true;
            sanitizer.AllowedSchemes.Add("data");

            html = sanitizer.Sanitize(html).Trim();

            return html;
        }
    }
}
