using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

namespace ChatApp
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Window
    {


        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int description, int reservedValue);

        public static bool IsInternetAvailable()
        {
            int description;
            return InternetGetConnectedState(out description, 0);
        }


        private bool successLogin = false;
        public static string usrName ;
        public SecureString SecurePassword { private get; set; }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext != null)
            { ((dynamic)this.DataContext).SecurePassword = ((PasswordBox)sender).SecurePassword; }
            //Console.WriteLine(SecurePassword);
        }

        // firebase work

        IFirebaseConfig ifc = new FirebaseConfig
        {
            AuthSecret = secret.authSec,
            BasePath = secret.basePath
        };

        IFirebaseClient client;


        public LoginPage()
        {
            InitializeComponent();

            // firebase
            try
            {
                client = new FireSharp.FirebaseClient(ifc);
            }
            catch
            {
                MessageBox.Show("Connection problem");
            }
        }


        public string getUserName()
        {
            return usrName;
        }

        private bool isExit = false;
        public void ExitBtn(object sender, RoutedEventArgs e)
        {
            isExit = true;
            Application.Current.Shutdown();
        }

        public bool _Exit()
        {
            return isExit;
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private bool isPressed = false;
        public void RegBtn_LoginPage(object sender, RoutedEventArgs e)
        {
            isPressed = true;
            this.Close();
            //this.Hide();
            //this.Show();
            //reg.Close();
        }

        public bool isRegPressed()
        {
            return isPressed;
        }

        public bool _sucess()
        {
            return successLogin;
        }

        private async void LoginBtn_Click(object sender, RoutedEventArgs e)
        {
            #region Condition
            if (string.IsNullOrWhiteSpace(UsernameTBox.Text) &&
                string.IsNullOrWhiteSpace(passTbox.Password.ToString()))
            {
                //MessageBox.Show("Fill all details!");
                LoginStatus.Foreground = new SolidColorBrush(Color.FromRgb(255, 23, 68));
                LoginStatus.Content = "Fill all details!";
                await Task.Delay(3000);
                LoginStatus.Content = "";
                return;
            }
            #endregion
            bool connection = IsInternetAvailable();
            //Debug.WriteLine(connection);
            if (connection == false)
            {
                NetworkCardLoginPage.Content = "Please check your internet connection";
                await Task.Delay(2000);
                NetworkCardLoginPage.Content = "";
            }
            else
            {
                FirebaseResponse res = client.Get(@"Users/" + UsernameTBox.Text);
                MyUser ResUser = res.ResultAs<MyUser>(); // data base info
                ResUser.Password = Decrypted(ResUser.Password);


                MyUser CurUser = new MyUser() // user info
                {
                    Username = UsernameTBox.Text,
                    Password = passTbox.Password.ToString()
                };


                if (MyUser.IsEqual(ResUser, CurUser))
                {

                    // run chat-app here and use the username/nickname of the guy there
                    //MainWindow ChatApp = new MainWindow();
                    //this.Visibility = Visibility.Hidden;
                    //ChatApp.Show();

                    LoginStatus.Foreground = new SolidColorBrush(Color.FromRgb(100, 221, 23));
                    LoginStatus.Content = "Successfully logged in. Please wait";
                    await Task.Delay(1000);
                    successLogin = true;
                    usrName = UsernameTBox.Text;
                    this.Close();
                  /*  //MessageBox.Show("LogedIn");
                    this.Close();
                    MainWindow ChatApp = new MainWindow();
                    ChatApp.Owner = this;
                    this.Hide();
                    ChatApp.ShowDialog();*/
                    //ChatApp.ShowDialog();
                    //this.Close();

                    //MessageBox.Show("Logged in!");
                }
                else
                {
                    //MessageBox.Show(MyUser.error);
                    if(MyUser.err == 'o')
                    {
                        LoginStatus.Foreground = new SolidColorBrush(Color.FromRgb(255, 23, 68));
                        LoginStatus.Content = "There was some error. Kindly check your username or password";
                        await Task.Delay(3000);
                        LoginStatus.Content = "";
                    }
                    else if(MyUser.err == 'u')
                    {
                        LoginStatus.Foreground = new SolidColorBrush(Color.FromRgb(255, 23, 68));
                        LoginStatus.Content = "Username does not exist!";
                        await Task.Delay(3000);
                        LoginStatus.Content = "";
                    }
                    else if(MyUser.err == 'p')
                    {
                        LoginStatus.Foreground = new SolidColorBrush(Color.FromRgb(255, 23, 68));
                        LoginStatus.Content = "Username or password does not match!";
                        await Task.Delay(3000);
                        LoginStatus.Content = "";
                    }
                }
            }
        }


        public static string IV = "1a1a1a1a1a1a1a1a";
        public static string Keyy = "1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a13";

        public static string Encrypt(string decrypted)
        {
            byte[] textBytes = ASCIIEncoding.ASCII.GetBytes(decrypted);
            AesCryptoServiceProvider endec = new AesCryptoServiceProvider(); ;
            endec.BlockSize = 128;
            endec.KeySize = 256;
            endec.IV = ASCIIEncoding.ASCII.GetBytes(IV);
            endec.Key = ASCIIEncoding.ASCII.GetBytes(Keyy);
            endec.Padding = PaddingMode.PKCS7;
            endec.Mode = CipherMode.CBC;
            ICryptoTransform icrypt = endec.CreateEncryptor(endec.Key, endec.IV);
            byte[] enc = icrypt.TransformFinalBlock(textBytes, 0, textBytes.Length);
            icrypt.Dispose();
            return Convert.ToBase64String(enc);
        }
        public static string Decrypted(string encrypted)
        {
            byte[] textbytes = Convert.FromBase64String(encrypted);
            AesCryptoServiceProvider endec = new AesCryptoServiceProvider();
            endec.BlockSize = 128;
            endec.KeySize = 256;
            endec.IV = ASCIIEncoding.ASCII.GetBytes(IV);
            endec.Key = ASCIIEncoding.ASCII.GetBytes(Keyy);
            endec.Padding = PaddingMode.PKCS7;
            endec.Mode = CipherMode.CBC;
            ICryptoTransform icrypt = endec.CreateDecryptor(endec.Key, endec.IV);
            byte[] enc = icrypt.TransformFinalBlock(textbytes, 0, textbytes.Length);
            icrypt.Dispose();
            return ASCIIEncoding.ASCII.GetString(enc);
        }



        private void enter_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
            {
                LoginBtn_Click(sender, e);
            }
        }

    }
}
