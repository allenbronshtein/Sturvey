using sturvey_app.Surveys;
using sturvey_app.Users;

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
            db.delete_from_table("Users",15245);
            db.delete_from_table("Users", 15245);

            db.create_table("Surveys", typeof(Survey));
            db.add_to_table("Surveys", new Survey(1234));
            db.add_to_table("Surveys", new Survey(4567));
            db.add_to_table("Surveys", new Survey(15245));
            db.delete_from_table("Surveys",15246);
        }

        public static void test_load_from_disk()
        {
            Data.DataBase db = Data.DataBase.get_instance();
        }
    }
}
