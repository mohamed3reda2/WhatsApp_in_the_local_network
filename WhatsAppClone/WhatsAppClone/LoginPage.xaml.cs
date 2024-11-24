using System.Windows;
using System.Linq;

namespace WhatsAppClone
{
    public partial class LoginPage : Window
    {
        private WhatsAppCloneEntities _context = new WhatsAppCloneEntities();

        public LoginPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

       
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
         
                ChatPage chatPage = new ChatPage(user.UserID);
                chatPage.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة");
            }
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            resetPassword.Visibility= Visibility.Visible;
            restbu.Visibility= Visibility.Visible;
            login.Visibility= Visibility.Collapsed;
            singup.Visibility= Visibility.Collapsed;

            reset.Visibility= Visibility.Collapsed;


        }

        private void resetassButton_Click(object sender, RoutedEventArgs e)
        {

            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            string resetpassword = resetPassword.Password;

         
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
                user.Password = resetpassword;
                _context.SaveChanges();
                MessageBox.Show("تم تغير كلمة المرور بنجاح");
                ChatPage chatPage = new ChatPage(user.UserID);
                chatPage.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة");
            }
        }

        private void singButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

          
            var user = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user != null)
            {
              
                MessageBox.Show("هذا المستخدم موجود بالفعل");

            }
            else
            {
                User user1 =new User() { Username = username, Password = password };
                _context.Users.Add(user1);
                _context.SaveChanges();
                ChatPage chatPage = new ChatPage(user1.UserID);
                chatPage.Show();
                this.Close();
            }
        }
    }
}
