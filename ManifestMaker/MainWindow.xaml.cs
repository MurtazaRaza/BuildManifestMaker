using System.IO;
using System.Windows;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ManifestMaker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string platformName = PlatformNameComboBox.Text;
            string deploymentName = DeploymentNameTextBox.Text;
            string path = PathTextBox.Text;

            DirectoryInfo d = new DirectoryInfo(path); //Assuming Test is your Folder

            FileInfo[] Files;
            try
            {
                Files = d.GetFiles("*.pak"); //Getting Text files
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
            
            List<FileInfo> ListOfFiles = Files.Where(file => !file.Name.Contains("pakchunk0")).ToList();

            string textFilePath = path + $"/BuildManifest-{platformName}.txt";
            // Check if file already exists. If yes, delete it.     
            if (File.Exists(textFilePath))
            {
                File.Delete(textFilePath);
            }

            // Create a new file     
            using (StreamWriter sw = File.CreateText(textFilePath))
            {
                sw.WriteLine($"$NUM_ENTRIES = {ListOfFiles.Count}");
                sw.WriteLine($"$BUILD_ID = {deploymentName}");
                foreach(var file in ListOfFiles)
                {
                    string pakFileName = file.Name;
                    string pakFileSize = file.Length.ToString();
                    string pakFileVersion = "ver001";

                    string toBeSearched = "pakchunk";
                    string substringedIntermediate = file.Name.Substring(file.Name.IndexOf(toBeSearched) + toBeSearched.Length);
                    List<string> escapeCharacters = new List<string>()
                    {
                        "-",
                        "_"
                    };
                    string pakFileChunkId = GetUntilOrEmpty(substringedIntermediate, escapeCharacters);

                    string pakFilePathRelative = $"/{platformName}/{file.Name}";
                    sw.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", pakFileName, pakFileSize, pakFileVersion, pakFileChunkId, pakFilePathRelative);
                }
            }
        }

        public string GetUntilOrEmpty(string text, List<string> escapeCharacters)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            
            int charLocation = -1;

            foreach (var stopAt in escapeCharacters)
            {
                int currentEscapeCharacterIndex = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (currentEscapeCharacterIndex == 0) continue;
                    
                if (charLocation == -1)
                {
                    charLocation = currentEscapeCharacterIndex;
                    continue;
                }

                if (charLocation > currentEscapeCharacterIndex)
                {
                    charLocation = currentEscapeCharacterIndex;
                    continue;
                }
            }

            return charLocation > -1 ? text.Substring(0, charLocation) : string.Empty;
        }
    }
}