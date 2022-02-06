using sturvey_app.Components;
using sturvey_app.Data;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System;
using sturvey_app.Comands;

namespace main
{
    public class main
    {
        private static void start_app() {
            EventManager event_manager = EventManager.get_instance();
            DataBase dataBase = DataBase.get_instance();
            SurveyManager survey_manager = SurveyManager.get_instance();
            SessionManager session_manager = SessionManager.get_instance();
            UserManager user_manager = UserManager.get_instance();
            hostAPI host_API = hostAPI.get_instance();
            Logger logger = Logger.get_instance();
            TcpListener server = new TcpListener(IPAddress.Parse("127.0.0.1"), 6000);
            server.Start();
            EventManager.get_instance().raise(new Event(event_title.SERVER_ON_EVENT, status.SUCCESS, "Server on"));

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("New connection from: {0}", client.Client.RemoteEndPoint.ToString());
                session_manager.create_session(client);
            }
        }
        static void Main(string[] args)
        {
            start_app();

        }
    }
}
