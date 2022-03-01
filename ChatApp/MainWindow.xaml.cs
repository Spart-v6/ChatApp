using System;
using System.Windows;
using System.Windows.Input;
using FireSharp.Config;
using FireSharp.Response;
using FireSharp.Interfaces;
using System.ComponentModel;
using System.Collections.Specialized;
using ChatApp.MVVM.ViewModel;
using System.Windows.Controls;
using FireSharp;
using Google.Cloud.Firestore;
using System.Collections.Generic;
using System.Windows.Media;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Media.Animation;
using System.Net;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace ChatApp
{

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		// shutting down login page 
		/*protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }*/
		[DllImport("user32.dll")]
		static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);


		public bool isOnline = true;

		protected override void OnClosing(CancelEventArgs e)
		{
			e.Cancel = true;  // cancels the window close    
			this.Hide();      // Programmatically hides the window
		}

		
		protected override void OnClosed(EventArgs e)
        {
			isOnline = false;
			MainViewModel vm = new(loginWindow.getUserName(), isOnline, NetworkCard, InsideNetworkCard, lstView);

			this.DataContext = vm;
			
			base.OnClosed(e);

		}

        public static bool isLoginClosed = false;

		public static MainWindow instance;

		LoginPage loginWindow = new LoginPage();
		public MainWindow()
        {
            try
            {
				InitializeComponent();
				


				//NetworkAvailabilityChangedEventHandler myHandler = new NetworkAvailabilityChangedEventHandler(AvailabilityChanged);
				//NetworkChange.NetworkAvailabilityChanged += myHandler;

				instance = this;
				this.Hide();
				RegistrationPage reg = new RegistrationPage();

				

				this.Resources["ChangeBoxColor"] = new SolidColorBrush(ConvertStringToColor("#3E4147"));
				this.Resources["ChangeChatBoxColor"] = new SolidColorBrush(ConvertStringToColor("#FFFFFF"));
				this.Resources["ListViewMsgColor"] = new SolidColorBrush(ConvertStringToColor("#fff7ff"));

				//DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
				/*var firebase = new Firebase.Database.FirebaseClient("");
				var dinos = await firebase.Child().OrderByKey().LimitToFirst().OnceAsync<>();*/
				

				((INotifyCollectionChanged)lstView.Items).CollectionChanged += ListView_CollectionChanged; // scrolling to latest msg

				loginWindow.Show();

				loginWindow.Closed += LoginWindow_Closed;

				void LoginWindow_Closed(object sender, EventArgs e)
				{
					if (loginWindow.isRegPressed())
					{
						reg.Show();
						loginWindow.Close();
						reg.Closed += RegWindow_Closed;
					}
                    else {
                        if (!loginWindow._Exit())
                        {
							this.Show(); // chat app show
							this.userNameLabel.Content = loginWindow.getUserName();

                            MainViewModel vm = new(loginWindow.getUserName(),isOnline,NetworkCard,InsideNetworkCard, lstView);
							this.DataContext = vm;
                            //Application.Current.Dispatcher.Invoke(async () =>
                            //{
                            //await Task.Delay(1000);
                            OnlineBounce.Fill = new SolidColorBrush(Colors.LightGreen);
							//});
							
							/*var idleTime = GetIdleTimeInfo();

							if (idleTime.IdleTime.TotalSeconds >= 15)
							{
								Debug.WriteLine("Idle!");
							}*/


							/*Thread thread = new Thread(new ThreadStart(CheckNetwork));
							thread.IsBackground = true;
							thread.Start();*/

							/*Application.Current.Dispatcher.Invoke(async () =>
							{
								await Task.Delay(1000);
								//MainChatColor.RelativeTransform = new ScaleTransform(1.15,1.15);
								ScaleTransform trans = new ScaleTransform();
								MainChatColor.RelativeTransform = trans;
								DoubleAnimation anim = new DoubleAnimation(1.00, 1.05, TimeSpan.FromMilliseconds(25000));
								anim.RepeatBehavior = RepeatBehavior.Forever;
								anim.AutoReverse = true;
								trans.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
								trans.BeginAnimation(ScaleTransform.ScaleYProperty, anim);

							});*/


						}
					}
				}

				void RegWindow_Closed(object sender, EventArgs e)
				{
					//MessageBox.Show("Showing login page now");
					this.Close();
					MainWindow mainWindow = new MainWindow();

				}
			}
            catch
            {
				MessageBox.Show("Something went wrong, try opening app again");
            }
		}


		private void ListView_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
			Application.Current.Dispatcher.Invoke(async () =>
			{
				await Task.Delay(1000);
				if (e.Action == NotifyCollectionChangedAction.Add)
				{
					// scroll the new item into view   
					lstView.ScrollIntoView(e.NewItems[0]);
				}
			});
        }


        #region ButtonClicks
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
				DragMove();
		}

		private void BtnMinimize_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}
		private void BtnMaximize_Click(object sender, RoutedEventArgs e)
		{
			//MessageBox.Show(sender.ToString());
			if (this.WindowState != WindowState.Maximized)
				this.WindowState = WindowState.Maximized;
			else
				this.WindowState = WindowState.Normal;
        }

		private void BtnClose_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
			if (this.WindowState != WindowState.Maximized)
				this.WindowState = WindowState.Maximized;
			else
				this.WindowState = WindowState.Normal;
		}

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e){	}





		public System.Windows.Media.Color ConvertStringToColor(String hex)
		{

			hex = hex.Replace("#", "");

			byte a = 255;
			byte r = 255;
			byte g = 255;
			byte b = 255;

			int start = 0;

			//handle ARGB strings (8 characters long) 
			if (hex.Length == 8)
			{
				a = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
				start = 2;
			}

			//convert RGB characters to bytes 
			r = byte.Parse(hex.Substring(start, 2), System.Globalization.NumberStyles.HexNumber);
			g = byte.Parse(hex.Substring(start + 2, 2), System.Globalization.NumberStyles.HexNumber);
			b = byte.Parse(hex.Substring(start + 4, 2), System.Globalization.NumberStyles.HexNumber);

			return System.Windows.Media.Color.FromArgb(a, r, g, b);
		}

		private void ToggleSwitch_Click(object sender, RoutedEventArgs e)
        {
			if(ToggleSwitch.IsChecked == true)
            {
				//Debug.WriteLine("Yep toggled! White mode");

				SolidColorBrush brush = new SolidColorBrush(ConvertStringToColor("#222222")); // from 
				Topbar.Background = brush;
				ColorAnimation animation = new ColorAnimation(Colors.MediumPurple, // to
										   new Duration(TimeSpan.FromMilliseconds(500)));
				animation.EasingFunction = new QuarticEase();
				brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);


				Topbar.Foreground = new SolidColorBrush(ConvertStringToColor("#121212"));

				MinimizeButton.Foreground = new SolidColorBrush(ConvertStringToColor("#121212"));
				MaximizeButton.Foreground = new SolidColorBrush(ConvertStringToColor("#121212"));
				CloseButton.Foreground = new SolidColorBrush(ConvertStringToColor("#121212"));


				SolidColorBrush brush1 = new SolidColorBrush(ConvertStringToColor("#202a40")); // from 
				LeftBar.Background = brush1;
				ColorAnimation animation1 = new ColorAnimation(ConvertStringToColor("#dae0ff"), // to
										   new Duration(TimeSpan.FromMilliseconds(500)));
				animation1.EasingFunction = new QuarticEase();
				brush1.BeginAnimation(SolidColorBrush.ColorProperty, animation1);



				TopLeftBar.Foreground = new SolidColorBrush(ConvertStringToColor("#121212"));

				GrpChat.Foreground = new SolidColorBrush(ConvertStringToColor("#121212"));


				/*SolidColorBrush brush2 = new SolidColorBrush(ConvertStringToColor("#2b303b")); // from 
				MainChatColor.Background = brush2;
				ColorAnimation animation2 = new ColorAnimation(ConvertStringToColor("#CFD8DC"), // to
										   new Duration(TimeSpan.FromMilliseconds(500)));
				animation2.EasingFunction = new QuarticEase();
				brush2.BeginAnimation(SolidColorBrush.ColorProperty, animation2);*/

				ImageBrush ImgBrush = new ImageBrush();
				ImgBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/papercut_07.jpg"));
				MainChatColor.ImageSource = ImgBrush.ImageSource;


				SolidColorBrush brush3 = new SolidColorBrush(ConvertStringToColor("#171e2e")); // from 
				BorderBelow.Background = brush3;
				ColorAnimation animation3 = new ColorAnimation(ConvertStringToColor("#bdc8fe"), // to
										   new Duration(TimeSpan.FromMilliseconds(500)));
				animation3.EasingFunction = new QuarticEase();
				brush3.BeginAnimation(SolidColorBrush.ColorProperty, animation3);


				BorderBelow.BorderBrush = new SolidColorBrush(ConvertStringToColor("#e4d3ff"));


				this.Resources["ChangeBoxColor"] = new SolidColorBrush(ConvertStringToColor("#fff7ff"));
				this.Resources["ChangeChatBoxColor"] = new SolidColorBrush(ConvertStringToColor("#121212"));
				this.Resources["ListViewMsgColor"] = new SolidColorBrush(ConvertStringToColor("#121212"));
			}
            else
            {
				//Debug.WriteLine("Nope not toggled! Dark Mode");

				SolidColorBrush brush = new SolidColorBrush(Colors.MediumPurple);
				Topbar.Background = brush;
				ColorAnimation animation = new ColorAnimation(ConvertStringToColor("#181818"),
										   new Duration(TimeSpan.FromMilliseconds(200)));
				animation.EasingFunction = new QuarticEase();
				brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);

                Topbar.Foreground = new SolidColorBrush(ConvertStringToColor("#D3D3D3"));


                MinimizeButton.Foreground = new SolidColorBrush(ConvertStringToColor("#fff7ff"));
                MaximizeButton.Foreground = new SolidColorBrush(ConvertStringToColor("#fff7ff"));
                CloseButton.Foreground = new SolidColorBrush(ConvertStringToColor("#fff7ff"));


				SolidColorBrush brush1 = new SolidColorBrush(ConvertStringToColor("#dfdfdf")); // from 
				LeftBar.Background = brush1;

				ColorAnimation animation1 = new ColorAnimation(ConvertStringToColor("#202a40"), // to
										   new Duration(TimeSpan.FromMilliseconds(200)));
				animation1.EasingFunction = new QuarticEase();
				brush1.BeginAnimation(SolidColorBrush.ColorProperty, animation1);



				TopLeftBar.Foreground = new SolidColorBrush(ConvertStringToColor("#FFFFFF"));

				GrpChat.Foreground = new SolidColorBrush(ConvertStringToColor("#fff7ff"));


				/*SolidColorBrush brush2 = new SolidColorBrush(ConvertStringToColor("#CFD8DC")); // from 
				MainChatColor.Background = brush2;
				ColorAnimation animation2 = new ColorAnimation(ConvertStringToColor("#2b303b"), // to
										   new Duration(TimeSpan.FromMilliseconds(200)));
				animation2.EasingFunction = new QuarticEase();
				brush2.BeginAnimation(SolidColorBrush.ColorProperty, animation2);*/

				ImageBrush ImgBrush = new ImageBrush();
				ImgBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/papercut_2.jpg"));
				MainChatColor.ImageSource = ImgBrush.ImageSource;


				SolidColorBrush brush3 = new SolidColorBrush(ConvertStringToColor("#e9e9e9")); // from 
				BorderBelow.Background = brush3;
				ColorAnimation animation3 = new ColorAnimation(ConvertStringToColor("#171e2e"), // to
										   new Duration(TimeSpan.FromMilliseconds(200)));
				animation3.EasingFunction = new QuarticEase();
				brush3.BeginAnimation(SolidColorBrush.ColorProperty, animation3);
				
				BorderBelow.BorderBrush = new SolidColorBrush(ConvertStringToColor("#21293c"));
				BorderShadow.Color = System.Windows.Media.Color.FromRgb(33,41,60);


				this.Resources["ChangeBoxColor"] = new SolidColorBrush(ConvertStringToColor("#3E4147"));
				this.Resources["ChangeChatBoxColor"] = new SolidColorBrush(ConvertStringToColor("#FFFFFF"));
				this.Resources["ListViewMsgColor"] = new SolidColorBrush(ConvertStringToColor("#fff7ff"));
				
			}
        }

		#endregion

		public static IdleTimeInfo GetIdleTimeInfo()
		{
			int systemUptime = Environment.TickCount,
				lastInputTicks = 0,
				idleTicks = 0;

			LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
			lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
			lastInputInfo.dwTime = 0;

			if (GetLastInputInfo(ref lastInputInfo))
			{
				lastInputTicks = (int)lastInputInfo.dwTime;

				idleTicks = systemUptime - lastInputTicks;
			}

			return new IdleTimeInfo
			{
				LastInputTime = DateTime.Now.AddMilliseconds(-1 * idleTicks),
				IdleTime = new TimeSpan(0, 0, 0, 0, idleTicks),
				SystemUptimeMilliseconds = systemUptime,
			};
		}

	}

	public class IdleTimeInfo
	{
		public DateTime LastInputTime { get; internal set; }

		public TimeSpan IdleTime { get; internal set; }

		public int SystemUptimeMilliseconds { get; internal set; }
	}

	internal struct LASTINPUTINFO
	{
		public uint cbSize;
		public uint dwTime;
	}
}
