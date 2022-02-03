using sturvey_app.Data;
using sturvey_app.Users;
using sturvey_app.Surveys;
using ID = System.Int32;
using System;

namespace sturvey_app.Tests
{
    public class Tests
    {
        public static void test_save_to_disk()
        {

        }

        public static void test_load_from_disk()
        {
            DataBase db = DataBase.get_instance();
            User user = new User(206228751);
            db.add_to_table("Users", user);
            db.add_to_table("Users", user);
            db.add_to_table("Users", user);
        }
    }
}
