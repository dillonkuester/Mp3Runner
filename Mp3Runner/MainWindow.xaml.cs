using System.IO;
using System.Windows;
using System.Windows.Forms;
using NAudio.Wave;
using Mp3Runner.Classes;

namespace Mp3Runner
{
    /// <summary>
    /// Mp3 Runner Main Window
    /// </summary>
    public partial class MainWindow : Window
    {

        private List<string> mp3Files = new List<string>();
        private IWavePlayer waveOut;
        private AudioFileReader audioFileReader;
        private bool isLogAlternate = false;
        private bool isClientIdVisible = false;
        private bool isClientSecretVisible = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateShowButtonVisibility();
            ShowOrHideClientIdAndClientSecret();
        }

        private void ShowOrHideClientIdAndClientSecret()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.ClientId))
            {
                clientIdInput.Visibility = Visibility.Collapsed;
                clientIdLabel.Visibility = Visibility.Visible;  
                clientIdLabel.Text = new string('*', Properties.Settings.Default.ClientId.Length); 
            }
            else
            {
                clientIdInput.Visibility = Visibility.Visible;   
                clientIdLabel.Visibility = Visibility.Collapsed; 
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.ClientSecret))
            {
                clientSecretInput.Visibility = Visibility.Collapsed; 
                clientSecretLabel.Visibility = Visibility.Visible;  
                clientSecretLabel.Text = new string('*', Properties.Settings.Default.ClientSecret.Length);
            }
            else
            {
                clientSecretInput.Visibility = Visibility.Visible;
                clientSecretLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void UpdateShowButtonVisibility()
        {
            bool isClientIdEmpty = string.IsNullOrWhiteSpace(clientIdInput.Text);
            bool isClientSecretEmpty = string.IsNullOrWhiteSpace(clientSecretInput.Text);

            ToggleClientIdVisibility.Visibility = isClientIdEmpty ? Visibility.Collapsed : Visibility.Visible;
            ToggleClientSecretVisibility.Visibility = isClientSecretEmpty ? Visibility.Collapsed : Visibility.Visible;
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string clientId = clientIdInput.Text;
            string clientSecret = clientSecretInput.Text;

            Properties.Settings.Default.ClientId = clientId;
            Properties.Settings.Default.ClientSecret = clientSecret;
            Properties.Settings.Default.Save();

            HideClientIdAndClientSecretOnSave(clientId, clientSecret);
        }

        private void HideClientIdAndClientSecretOnSave(string clientId, string clientSecret)
        {
            clientIdInput.Visibility = Visibility.Collapsed;
            clientIdLabel.Visibility = Visibility.Visible;
            clientIdLabel.Text = new string('*', clientId.Length);

            clientSecretInput.Visibility = Visibility.Collapsed;
            clientSecretLabel.Visibility = Visibility.Visible;
            clientSecretLabel.Text = new string('*', clientSecret.Length);
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ClientId = string.Empty;
            Properties.Settings.Default.ClientSecret = string.Empty;
            Properties.Settings.Default.Save();
            ResetClientIdAndClientSecretVisibility();
        }

        private void ResetClientIdAndClientSecretVisibility()
        {
            clientIdInput.Visibility = Visibility.Visible;
            clientIdInput.Text = string.Empty;
            clientIdLabel.Visibility = Visibility.Collapsed;

            clientSecretInput.Visibility = Visibility.Visible;
            clientSecretInput.Text = string.Empty;
            clientSecretLabel.Visibility = Visibility.Collapsed;
        }


        private void ToggleClientIdVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (isClientIdVisible)
            {
                clientIdLabel.Text = new string('*', Properties.Settings.Default.ClientId.Length);
                ToggleClientIdVisibility.Content = "Show";
            }
            else
            {
                clientIdLabel.Text = Properties.Settings.Default.ClientId;
                ToggleClientIdVisibility.Content = "Hide";
            }
            isClientIdVisible = !isClientIdVisible;
        }

        private void ToggleClientSecretVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (isClientSecretVisible)
            {
                clientSecretLabel.Text = new string('*', Properties.Settings.Default.ClientSecret.Length);
                ToggleClientSecretVisibility.Content = "Show";
            }
            else
            {
                clientSecretLabel.Text = Properties.Settings.Default.ClientSecret;
                ToggleClientSecretVisibility.Content = "Hide";
            }
            isClientSecretVisible = !isClientSecretVisible;
        }

        private async void btnRename_Click(object sender, RoutedEventArgs e)
        {
            if (mp3Files == null || mp3Files.Count == 0)
            {
                System.Windows.MessageBox.Show("No MP3 files loaded.");
                return;
            }

            int totalFiles = mp3Files.Count;
            int renamedFiles = 0;
            progressBar.Maximum = totalFiles;

            foreach (var filePath in mp3Files)
            {
                var trackInfo = await SpotifyTrackAnalyzer.AnalyzeAndFetchSpotifyInfo(filePath);
                var (bpm, key) = trackInfo;

                if (!string.IsNullOrEmpty(bpm) && !string.IsNullOrEmpty(key))
                {
                    RenameFileWithBpmAndKey(filePath, (bpm, key)); 
                }
                else
                {
                    logListBox.Items.Add($"Failed to retrieve BPM or Key for the track: {filePath}");
                }

                renamedFiles++;
                progressBar.Value = renamedFiles;
                progressText.Text = $"{renamedFiles} of {totalFiles} files renamed";
                logListBox.Items.Add($"Processed {renamedFiles}/{totalFiles} files.");
                await Task.Delay(50);
            }
            System.Windows.MessageBox.Show("File renaming complete!");

        }

        private void RenameFileWithBpmAndKey(string filePath, (string bpm, string key) trackInfo)
        {
            var directory = Path.GetDirectoryName(filePath);
            var originalFileName = Path.GetFileName(filePath);

            var cleanedBpm = Helpers.CleanFileName(trackInfo.bpm);
            var cleanedKey = Helpers.CleanFileName(trackInfo.key);

            var fileNameWithoutBpmKey = SpotifyTrackAnalyzer.ExtractTrackName(originalFileName);

            var newFileName = $"{cleanedBpm}_{cleanedKey}_{fileNameWithoutBpmKey}";

            if (originalFileName.StartsWith($"{cleanedBpm}_{cleanedKey}_"))
            {
                logListBox.Items.Add($"Skipping file: {filePath} (already named with BPM and Key)");
                return;
            }

            var newFilePath = Path.Combine(directory, newFileName);

            logListBox.Items.Add($"Renaming file to: {newFilePath}");

            try
            {
                File.Move(filePath, newFilePath);
            }
            catch (Exception ex)
            {
                logListBox.Items.Add($"Failed to rename file: {filePath}. Error: {ex.Message}");
            }
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            waveOut?.Stop();
            waveOut?.Dispose();
            audioFileReader?.Dispose();
        }

        private void btnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtFolderPath.Text = dialog.SelectedPath;
                    LoadMp3Files(dialog.SelectedPath);
                }
            }
        }

        private void LoadMp3Files(string folderPath)
        {
            var files = Directory.GetFiles(folderPath);
            mp3Files = files.Where(f => f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)).ToList();
            lstFiles.ItemsSource = mp3Files.Select(Path.GetFileName).ToList();
        }
    }
}