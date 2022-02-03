using sturvey_app.Data;
using sturvey_app.Users;
using sturvey_app.Surveys;
using ID = System.Int32;

namespace sturvey_app.Tests
{
    public class Tests
    {
        public static void test_save_to_disk()
        {
            DataBase db = DataBase.get_instance();
            db.create_table("Users",typeof(User));
            ITable table = db.get_table("Users");
            User user1 = new User(1);
            User user2 = new User(2);
            User user3 = new User(3);
            
            table.add(user1);
            table.add(user2);
            table.add(user3);
            db.save_to_disk();
        }

        public static void test_load_from_disk()
        {
        }
    }
}
