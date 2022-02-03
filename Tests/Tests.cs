using sturvey_app.Data;
using sturvey_app.Users;
using ID = System.Int32;
using System;

namespace sturvey_app.Tests
{
    public class Tests
    {
        public static void test_save_to_disk()
        {
            Data.DataBase db = Data.DataBase.get_instance();
            db.create_table("Users", typeof(User));
            db.add_to_table("Users", new User(1234));
            db.add_to_table("Users", new User(4567));
            db.add_to_table("Users", new User(15245));
        }

        public static void test_load_from_disk()
        {
            Data.DataBase db = Data.DataBase.get_instance();
        }
    }
}
