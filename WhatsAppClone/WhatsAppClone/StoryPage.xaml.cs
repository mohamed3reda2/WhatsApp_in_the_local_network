using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Controls;

namespace WhatsAppClone
{
    public partial class StoryPage : Window
    {
        private WhatsAppCloneEntities _context = new WhatsAppCloneEntities();
        private int _currentUserID;

        public StoryPage(int userID)
        {
            InitializeComponent();
            _currentUserID = userID;
            LoadUsers();  
            LoadStories();  
        }

        
        private void LoadUsers()
        {
            var users = _context.Users.Select(u => new { u.UserID, u.Username }).ToList();
            UsersComboBox.ItemsSource = users;
            UsersComboBox.DisplayMemberPath = "Username";
            UsersComboBox.SelectedValuePath = "UserID";
        }


        private void LoadStories()
        {
            var stories = _context.Stories
                .Where(s => s.UserID != _currentUserID && s.Expiration > DateTime.Now)
                .ToList();

            StoriesList.Items.Clear();

            foreach (var story in stories)
            {
                if (story.Type == "Image")
                {
                    var image = new Image();
                    var bitmap = new BitmapImage();
                    using (var stream = new MemoryStream(story.Content))
                    {
                        bitmap.BeginInit();
                        bitmap.StreamSource = stream;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                    }
                    image.Source = bitmap;
                    StoriesList.Items.Add(image);
                }
                else if (story.Type == "Video")
                {
                    var mediaElement = new MediaElement();
                    var stream = new MemoryStream(story.Content);
                    string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".mp4");

                    
                    File.WriteAllBytes(tempFilePath, stream.ToArray());

                    mediaElement.Source = new Uri(tempFilePath);
                    mediaElement.LoadedBehavior = MediaState.Manual;
                    mediaElement.Play();
                    StoriesList.Items.Add(mediaElement);

                
                    mediaElement.MediaEnded += (obj, args) =>
                    {
                        File.Delete(tempFilePath);  
                    };
                }
            }
        }

    
        private void UploadStoryButton_Click(object sender, RoutedEventArgs e)
        {
         
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png|Video Files|*.mp4;*.avi";

            if (fileDialog.ShowDialog() == true)
            {
                string filePath = fileDialog.FileName;  

            
                byte[] fileData = File.ReadAllBytes(filePath);

               
                string fileType = filePath.EndsWith(".mp4") || filePath.EndsWith(".avi") ? "Video" : "Image";

             
                var newStory = new Story
                {
                    UserID = _currentUserID,
                    Content = fileData,
                    Type = fileType,
                    Expiration = DateTime.Now.AddHours(24)  
                };

                _context.Stories.Add(newStory);
                _context.SaveChanges();

      
                LoadStories();
            }
        }


        private void UsersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selectedUserID = (int)UsersComboBox.SelectedValue;

            var userStories = _context.Stories
                .Where(s => s.UserID == selectedUserID && s.Expiration > DateTime.Now)
                .ToList();

            if (userStories.Any())
            {
                StoriesList.Items.Clear();

                foreach (var story in userStories)
                {
                    if (story.Type == "Image")
                    {
                        var image = new Image();
                        var bitmap = new BitmapImage();
                        using (var stream = new MemoryStream(story.Content))
                        {
                            bitmap.BeginInit();
                            bitmap.StreamSource = stream;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                        }
                        image.Source = bitmap;
                        StoriesList.Items.Add(image);
                    }
                    else if (story.Type == "Video")
                    {
                        var mediaElement = new MediaElement();
                        var stream = new MemoryStream(story.Content);
                        string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".mp4");

                     
                        File.WriteAllBytes(tempFilePath, stream.ToArray());

                        mediaElement.Source = new Uri(tempFilePath);
                        mediaElement.LoadedBehavior = MediaState.Manual;
                        mediaElement.Play();
                        StoriesList.Items.Add(mediaElement);

                 
                        mediaElement.MediaEnded += (obj, args) =>
                        {
                            File.Delete(tempFilePath);  
                        };
                    }
                }
            }
            else
            {
                MessageBox.Show("No active stories for this user.");
            }
        }
    }
    }

