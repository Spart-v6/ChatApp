using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.MVVM.Model
{
	class MessageModel
	{
		public string Username { get; set; }
		public string imageSource { get; set; }
		public string Message { get; set; }
		public string Time { get; set; }
		//public bool isNativeOrigin { get; set; }
		public bool isOnline { get; set; }
		public bool? FirstMessage { get; set; }

		/*public override bool Equals(object obj)
		{
			if (obj == null || !(obj is MessageModel))
				return false;
			else
				return this.Time == ((MessageModel)obj).Time;
		}*/
		/*public override int GetHashCode()
		{
			return this.Time.GetHashCode();
		}*/
	}
}
