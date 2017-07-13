namespace RepoCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            LibraryManager manager = new LibraryManager("repositories", "addons");

            manager.Start().Wait();
        }
    }
}