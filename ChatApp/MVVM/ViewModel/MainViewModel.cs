using System;
using System.Collections.ObjectModel;
using ChatApp.Core;
using ChatApp.MVVM.Model;
using System.ComponentModel;
using System.Windows.Controls;
using FireSharp.Config;
using FireSharp;
using FireSharp.Response;
using System.Windows.Threading;
using System.Windows;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Google.Cloud.Firestore;
using Firebase;
using Firebase.Database;
using Firebase.Database.Query;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Threading;
using System.Collections.Specialized;

namespace ChatApp.MVVM.ViewModel
{
    class MainViewModel : ObservableObject, INotifyPropertyChanged
    {
        public ObservableCollection<MessageModel> Messages { get; set; }
        public ObservableCollection<ContactModel> Contacts { get; set; }
		ContactModel Cont = new();

        #region Commands 

        public RelayCommand SendCommand { get; set; }

        private string _message;
        public string Message 
		{
			get { return _message; }
			set 
			{
				_message = value;
				onPropertyChanged();
			}
		}


        private string usrName;
        public string mainUserName
        {
            get { return usrName; }
            set
            {
                usrName = value;
                onPropertyChanged();
            }
        }


        private string time;
        public string Time
        {
            get { return time; }
            set
            {
                time = value;
                onPropertyChanged();
            }
        }
        #endregion

        #region Internet Checker
        [DllImport("wininet.dll")]
		private extern static bool InternetGetConnectedState(out int description, int reservedValue);

		public static bool IsInternetAvailable()
		{
			int description;
			return InternetGetConnectedState(out description, 0);
		}
        #endregion

        FirestoreDb db;

		// string TIME = DateTime.Now.ToString("dd-MMM-yy hh:mm tt");
		Timestamp TIME = Timestamp.GetCurrentTimestamp();

        public MainViewModel(string xmlUserName, bool isOnline, Border NetworkCard, TextBlock InsideNetworkCard, ListView lstView) 
        {

			string path = AppDomain.CurrentDomain.BaseDirectory + @"chatapp_firestore.json";
			Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
			db = FirestoreDb.Create(secret.coll);


			usrName = xmlUserName;

            //loadMsgs(usrName, lstView); 

			SetListenerMsgs(usrName, Message, lstView); 


			Messages = new ObservableCollection<MessageModel>();
			Contacts = new ObservableCollection<ContactModel>();
			SendCommand = new RelayCommand(async o =>
			{
				if (!string.IsNullOrWhiteSpace(Message))
				{
                    if (IsInternetAvailable())
                    {
						Timestamp ts = Timestamp.GetCurrentTimestamp();
						//Debug.WriteLine(DateTime.Now.ToLocalTime().ToString());

						Messages.Add(new MessageModel
						{
							Message = Message,
							Username = usrName,
							Time = "Today at " + ts.ToDateTime().ToLocalTime().ToString("hh:mm tt"),
						});

						addMsgs((usrName), Encrypt(Message), ts);
                    }
                    else
                    {
                        while (!IsInternetAvailable())
                        {
							NetworkCard.Background = new SolidColorBrush(ConvertStringToColor("#FF5733"));
							InsideNetworkCard.Text = "Please check your connection";
							await Task.Delay(2000);
                        }
						InsideNetworkCard.Text = "";
						NetworkCard.Background = null; // transparent
					}

                }
				Message = "";
            });

			// Adding childs of Contact
			addValue(isOnline, usrName);
			isOnlineContact(isOnline);

        }


