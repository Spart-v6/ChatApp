using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.MVVM.Model
{
	class ContactModel
	{
		public string Username { get; set; }
		public bool isOnlineUsrname { get; set; }


        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is ContactModel))
                return false;
            else
                return this.Username == ((ContactModel)obj).Username;
        }
        public override int GetHashCode()
        {
            return this.Username.GetHashCode(); // diff? contact model contains this, msgsmodel doesnt'
        } // ? // It helps remove()  coz there u create new object...which is...compared to old object and removed
        // this is just for i guess RemoveItem () function? which is written below
        // Nah it works for remove() too coz there's also comparison with new()
        // ayt then remove isn't needed for messageModel

        /*public void RemoveItem(ObservableCollection<ContactModel> collection, ContactModel instance)
        {
            collection.Remove(collection.Where(i => i.Username == instance.Username).Single());
        }*/
        // Other differences? wait ?  ChatItem.xaml?  i've opned it

        //public ObservableCollection<MessageModel> Messages { get; set; }
        //public string LastMessage => Messages.Last().Message;
    }
}
