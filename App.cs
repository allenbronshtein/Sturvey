using sturvey_app.Components;
using sturvey_app.Data;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace main
{
    class main
    {
        static void Main(string[] args)
        {
            DataBase dataBase = DataBase.get_instance();
            SurveyManager survey_manager = SurveyManager.get_instance();
            UserManager user_manager = UserManager.get_instance();
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 6000);
            server.Start();

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Session session = new Session(client);
                Thread thread = new Thread(session.host_api);
                thread.Start();
            }
        }
    }
}
