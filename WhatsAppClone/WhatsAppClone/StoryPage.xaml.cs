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
            LoadUsers();  // تحميل المستخدمين في الـ ComboBox
            LoadStories();  // تحميل القصص من جميع المستخدمين
        }

        // تحميل المستخدمين في الـ ComboBox
        private void LoadUsers()
        {
            var users = _context.Users.Select(u => new { u.UserID, u.Username }).ToList();
            UsersComboBox.ItemsSource = users;
            UsersComboBox.DisplayMemberPath = "Username";
            UsersComboBox.SelectedValuePath = "UserID";
        }

        // تحميل القصص لجميع المستخدمين باستثناء القصة الخاصة بالمستخدم الحالي
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

                    // حفظ الفيديو في ملف مؤقت
                    File.WriteAllBytes(tempFilePath, stream.ToArray());

                    mediaElement.Source = new Uri(tempFilePath);
                    mediaElement.LoadedBehavior = MediaState.Manual;
                    mediaElement.Play();
                    StoriesList.Items.Add(mediaElement);

                    // إضافة حدث عند الانتهاء من تشغيل الفيديو
                    mediaElement.MediaEnded += (obj, args) =>
                    {
                        File.Delete(tempFilePath);  // حذف الملف المؤقت بعد الانتهاء من تشغيل الفيديو
                    };
                }
            }
        }

        // رفع القصة (صورة أو فيديو)
        private void UploadStoryButton_Click(object sender, RoutedEventArgs e)
        {
            // فتح نافذة لاختيار الملف (صورة أو فيديو)
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png|Video Files|*.mp4;*.avi";

            if (fileDialog.ShowDialog() == true)
            {
                string filePath = fileDialog.FileName;  // المسار الكامل للملف

                // تحويل الملف إلى بيانات ثنائية (Binary)
                byte[] fileData = File.ReadAllBytes(filePath);

                // تحديد نوع القصة بناءً على الامتداد
                string fileType = filePath.EndsWith(".mp4") || filePath.EndsWith(".avi") ? "Video" : "Image";

                // تخزين القصة في قاعدة البيانات
                var newStory = new Story
                {
                    UserID = _currentUserID,
                    Content = fileData,
                    Type = fileType,
                    Expiration = DateTime.Now.AddHours(24)  // القصة ستنتهي بعد 24 ساعة
                };

                _context.Stories.Add(newStory);
                _context.SaveChanges();

                // إعادة تحميل القصص بعد إضافة القصة الجديدة
                LoadStories();
            }
        }

        // عرض حالة القصة للمستخدم الذي تم اختياره
       
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

                        // حفظ الفيديو في ملف مؤقت
                        File.WriteAllBytes(tempFilePath, stream.ToArray());

                        mediaElement.Source = new Uri(tempFilePath);
                        mediaElement.LoadedBehavior = MediaState.Manual;
                        mediaElement.Play();
                        StoriesList.Items.Add(mediaElement);

                        // إضافة حدث عند الانتهاء من تشغيل الفيديو
                        mediaElement.MediaEnded += (obj, args) =>
                        {
                            File.Delete(tempFilePath);  // حذف الملف المؤقت بعد الانتهاء من تشغيل الفيديو
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

