using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace ChatApp.MVVM.ViewModel
{
    [FirestoreData]

    class FireOnline
    {
        [FirestoreProperty]
        public bool isOnline { get; set; }
    }
}
