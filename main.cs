using sturvey_app.Data;
using System.Diagnostics;

namespace main
{
    class main
    {
        static void Main(string[] args)
        {
            DataBase db = DataBase.get_instance();
            db.save_to_disk();
        }
    }
}
