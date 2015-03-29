using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace SitemapGenerator.Infrastructure
{
	class Robot
	{
		Uri TargetUrl { get; set; }
		WebClient WebClient { get; set; }
		SiteMapModel SiteMap { get; set; }

		List<string> visitedUrls;

		public string ResultFileName
		{
			get
			{
				return SiteMap.FileName;
			}
		}

		private List<string> pendingUrls;

		public Robot(Uri uri)
		{
			TargetUrl = uri;

			WebClient = new WebClient();
			WebClient.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2272.101 Safari/537.36";

			SiteMap = new SiteMapModel(uri.Host.Replace("www.", String.Empty));
			pendingUrls = new List<string>();
			visitedUrls = new List<string>();
		}

		public void Work()
		{
			for (int i = 0; i < 3; i++)
			{
				if (i == 0)
				{
					Process(TargetUrl);
				}
				else
				{
					var targetUrls = pendingUrls;
					pendingUrls = new List<string>();

					foreach (var t in targetUrls)
					{
						var targUrl = t;
						if (targUrl.IndexOf("#") >= 0)
						{
							targUrl = targUrl.Remove(targUrl.IndexOf("#"));
						}

						if (!visitedUrls.Contains(targUrl))
						{
							Process(new Uri(targUrl));
							visitedUrls.Add(targUrl);
						}
					}
				}

				if (SiteMap.AddedCount >= 50000 || new FileInfo(SiteMapModel.FullPathByName(ResultFileName)).Length >=10485760)
					break;
			}
		}

		public void Process(Uri url)
		{
			var doc = GetHtmlDoc(url);

			if (doc != null)
			{
				var baseUri = new Uri(TargetUrl.GetLeftPart(UriPartial.Authority));
				var bodyNode = doc.DocumentNode.SelectSingleNode("//body");

				if (bodyNode != null)
				{
					var linkNodes = bodyNode.SelectNodes(".//a");

					if (linkNodes != null)
					{
						foreach (var node in linkNodes)
						{
							var hrefValue = node.GetAttributeValue("href", null);
							if (!String.IsNullOrWhiteSpace(hrefValue) && !hrefValue.ToLower().Contains("javascipt:"))
							{
								var uri = GetAbsoluteUrl(baseUri, hrefValue);

								if (uri != null && CheckSameDomains(baseUri, uri))
								{
									var u = uri.ToString();
									if (u.IndexOf("#") >= 0)
										u = u.Remove(u.IndexOf("#"));
									pendingUrls.Add(u);
								}
							}
						}

						SiteMap.AddUrls(pendingUrls);
					}
				}
			}
		}

		public bool CheckSameDomains(Uri uri1, Uri uri2)
		{
			var domain1 = uri1.Host.Replace("www.", String.Empty);
			var domain2 = uri2.Host.Replace("www.", String.Empty);
			return domain1.ToLower() == domain2.ToLower();
		}


		public HtmlAgilityPack.HtmlDocument GetHtmlDoc(Uri url)
		{
			HtmlAgilityPack.HtmlDocument doc = null;

			try
			{
				doc = new HtmlAgilityPack.HtmlDocument();
				var page = WebClient.DownloadString(url);
				if (!WebClient.ResponseHeaders["content-type"].ToLower().Contains("text/html"))
				{
					return null;
				}

				doc.LoadHtml(page);
				return doc;
			}
			catch (Exception ex)
			{
				return null;
			}
		}

		static Uri GetAbsoluteUrl(Uri baseUrl, string url)
		{
			Uri rez = null;

			if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out rez))
			{
				if (!rez.IsAbsoluteUri)
					rez = new Uri(new Uri(baseUrl.ToString()), rez);
			}
			return rez;
		}
	}
}