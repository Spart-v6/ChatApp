using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp
{
    class MyUser
    {
        public string Username { get; set; }
        public string Nickname { get; set; }
        public string Password { get; set; }


        //public static string error = "Oops! There was some error";
        public static char err = 'o';

        public static bool IsEqual(MyUser user1, MyUser user2)
        {
            if (user1 == null || user2 == null) { return false; }
            if (user1.Username != user2.Username)
            {
                //error = "Username does not exist!";
                err = 'u';
                return false;
            }
            else if (user1.Password != user2.Password)
            {
                //error = "Username or password does not match!";
                err = 'p';
                return false;
            }
            return true;
        }
    }
}