		#region Messages!
		async void loadMsgs(string usrName, ListView lstView)
		{
            string msg = null, usrname = null, time = null; 

			//Query query = db.Collection(@"Msgs").OrderByDescending("Time");
			
            Query query = db.Collection(@"Msgs").WhereEqualTo("UserName",usrName).Limit(150).OrderByDescending("Time");
            QuerySnapshot snap = await query.GetSnapshotAsync();


            foreach (DocumentSnapshot documentSnapshot in snap.Documents)
            {
                if (documentSnapshot.Exists)
                {
                    Dictionary<string, object> docs = documentSnapshot.ToDictionary();
                    //Debug.WriteLine("Inside snap"); 
                    foreach (KeyValuePair<string, object> pair in docs)
                    {

                        if (pair.Key == "Message")
                            msg = pair.Value.ToString();
                        if (pair.Key == "UserName")
                            usrname = pair.Value.ToString();
                        if (pair.Key == "Time")
                            time = pair.Value.ToString();



                        if (msg != null && usrname != null && time != null)
                        {
							var found = Messages.FirstOrDefault(i => i.Time == time);

                            if (found == null)
                            { //isOnline(eventReceived.Object.timeStamp, DateTime.Now)
                              // Alright

                                #region Time Conversion
                                string temp = DateTime.ParseExact(time.ToString().Replace("Timestamp:", "").Trim(),
                                            "yyyy-MM-ddTHH:mm:ss.ffffffK", null).ToString();
                                DateTime tempTime = Convert.ToDateTime(temp);
                                string tempTime1 = tempTime.ToShortDateString();


								string forToday = DateTime.Today.ToShortDateString();
								string forYesterday = DateTime.Today.AddDays(-1).ToShortDateString();
								

								string TimeAns = "";
								if(tempTime1 == forToday)	TimeAns = "Today at " + tempTime.ToShortTimeString();
								else if(tempTime1 == forYesterday)	TimeAns = "Yesterday at " + tempTime.ToShortTimeString();
                                else  TimeAns = temp;
                                
                                #endregion

                                Messages.Add(new MessageModel
								{
									Message = Decrypted(msg),
									Username = (usrname),
									Time = TimeAns
								});
								lstView.ScrollIntoView(lstView.Items.Count - 1);
								//Debug.WriteLine("username: " + usrname);
								//Debug.WriteLine("msg: " + msg);
								//Debug.WriteLine("time: " +time);

								msg = null; usrname = null; time = null;

                            }
                        }
                    }
                }
            }
		}

		async void addMsgs(string usrName, string Message, Timestamp time)
        {
			DocumentReference newMsg = db.Collection(@"Msgs").Document();

			Dictionary<string, object> data = new Dictionary<string, object>()
			{
				{"Message", Message},
				{"UserName",  usrName},
				{"Time", time} 
			};
			await newMsg.SetAsync(data); 
			
		}
		void SetListenerMsgs(string usrName, string Message, ListView lstView)
		{
			// FIRESTORE 
			string msg = null, usrname = null, time = null;

			//Query qr = db.Collection(@"Msgs").OrderBy("Time").Limit(1); 
			//QuerySnapshot snapshot = await qr.GetSnapshotAsync(); 

			Query query = db.Collection(@"Msgs").OrderBy("Time"); //.WhereLessThanOrEqualTo("Time", TIME); 


			//Debug.WriteLine("================= Inside set listener msg================= "); 

			FirestoreChangeListener listener = query.Listen(snapshot =>
			{
				foreach (DocumentChange change in snapshot.Changes)
				{
					//Debug.WriteLine(" ================= Not working ================= ");
					if (change.ChangeType.ToString() == "Added")
					{
						//MessageBox.Show("Msgs added");
						//Debug.WriteLine(" ================= Added ================= ");
						//Debug.WriteLine("Ref: "+ change.Document.Id);
						Application.Current.Dispatcher.BeginInvoke(async () =>
						{
							DocumentReference docRef = db.Collection(@"Msgs").Document(change.Document.Id);
							// cannot convert from docSnapshot to DocReference ^
							DocumentSnapshot snap = await docRef.GetSnapshotAsync();

							if (snap.Exists)
							{
								//Debug.WriteLine(" ================= Inside snap ================= ");

								// Debug.WriteLine
								//Debug.WriteLine("Snap ID: "+ snap.Id); 
								Dictionary<string, object> docs = snap.ToDictionary();
								//Debug.WriteLine("Inside snap");
								foreach (KeyValuePair<string, object> pair in docs)
								{
									//Debug.WriteLine(" ================= Inside foreach ================= ");

									if (pair.Key == "Message") msg = pair.Value.ToString();
									if (pair.Key == "UserName") usrname = pair.Value.ToString();
									if (pair.Key == "Time") time = pair.Value.ToString();

									//Debug.WriteLine(msg);
									//Debug.WriteLine(time);
									//Debug.WriteLine(usrname); 


									if (msg != null && usrname != null && time != null)
									{
										//Debug.WriteLine(" ================= Inside if ================= ");
										//    var found = Messages.FirstOrDefault(i => i.Time == time);

										//    if (found == null)
										//    { //isOnline(eventReceived.Object.timeStamp, DateTime.Now)


										#region Time Conversion
										string temp = DateTime.ParseExact(time.ToString().Replace("Timestamp:", "").Trim(),
													"yyyy-MM-ddTHH:mm:ss.ffffffK", null).ToString();
										DateTime tempTime = Convert.ToDateTime(temp);
										string tempTime1 = tempTime.ToShortDateString();


										string forToday = DateTime.Today.ToShortDateString();
										string forYesterday = DateTime.Today.AddDays(-1).ToShortDateString();


										string TimeAns = "";
										if (tempTime1 == forToday) TimeAns = "Today at " + tempTime.ToShortTimeString();
										else if (tempTime1 == forYesterday) TimeAns = "Yesterday at " + tempTime.ToShortTimeString();
										else TimeAns = temp;

										#endregion


										{
											
											Messages.Add(new MessageModel
											{
												Message = Decrypted(msg),
												Username = (usrname),
												Time = TimeAns
											});
										}
										lstView.ScrollIntoView(lstView.Items.Count - 1);

										/*Debug.WriteLine("username: " + usrname);
										Debug.WriteLine("msg: " + msg);
										Debug.WriteLine("time: " + time);*/

										msg = null; usrname = null; time = null;

										//    }
									}
								}
							}
							CollectionViewSource.GetDefaultView(Messages).Refresh();



							/*Query query = db.Collection(@"Msgs").OrderBy("Time").Limit(150); //.Database.Document(change.Document.Id).GetSnapshotAsync(); 
							QuerySnapshot snap1 = await query.GetSnapshotAsync();*/

							//CollectionViewSource.GetDefaultView(Messages).Refresh();
						});
					}
				}
			});
		}
		#endregion


