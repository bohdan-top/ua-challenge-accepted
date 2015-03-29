using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security;
using System.Xml;

namespace SitemapGenerator.Infrastructure
{
	class SiteMapModel
	{
		XmlDocument Doc { get; set; }
		protected List<string> addedUrls;
		string hostName;

		public string FilePath { get; private set; }
		public string FileName { get; private set; }

		public int AddedCount
		{
			get
			{
				return addedUrls.Count;
			}
		}

		public SiteMapModel(string hostName)
		{
			this.hostName = hostName.Replace(".", String.Empty);
			InitXml();
			addedUrls = new List<string>();
		}

		public void AddUrls(List<string> urls)
		{
			Doc.Load(FilePath);
			foreach (var u in urls)
			{
				AddNode(u, DateTime.Now);
			}

			Save();
		}

		public void AddNode(string url, DateTime? lastMod = null, string changeFreq = null, double? priority = null)
		{
			url = SecurityElement.Escape(url);

			if (!addedUrls.Contains(url))
			{

				var urlNode = Doc.CreateElement("url", Doc.DocumentElement.NamespaceURI);

				var linkNode = Doc.CreateElement("link", Doc.DocumentElement.NamespaceURI);
				linkNode.InnerText = url;
				urlNode.AppendChild(linkNode);

				if (lastMod.HasValue)
				{
					var lastModNode = Doc.CreateElement("lastmod", Doc.DocumentElement.NamespaceURI);
					lastModNode.InnerText = lastMod.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz");
					urlNode.AppendChild(lastModNode);
				}

				if (changeFreq != null)
				{
					var changefreqNode = Doc.CreateElement("changefreq", Doc.DocumentElement.NamespaceURI);
					changefreqNode.InnerText = changeFreq;
					urlNode.AppendChild(changefreqNode);
				}

				if (priority.HasValue)
				{
					var priorityNode = Doc.CreateElement("priority", Doc.DocumentElement.NamespaceURI);
					priorityNode.InnerText = priority.Value.ToString("0.00", CultureInfo.InvariantCulture);
					urlNode.AppendChild(priorityNode);
				}

				Doc.DocumentElement.AppendChild(urlNode);

				addedUrls.Add(url);
			}
		}

		private void InitXml()
		{
			Doc = new XmlDocument();

			XmlDeclaration xmlDeclaration = Doc.CreateXmlDeclaration("1.0", "UTF-8", null);
			Doc.AppendChild(xmlDeclaration);

			XmlElement root = Doc.CreateElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");
			Doc.AppendChild(root);
			Save();
		}

		public void Save()
		{
			if (String.IsNullOrEmpty(FilePath))
			{
				FileName = String.Format("{0}_{1}", hostName, DateTime.Now.ToFileTimeUtc());
				var name = String.Format("{0}.xml", FileName);
				FilePath = Path.Combine(System.Web.HttpContext.Current.Server.MapPath(@"~/App_Data"), name);
			}
			Doc.Save(FilePath);			
		}

		public static string FullPathByName(string fileName)
		{
			return Path.Combine(System.Web.HttpContext.Current.Server.MapPath(@"~/App_Data"), fileName + ".xml");
		}
	}
}