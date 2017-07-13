using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RepoCrawler
{
    public class Repository
    {
        public Repository(string id, string addonListUrl, string checksumUrl, string dataDirUrl)
        {
            this.Id = id;
            this.AddonListUrl = addonListUrl;
            this.ChecksumUrl = checksumUrl;
            this.DataDirUrl = dataDirUrl;
        }

        public string Id { get; }

        public string AddonListUrl { get; }

        public string ChecksumUrl { get; }

        public string DataDirUrl { get; }

        public async static Task<Repository> Load(string path)
        {
            using (FileStream zipFile = File.OpenRead(path))
            {
                ZipArchive zip = new ZipArchive(zipFile);

                ZipArchiveEntry addonZipEntry = zip.Entries.First(entry => entry.Name == "addon.xml");

                using (Stream addon = addonZipEntry.Open())
                using (TextReader reader = new StreamReader(addon))
                {
                    XDocument xmlRoot = XDocument.Parse(await reader.ReadToEndAsync());

                    XElement repositoryExtension = xmlRoot.Element("addon").Elements("extension").First(element => (string)element.Attribute("point") == "xbmc.addon.repository");

                    return new Repository(
                        id: (string)xmlRoot.Element("addon").Attribute("id"),
                        addonListUrl: repositoryExtension.Element("info").Value,
                        checksumUrl: repositoryExtension.Element("checksum").Value,
                        dataDirUrl: repositoryExtension.Element("datadir").Value);
                }
            }
        }

        public static IReadOnlyList<Addon> ParseAddons(string addonsXml, Repository repository)
        {
            XDocument xml = XDocument.Parse(addonsXml);

            return xml.Element("addons").Elements("addon")
                .Select(element => new Addon(
                    id: (string)element.Attribute("id"),
                    version: (string)element.Attribute("version"),
                    repository: repository,
                    xmlDefinition: element))
                .ToList()
                .AsReadOnly();
        }

        public async Task<IReadOnlyList<Addon>> ListAddons()
        {
            HttpClient client = new HttpClient();

            HttpResponseMessage responseMessage = await client.GetAsync(this.AddonListUrl);

            return ParseAddons(await responseMessage.EnsureSuccessStatusCode().Content.ReadAsStringAsync(), this);
        }
    }
}
