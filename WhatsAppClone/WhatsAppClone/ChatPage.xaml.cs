using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading; // استيراد المكتبة الخاصة بالمؤقت

namespace WhatsAppClone
{
    public partial class ChatPage : Window
    {
        private WhatsAppCloneEntities _context = new WhatsAppCloneEntities();
        private int _currentUserID;
        private int _selectedContactID;
        private DispatcherTimer _messageTimer; // تعريف المؤقت
        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (!string.IsNullOrWhiteSpace(MessageTextBox.Text))
                {
                    var newMessage = new Chat
                    {
                        SenderID = _currentUserID,
                        ReceiverID = _selectedContactID,
                        MessageText = MessageTextBox.Text,
                        Timestamp = System.DateTime.Now
                    };

                    _context.Chats.Add(newMessage);
                    _context.SaveChanges();

                    MessageTextBox.Clear();
                    LoadMessages();
                }
                e.Handled = true; // منع الإدخال الافتراضي لـ Enter
            }
        }
            public ChatPage(int userID)
        {
            InitializeComponent();
            _currentUserID = userID;
            LoadContacts();

            // إعداد المؤقت
            _messageTimer = new DispatcherTimer();
            _messageTimer.Interval = System.TimeSpan.FromSeconds(1); // التحديث كل ثانية
            _messageTimer.Tick += (sender, e) => LoadMessages(); // استدعاء الدالة لتحديث الرسائل
            _messageTimer.Start(); // بدء المؤقت
        }

        private void LoadContacts()
        {
            var contacts = _context.Users.Where(u => u.UserID != _currentUserID)
                                         .Select(u => new { u.UserID, u.Username }).ToList();
            ContactsList.ItemsSource = contacts;
        }

        private void ContactsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedContact = (dynamic)ContactsList.SelectedItem;
            if (selectedContact != null)
            {
                _selectedContactID = selectedContact.UserID;
                LoadMessages();
            }
        }

        private void LoadMessages()
        {
            ChatMessages.Children.Clear();

            var messages = _context.Chats
                .Where(c => (c.SenderID == _currentUserID && c.ReceiverID == _selectedContactID) ||
                            (c.SenderID == _selectedContactID && c.ReceiverID == _currentUserID))
                .OrderBy(c => c.Timestamp)
                .ToList();

            foreach (var message in messages)
            {
                var messageBlock = new TextBlock
                {
                    Text = message.MessageText,
                    Margin = new Thickness(5),
                    Padding = new Thickness(10),
                    Background = message.SenderID == _currentUserID ?
                                 System.Windows.Media.Brushes.LightBlue :
                                 System.Windows.Media.Brushes.LightGray,
                    HorizontalAlignment = message.SenderID == _currentUserID ?
                                          HorizontalAlignment.Right :
                                          HorizontalAlignment.Left
                };

                ChatMessages.Children.Add(messageBlock);
            }
        }

        private void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(MessageTextBox.Text))
            {
                var newMessage = new Chat
                {
                    SenderID = _currentUserID,
                    ReceiverID = _selectedContactID,
                    MessageText = MessageTextBox.Text,
                    Timestamp = System.DateTime.Now
                };

                _context.Chats.Add(newMessage);
                _context.SaveChanges();

                MessageTextBox.Clear();
                LoadMessages();
            }
        }

        private void GoToStoryPage_Click(object sender, RoutedEventArgs e)
        {
            // فتح صفحة الحالة
            StoryPage storyPage = new StoryPage(_currentUserID);
            storyPage.Show();
        }
    }
}
