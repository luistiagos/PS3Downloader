namespace PS3_Downloader
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Net;

    class NPS
    {
        private ProgressBar progressBar;
        private static string[] REGIONS = { "us", "eu", "jp", "asia" };
        private static string GAMES_TSV_FILENAME = "{0}_GAMES.tsv";
        private static string GAMES_TSV_URL = "https://nopaystation.com/tsv/{0}_GAMES.tsv";
        private static string PKG2ZIP = "pkg2zip";

        private static Dictionary<string, Dictionary<string, Dictionary<string, object>>> PLATFORMS = new Dictionary<string, Dictionary<string, Dictionary<string, object>>>
    {
        {
            "psp", new Dictionary<string, Dictionary<string, object>>
            {
                {
                    "games", new Dictionary<string, object>
                    {
                        { "filename", string.Format(GAMES_TSV_FILENAME, "PSP") },
                        { "url", string.Format(GAMES_TSV_URL, "PSP") },
                        { "fieldnames", new string[] { "title_id", "region", "type", "name", "pkg_url", "content_id", "last_modification_date", "rap", "download_rap_file", "file_size", "sha256" } }
                    }
                }
            }
        },
        {
            "psx", new Dictionary<string, Dictionary<string, object>>
            {
                {
                    "games", new Dictionary<string, object>
                    {
                        { "filename", string.Format(GAMES_TSV_FILENAME, "PSX") },
                        { "url", string.Format(GAMES_TSV_URL, "PSX") },
                        { "fieldnames", new string[] { "title_id", "region", "name", "pkg_url", "content_id", "last_modification_date", "original_name", "file_size", "sha256" } }
                    }
                }
            }
        },
        {
            "psv", new Dictionary<string, Dictionary<string, object>>
            {
                {
                    "games", new Dictionary<string, object>
                    {
                        { "filename", string.Format(GAMES_TSV_FILENAME, "PSV") },
                        { "url", string.Format(GAMES_TSV_URL, "PSV") },
                        { "fieldnames", new string[] { "title_id", "region", "name", "pkg_url", "zrif", "content_id", "last_modification_date", "original_name", "file_size", "sha256" } }
                    }
                }
            }
        },
        {
            "ps3", new Dictionary<string, Dictionary<string, object>>
            {
                {
                    "games", new Dictionary<string, object>
                    {
                        { "filename", string.Format(GAMES_TSV_FILENAME, "PS3") },
                        { "url", string.Format(GAMES_TSV_URL, "PS3") },
                        { "fieldnames", new string[] { "title_id", "region", "name", "pkg_url", "zrif", "content_id", "last_modification_date", "original_name", "file_size", "sha256" } }
                    }
                }
            }
        }
    };

        public static string FormatSize(string num, string suffix = "B")
        {
            Double size = Double.Parse(num);
            string[] units = { "", "Ki", "Mi", "Gi", "Ti", "Pi", "Ei", "Zi" };

            foreach (string unit in units)
            {
                if (Math.Abs(size) < 1024.0)
                {
                    return $"{size:0.0}{unit}{suffix}";
                }

                size /= 1024.0;
            }

            return $"{size:0.0}Yi{suffix}";
        }

        private static string FormatItem(Dictionary<string, string> item)
        {
            return $"[{item["region"]}-{item["title_id"]}] {item["name"]} ({FormatSize(item["file_size"])})";
        }

        public static List<Dictionary<string, string>> LoadItems(string platform)
        {
            string filename = (string)PLATFORMS[platform]["games"]["filename"];
            string url = (string)PLATFORMS[platform]["games"]["url"];
            string[] fieldnames = (string[])PLATFORMS[platform]["games"]["fieldnames"];
            List<Dictionary<string, string>> listItems = new List<Dictionary<string, string>>();
            List<string> fileNames = GetFileNames();

            if (!File.Exists(filename))
            {
                Console.WriteLine("Downloading database");
                new WebClient().DownloadFile(url, filename);
            }

            using (StreamReader reader = new StreamReader(filename))
            {
                reader.ReadLine(); // Skip header
                while (!reader.EndOfStream)
                {
                    string[] values = reader.ReadLine().Split('\t');
                    Dictionary<string, string> item = new Dictionary<string, string>();
                    for (int i = 0; i < fieldnames.Length; i++)
                    {
                        item[fieldnames[i]] = (string)values[i];
                    }

                    if (item["pkg_url"] != "MISSING")
                    {
                        //listItems.Add(item);
                        if (SearchFiles(fileNames, item["title_id"]))
                        {
                            item["name"] = ((string)item["name"]).Trim();
                            listItems.Add(item);
                        }
                        
                    }
                }
            }

            List<Dictionary<string, string>> sortedData = listItems.OrderBy(x => x["name"]).ToList();
            return sortedData;
        }

        public static async Task DownloadSelectedItem(string titleId)
        {
            List<Dictionary<string, string>> items = NPS.LoadItems("ps3");

            Dictionary<string, string> selectedItem = items.Find(item => item["title_id"] == titleId);

            if (selectedItem != null)
            {
                using (ProgressForm progressForm = new ProgressForm())
                {
                    // Show the progress form
                    progressForm.Show();

                    // Download the selected item
                    await DownloadItem(selectedItem, (e) =>
                    {
                        // Update progress in the progress form
                        progressForm.UpdateProgress(e);
                    }, "./games");

                    // Close the progress form when download is complete
                    progressForm.Close();
                }
            }
        }

        public static async Task DownloadItem(Dictionary<string, string> item, Action<DownloadProgressChangedEventArgs> progressCallback, string outputPath = ".")
        {
             Console.WriteLine($"Downloading: {FormatItem(item)}\n");
            //new WebClient().DownloadFile(item["pkg_url"], Path.Combine(outputPath, item["name"] + ".pkg"));

             using (WebClient client = new WebClient())
             {
                client.DownloadProgressChanged += (sender, e) =>
                {
                    //DownloadInfo info = new DownloadInfo(e.ProgressPercentage, CalculateDownloadSpeed(e.BytesReceived, e.TotalBytesToReceive));
                    // Report progress
                    progressCallback?.Invoke(e);

                    //progressCallback?.Invoke(e.ProgressPercentage);
                };

                await client.DownloadFileTaskAsync(item["pkg_url"], Path.Combine(outputPath, item["name"] + ".pkg"));
             }

        }

        static bool SearchFiles(List<string> fileNames, string searchString)
        {

            try
            {
                foreach (string file in fileNames)
                {
                    if (file.Contains(searchString))
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }

            return false;
        }


        static List<string> GetFileNames()
        {

            string folderPath = ".\\dev_hdd0\\home\\00000001\\exdata\\";
            List<string> fileNames = new List<string>();

            try
            {
                foreach (string file in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
                {
                    String name = file.Substring(file.LastIndexOf("\\")+1);
                    fileNames.Add(name);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }

            return fileNames;
        }


    }


}
