using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
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
using System.Text.RegularExpressions;
using System.Windows.Media.Animation;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Security.Cryptography;

namespace ChatApp
{
    /// <summary>
    /// Interaction logic for RegistrationPage.xaml
    /// </summary>
    /// 

    public enum PasswordScore
    {
        Blank = 0,
        VeryWeak = 1,
        Weak = 2,
        Medium = 3,
        Strong = 4,
        VeryStrong = 5
    }

    public partial class RegistrationPage : Window
    {

        int isValid;
        public static string ussrName;

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            /* if (this.DataContext != null)
             { ((dynamic)this.DataContext).SecurePassword = ((PasswordBox)sender).SecurePassword; }*/
            //Console.WriteLine(SecurePassword);

            PasswordScore passwordStrengthScore = CheckStrength(passTbox.Password.ToString());
            switch (passwordStrengthScore)
            {
                case PasswordScore.Blank:
                    loadProgressBar(0, 0, 0, 0);
                    RegStatus.Foreground = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                    RegStatus.Content = "Password can't be empty";
                    isValid = 0;
                    break;
                case PasswordScore.VeryWeak:
                    loadProgressBar(10, 255, 0, 0);
                    RegStatus.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    RegStatus.Content = "Very weak";
                    isValid = 0;
                    //await Task.Delay(2000);
                   // RegStatus.Content = "";
                    break;
                case PasswordScore.Weak:
                    loadProgressBar(20, 255, 255, 0);
                    RegStatus.Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 0));
                    RegStatus.Content = "Weak";
                    isValid = 0;
                    //await Task.Delay(2000);
                    //RegStatus.Content = "";
                    break;
                case PasswordScore.Medium:
                    loadProgressBar(50, 255, 140, 0);
                    RegStatus.Foreground = new SolidColorBrush(Color.FromRgb(255, 140, 0));
                    RegStatus.Content = "Okay...";
                    isValid = 1;
                    //await Task.Delay(2000);
                    //RegStatus.Content = "";
                    break;
                case PasswordScore.Strong:
                    loadProgressBar(70, 50, 205, 50);
                    RegStatus.Foreground = new SolidColorBrush(Color.FromRgb(50, 205, 50));
                    RegStatus.Content = "Pretty good";
                    isValid = 1;
                    //await Task.Delay(2000);
                    //RegStatus.Content = "";
                    break;
                case PasswordScore.VeryStrong:
                    loadProgressBar(90, 34, 139, 34);
                    RegStatus.Foreground = new SolidColorBrush(Color.FromRgb(34, 139, 34));
                    RegStatus.Content = "Strong!";
                    isValid = 1;
                    //await Task.Delay(2000);
                    //RegStatus.Content = "";
                    break;
            }
        }


        IFirebaseConfig ifc = new FirebaseConfig
        {
            AuthSecret = secret.authSec,
            BasePath = secret.basePath
        };
        public RegistrationPage()
        {
            InitializeComponent();
            try
            {
                client = new FireSharp.FirebaseClient(ifc);
            }
            catch
            {
                MessageBox.Show("Connection problem");
            }
        }



        IFirebaseClient client;

        public string getUserName()
        {
            return ussrName;
        }

        private void BackBtn(object sender, RoutedEventArgs e)
        {
            this.Close();
            /*LoginPage login = new LoginPage();
            login.Show();*/
        }

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private async void RegBtn_Click(object sender, RoutedEventArgs e)
        {
            bool IsUsernameExists = false;
            #region Condition
            if (string.IsNullOrWhiteSpace(UsernameTBox.Text))          
            {
                RegStatus.Foreground = new SolidColorBrush(Color.FromRgb(255, 23, 68));
                RegStatus.Content = "Fill all details!";
                await Task.Delay(2000);
                RegStatus.Content = "";
                return;
            }
            else if (string.IsNullOrWhiteSpace(nameTbox.Text))
            {
                RegStatus.Foreground = new SolidColorBrush(Color.FromRgb(255, 23, 68));
                RegStatus.Content = "Fill all details!";
                await Task.Delay(2000);
                RegStatus.Content = "";
                return;
            }
            else if(string.IsNullOrWhiteSpace(passTbox.Password.ToString()))
            {
                RegStatus.Foreground = new SolidColorBrush(Color.FromRgb(135, 206, 235));
                RegStatus.Content = "Enter your password";
                await Task.Delay(2000);
                RegStatus.Content = "";
                return;
            }
            else if(isValid == 0)
            {
                RegStatus.Foreground = new SolidColorBrush(Color.FromRgb(135, 206, 235));
                RegStatus.Content = "Enter a strong password";
                await Task.Delay(2000);
                RegStatus.Content = "";
                return;
            }

            #endregion
            else
            {
                try
                {
                    FirebaseResponse res = await client.GetAsync(@"Users/" + (UsernameTBox.Text));
                    Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(res.Body.ToString());
                    if (UsernameTBox.Text == data.ElementAt(2).Value)
                    {
                        IsUsernameExists = true;
                    }
                }
                catch { }
                if (IsUsernameExists)
                {
                    usernameExistsError.Content = "Username already exists";
                    await Task.Delay(5000);
                    usernameExistsError.Content = "";
                }
                else
                {
                    MyUser user = new MyUser()
                    {
                        Username = UsernameTBox.Text,
                        Nickname = nameTbox.Text,
                        Password = Encrypt(passTbox.Password.ToString())
                    };

                    ussrName = UsernameTBox.Text;

                    // uploading data to firebase
                    SetResponse set = client.Set(@"Users/" + UsernameTBox.Text, user);
                    //MessageBox.Show("Registeration done!");
                    RegStatus.Foreground = new SolidColorBrush(Color.FromRgb(100, 221, 23));
                    RegStatus.Content = "Registration done... Hold on!";
                    await Task.Delay(3000);
                    BackBtn(sender, e);
                }
            }
        }



        public static string IV = "1a1a1a1a1a1a1a1a";
        public static string Key = "1a1a1a1a1a1a1a1a1a1a1a1a1a1a1a13";

        public static string Encrypt(string decrypted)
        {
            byte[] textBytes = ASCIIEncoding.ASCII.GetBytes(decrypted);
            AesCryptoServiceProvider endec = new AesCryptoServiceProvider(); ;
            endec.BlockSize = 128;
            endec.KeySize = 256;
            endec.IV = ASCIIEncoding.ASCII.GetBytes(IV);
            endec.Key = ASCIIEncoding.ASCII.GetBytes(Key);
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
            endec.Key = ASCIIEncoding.ASCII.GetBytes(Key);
            endec.Padding = PaddingMode.PKCS7;
            endec.Mode = CipherMode.CBC;
            ICryptoTransform icrypt = endec.CreateDecryptor(endec.Key, endec.IV);
            byte[] enc = icrypt.TransformFinalBlock(textbytes, 0, textbytes.Length);
            icrypt.Dispose();
            return ASCIIEncoding.ASCII.GetString(enc);
        }


        private void loadProgressBar(double value, byte r, byte g, byte b)
        {
            Duration dur = new Duration(TimeSpan.FromSeconds(1));
            DoubleAnimation dblani = new DoubleAnimation(value, dur);
            pBar.Foreground = new SolidColorBrush(Color.FromRgb(r, g, b));
            pBar.BeginAnimation(System.Windows.Controls.Primitives.RangeBase.ValueProperty, dblani);
        }

        public static PasswordScore CheckStrength(string password)
        {
            int score = 1;

            if (password.Length < 1)
                return PasswordScore.Blank;
            if (password.Length < 4)
                return PasswordScore.VeryWeak;

            if (password.Length >= 8)
                score++;
            if (password.Length >= 12)
                score++;
            if (Regex.Match(password, @"/\d+/", RegexOptions.ECMAScript).Success)
                score++;
            if (Regex.Match(password, @"[a-z]", RegexOptions.ECMAScript).Success &&
              Regex.Match(password, @"[A-Z]", RegexOptions.ECMAScript).Success)
                score++;
            if (Regex.Match(password, @".[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]", RegexOptions.ECMAScript).Success)
                score++;

            return (PasswordScore)score;
        }

        public static SecureString ToSecureString(string plainString)
        {
            if (plainString == null)
                return null;

            SecureString secureString = new SecureString();
            foreach (char c in plainString.ToCharArray())
            {
                secureString.AppendChar(c);
            }
            return secureString;
        }
    }

    internal class Data
    {
        public string Name { get; set; }
        public string Pass { get; set; }
        public string Nick { get; set; }
    }
}
