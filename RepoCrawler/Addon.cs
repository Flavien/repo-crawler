using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RepoCrawler
{
    public class Addon
    {
        public Addon(string id, string version, Repository repository, XElement xmlDefinition)
        {
            this.Id = id;
            this.Version = version;
            this.Repository = repository;
            this.XmlDefinition = xmlDefinition;
        }

        public string Id { get; }

        public string Version { get; }

        public Repository Repository { get; }

        public XElement XmlDefinition { get; }

        public async Task Download(string addonDirectory)
        {
            HttpClient client = new HttpClient();

            Directory.CreateDirectory(Path.Combine(addonDirectory, Id));

            using (Stream addonStream = await client.GetStreamAsync($"{Repository.DataDirUrl}/{Id}/{Id}-{Version}.zip"))
            using (FileStream output = File.OpenWrite(Path.Combine(addonDirectory, Id, $"{Id}-{Version}.zip")))
            {
                await addonStream.CopyToAsync(output);
            }
        }
    }
}
