using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace ChatApp.MVVM.ViewModel
{
    [FirestoreData]
    public class FireMsg
    {
        [FirestoreProperty]
        public string NameUser { get; set; }
        [FirestoreProperty]
        public string text { get; set; }
        [FirestoreProperty]
        public string time { get; set; }
    }
}