		#region Contacts
		void addValue(bool isOnline, string usrName)
		{
			try
			{
				DocumentReference coll = db.Collection("isOnline").Document(usrName);
				//Debug.WriteLine("========= Inside addValue (Contacts) ========= " + isOnline);
				Dictionary<string, object> data = new Dictionary<string, object>()
				{
					{"isOnline", isOnline}
				};
				if (isOnline)
				{
					coll.SetAsync(data);
				}
            }
			catch { }

		}

		public void isOnlineContact(bool isOnline)
        {
            //This is firestore
            try
            {
				DocumentReference contactRef = db.Collection(@"isOnline").Document(usrName);
				Dictionary<string, object> updates = new Dictionary<string, object> 
				{
					{ "isOnline", isOnline },
				}; 
				contactRef.SetAsync(updates);
				Thread.Sleep(1500);
			
				SetListenerContact(); 	
            }
			catch { }
		}
		
		void SetListenerContact() 
        {
			// FIRESTORE
			Query query = db.Collection(@"isOnline").WhereEqualTo("isOnline", true);
            //Debug.WriteLine("Inside set listener contact");
			
			FirestoreChangeListener listener = query.Listen(snapshot => 
			{
				foreach (DocumentChange change in snapshot.Changes) 
				{
					if (change.ChangeType.ToString() == "Added")
					{
						Application.Current.Dispatcher.BeginInvoke(() => // OnAdd
						{
							Contacts.Add(new ContactModel
							{
								Username = change.Document.Id,
							});
							//Debug.WriteLine(change.Document.Id.ToString()); 

							//CollectionViewSource.GetDefaultView(Contacts).Refresh();
						});
					}
					else if (change.ChangeType.ToString() == "Modified")
					{
						// if(!change.Document.Id) 
                        {
                            Application.Current.Dispatcher.BeginInvoke(() => // OnModify
                            {
								Contacts.Remove(new ContactModel
								{
									Username = change.Document.Id,
								});

								//CollectionViewSource.GetDefaultView(Contacts).Refresh();
							}); 
						}

						// Debug.WriteLine(change.Document.Id); 
					}
					else if (change.ChangeType.ToString() == "Removed")
					{
						Application.Current.Dispatcher.BeginInvoke(() => // OnRemove
						{
							Contacts.Remove(new ContactModel
							{
								Username = change.Document.Id,
							});

							//CollectionViewSource.GetDefaultView(Contacts).Refresh();
						});	
					}
				}
			});


			Query query1 = db.Collection(@"isOnline").WhereEqualTo("isOnline", false);
			FirestoreChangeListener listener1 = query1.Listen(snapshot => 
			{
				foreach (DocumentChange change in snapshot.Changes)
				{
					if (change.ChangeType.ToString() == "Modified")
					{
						// if(!change.Document.Id)  
                        {
                            Application.Current.Dispatcher.BeginInvoke(() => // OnAdd
                            {
								Contacts.Remove(new ContactModel
								{
									Username = change.Document.Id, 
								}); 

								//CollectionViewSource.GetDefaultView(Contacts).Refresh();
							});
						}

					}
				}
			});

        }
        #endregion


        #region Encryption/Decryption
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
		#endregion


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
	}
}