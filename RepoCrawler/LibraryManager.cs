using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RepoCrawler
{
    public class LibraryManager
    {
        private static readonly string addonsManifest = "addons.xml";

        private readonly string repositoryDirectory;
        private readonly string addonDirectory;

        public LibraryManager(string repositoryDirectory, string addonDirectory)
        {
            this.repositoryDirectory = repositoryDirectory;
            this.addonDirectory = addonDirectory;
        }

        public async Task Start()
        {
            Dictionary<string, Addon> currentAddons = await GetCurrentAddons();

            foreach (string repositoryFileName in Directory.EnumerateFiles(this.repositoryDirectory))
            {
                try
                {
                    Repository repository = await Repository.Load(repositoryFileName);

                    Console.WriteLine($"Crawling {repository.Id}");

                    IReadOnlyList<Addon> addons = await repository.ListAddons();

                    foreach (Addon addon in addons)
                    {
                        if (IsNew(addon, currentAddons))
                        {
                            await DownloadAddon(addon);
                            currentAddons[addon.Id] = addon;
                        }
                        else
                        {
                            Console.WriteLine($"  Skipping {addon.Id}-{addon.Version}");
                        }
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Error loading {repositoryFileName}: {exception.Message}");
                }

                Console.WriteLine();
            }

            await WriteRepository(currentAddons.Values);
        }

        private bool IsNew(Addon addon, IReadOnlyDictionary<string, Addon> currentAddons)
        {
            if (currentAddons.TryGetValue(addon.Id, out Addon existingAddon))
            {
                try
                {
                    Version currentVersion = Version.Parse(existingAddon.Version);
                    Version newVersion = Version.Parse(addon.Version);

                    return newVersion > currentVersion;
                }
                catch (FormatException)
                {
                    return StringComparer.OrdinalIgnoreCase.Compare(addon.Version, existingAddon.Version) > 0;
                }
            }
            else
            {
                return true;
            }
        }

        private async Task<Dictionary<string, Addon>> GetCurrentAddons()
        {
            string filePath = Path.Combine(addonDirectory, addonsManifest);

            if (!File.Exists(filePath))
                return new Dictionary<string, Addon>(0);

            using (StreamReader reader = File.OpenText(filePath))
            {
                return Repository.ParseAddons(await reader.ReadToEndAsync(), null)
                    .ToDictionary(item => item.Id, item => item);
            }
        }

        private async Task DownloadAddon(Addon addon)
        {
            try
            {
                Console.Write($"  Downloading {addon.Id}-{addon.Version}");
                await addon.Download(addonDirectory);
                Console.WriteLine($" [OK]");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"\n  Error downloading {addon.Id}-{addon.Version}: {exception.Message}");
            }
        }

        private async Task WriteRepository(IEnumerable<Addon> downloadedAddons)
        {
            XDocument xml = new XDocument(new XElement("addons",
                downloadedAddons
                    .OrderBy(addon => addon.Id, StringComparer.OrdinalIgnoreCase)
                    .Select(addon => addon.XmlDefinition)));

            using (FileStream addonsFile = File.OpenWrite(Path.Combine(addonDirectory, addonsManifest)))
            using (TextWriter writer = new StreamWriter(addonsFile))
            {
                await writer.WriteAsync(xml.ToString());
            }
        }
    }
}
