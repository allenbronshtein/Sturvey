using sturvey_app.Surveys;
using sturvey_app.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ID = System.Int32;
namespace sturvey_app.Components
{
    public class Session
    {
        private NetworkStream m_stream_;
        private TcpClient m_client_;
        private User m_user_;
        public Session(TcpClient client)
        {
            m_client_ = client;
            m_stream_ = client.GetStream();
            m_user_ = default(User);
        }

        public void host_api()
        {
            Byte[] bytes = new Byte[1024];
            int i;
            string command = null;
            while ((i = m_stream_.Read(bytes, 0, bytes.Length)) != 0)
            {
                command = Encoding.ASCII.GetString(bytes, 0, i);
                Console.WriteLine("Received: {0}", command);
            }
        }

    }

    public class UserManager
    {
        private static UserManager m_instance_ = new UserManager();
        public static UserManager get_instance() { return m_instance_; }

        private UserManager() { }

        //-----------User API--------------//
        public void sign_up() { }
        public void login() { }
        public void logout() { }
        public void delete_user() { }
        //--------------------------------//
    }

    public class SurveyManager
    {
        private static SurveyManager m_instance_ = new SurveyManager();
        public static SurveyManager get_instance() { return m_instance_; }

        private SurveyManager() { }
    }
}
