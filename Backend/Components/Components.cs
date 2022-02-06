using sturvey_app.Comands;
using sturvey_app.Data;
using sturvey_app.Security;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ID = System.Int32;
using Requester = System.Tuple<int, int>; // <UID,SID>
namespace sturvey_app.Components
{
    public interface IExecuter
    {
        Event execute(Command command, Requester tuple);
    } //Implemented by classes that can execute commands 
    public interface IEventHandler {
        void sign();
        void queue(Event evt);
        void raise(Event evt);
    } //Implemented by classes that want to raise/receive events

    public class UserManager : IExecuter
    {
        private static UserManager m_instance_ = new UserManager();
        public static UserManager get_instance() { return m_instance_; }
        private UserManager()
        {
            m_event_handler_ = EventHandler.get_instance();
        }

        private EventHandler m_event_handler_;

        //------------------API---------------------//
        public Event sign_up(string[] args, Requester requester)
        {
            Event evt = new Event();
            return evt;
        }
        public Event login(string[] args, Requester requester)
        {
            Event evt = new Event();
            return evt;
        }
        public Event logout(string[] args, Requester requester)
        {
            Event evt = new Event();
            return evt;
        }
        public Event delete_user(string[] args, Requester requester)
        {
            Event evt = new Event();
            return evt;
        }

        public Event execute(Command command, Requester requester)
        {
            return command.Request(command.Args,requester);
        }
        //------------------------------------------//


        private class EventHandler : IEventHandler
        {
            private static EventHandler m_instance_ = new EventHandler();
            public static EventHandler get_instance() { return m_instance_; }

            private EventHandler()
            {
                sign();
                Thread handler = new Thread(handler_task);
                handler.Start();
            }
            private Queue<Event> m_event_queue_ = new Queue<Event>();

            public void handler_task()
            {
                while (true)
                {
                    Thread.Sleep(500);
                    while (m_event_queue_.Count != 0)
                    {
                        handle();
                    }
                }
            }
            private void handle()
            {
                Event evt = m_event_queue_.Dequeue();
            }
            public void queue(Event evt)
            {
                m_event_queue_.Enqueue(evt);
            }
            public void raise(Event evt)
            {
                EventManager.get_instance().raise(evt);
            }
            public void sign()
            {
                EventManager.get_instance().sign(this);
            }
        }
    } // Executer

    public class SurveyManager : IExecuter
    {
        private static SurveyManager m_instance_ = new SurveyManager();
        public static SurveyManager get_instance() { return m_instance_; }
        private SurveyManager() {
            m_event_handler_ = EventHandler.get_instance();
        }

        private EventHandler m_event_handler_;

        //------------------API---------------------//
        public Event create_survey(string[] args, Requester requester)
        {
            Event evt = new Event();
            return evt;
        }
        public Event vote_survey(string[] args, Requester requester)
        {
            Event evt = new Event();
            return evt;
        }
        public Event view_survey(string[] args, Requester requester)
        {
            Event evt = new Event();
            return evt;
        }
        public Event delete_survey(string[] args, Requester requester)
        {
            Event evt = new Event();
            return evt;
        }
        public Event clear_survey(string[] args, Requester requester)
        {
            Event evt = new Event();
            return evt;
        }

        public Event execute(Command command,Requester requester)
        {
            return command.Request(command.Args,requester);
        }
        //------------------------------------------//


        private class EventHandler : IEventHandler
        {
            private static EventHandler m_instance_ = new EventHandler();
            public static EventHandler get_instance() { return m_instance_; }

            private EventHandler()
            {
                sign();
                Thread handler = new Thread(handler_task);
                handler.Start();
            }
            private Queue<Event> m_event_queue_ = new Queue<Event>();

            public void handler_task()
            {
                while (true)
                {
                    Thread.Sleep(500);
                    while (m_event_queue_.Count != 0)
                    {
                        handle();
                    }
                }
            }
            private void handle()
            {
                Event evt = m_event_queue_.Dequeue();
            }
            public void queue(Event evt)
            {
                m_event_queue_.Enqueue(evt);
            }
            public void raise(Event evt)
            {
                EventManager.get_instance().raise(evt);
            }
            public void sign()
            {
                EventManager.get_instance().sign(this);
            }
        }

    } // Executer

    public class EventManager
    {
        private static EventManager m_instance_ = new EventManager();
        public static EventManager get_instance() { return m_instance_; }
        private EventManager() { }

        private List<IEventHandler> handlers = new List<IEventHandler>();

        public void sign(IEventHandler eventHandler) {
            handlers.Add(eventHandler);
        }
        public void raise(Event evt)
        {
            foreach(IEventHandler handler in handlers)
            {
                handler.queue(evt);
            }
        }
    }

    public class SessionManager
    {
        private int MAX_SESSIONS = 100; 
        private class Session
        {
            private NetworkStream m_stream_;
            private TcpClient m_client_;
            private ID m_user_id_;
            private ID m_session_id_;
            private Byte[] m_in_;
            public Session(TcpClient client, ID session_id)
            {
                m_session_id_ = session_id;
                m_client_ = client;
                m_stream_ = client.GetStream();
                m_in_ = new byte[1024];
                m_user_id_ = default(ID);
            }

            public void run_task()
            {
                int i;
                while ((i = m_stream_.Read(m_in_, 0, m_in_.Length)) != 0)
                {
                    Command command = hostAPI.get_instance().parse(Encoding.ASCII.GetString(m_in_, 0, i));
                    if (command != default(Command))
                    {
                        Event evt = command.Executer.execute(command, new Requester(m_user_id_, m_session_id_));
                        EventManager.get_instance().raise(evt);
                    }
                }
                m_session_id_ = -1;
            }

            public ID id()
            {
                return m_session_id_;
            }
            public ID UserID
            {
                get { return m_user_id_; }
                set { m_user_id_ = value; }
            }
        }
        private static SessionManager m_instance_ = new SessionManager();
        public static SessionManager get_instance() { return m_instance_; }

        private SessionManager()
        {
            m_event_handler_ = EventHandler.get_instance();
            m_event_handler_.setHandlerField(sessions);
            m_createM_ = new Mutex();
            m_deleteM_ = new Mutex();
            session_cleaner = new Thread(sessions_cleaner_task);
            session_cleaner.Start();
        }

        private Dictionary<ID, Session> sessions = new Dictionary<ID, Session>();
        private Mutex m_createM_, m_deleteM_;
        private Queue<ID> queue_idx = new Queue<ID>();
        private ID count_sidx = (ID)SID.USER_INIT_SID;
        private Thread session_cleaner;
        private EventHandler m_event_handler_;

        public void create_session(TcpClient client)
        {
            m_createM_.WaitOne();
            ID session_id = gen_session_id();
            if (session_id != (ID)SID.UNAVAILABLE_SID)
            {
                Session session = new Session(client, session_id);
                sessions[session_id] = session;
                Thread sessions_runner = new Thread(session.run_task);
                sessions_runner.Start();
            }
            m_createM_.ReleaseMutex();
        }
        public void remove_session(ID session_id)
        {
            m_deleteM_.WaitOne();
            if (sessions.ContainsKey(session_id))
            {
                sessions.Remove(session_id);
                queue_idx.Enqueue(session_id);
            }
            m_deleteM_.ReleaseMutex();
        }

        private void sessions_cleaner_task() {
            while (true)
            {
                List<ID> remove_list = new List<ID>();
                foreach(KeyValuePair<ID,Session> pair in sessions)
                {
                    ID session_id = pair.Key;
                    Session session = pair.Value;
                    if(session.id() == (int)SID.UNAVAILABLE_SID || session.id() != session_id)
                    {
                        remove_list.Add(session_id);
                    }
                }
                foreach(ID session_id in remove_list)
                {
                    remove_session(session_id);
                }
                System.Threading.Thread.Sleep(500);
            }
        }
        private ID gen_session_id()
        {
            ID session_id = (ID)SID.UNAVAILABLE_SID;
            if (sessions.Count < MAX_SESSIONS)
            {
                if (queue_idx.Count > 0)
                {
                    session_id = queue_idx.Dequeue();
                    if (sessions.ContainsKey(session_id))
                    {
                        session_id = (ID)SID.UNAVAILABLE_SID;
                    }
                }
                else
                {
                    session_id = count_sidx++;
                }
            }
            return session_id;
        }

        private class EventHandler : IEventHandler
        {
            private static EventHandler m_instance_ = new EventHandler();
            public static EventHandler get_instance() { return m_instance_; }

            private EventHandler()
            {
                sign();
                Thread handler = new Thread(handler_task);
                handler.Start();
            }

            private Queue<Event> m_event_queue_ = new Queue<Event>();
            //-----------Handled Class fields--------------//
            private Dictionary<ID, Session> m_sessions_;
            public void setHandlerField(Dictionary<ID,Session> sessions)
            {
                m_sessions_ = sessions;
            }
            //---------------------------------------------//
            public void handler_task()
            {
                while (true)
                {
                    Thread.Sleep(500);
                    while (m_event_queue_.Count != 0)
                    {
                        handle();
                    }
                }
            }
            private void handle()
            {
                Event evt = m_event_queue_.Dequeue();
                int title = (int)evt.Title;
                switch (title)
                {
                    case (int)event_title.ADMIN_LOGIN_EVENT:
                        if(evt.Status == status.SUCCESS)
                        {
                            m_sessions_[evt.SID].UserID = (ID) UID.ADMIN_UID;  
                        }
                        break;
                }
            }
            public void queue(Event evt)
            {
                m_event_queue_.Enqueue(evt);
            }
            public void raise(Event evt)
            {
                EventManager.get_instance().raise(evt);
            }
            public void sign()
            {
                EventManager.get_instance().sign(this);
            }
        }
    }

    public class Logger : IEventHandler
    {
        private static Logger m_instance_ = new Logger();
        public static Logger get_instance() { return m_instance_; }

        private Logger() {
            sign();
            Thread handler = new Thread(handler_task);
            handler.Start();
        }
        private Queue<Event> m_event_queue_ = new Queue<Event>();

        public void handler_task()
        {
            while (true)
            {
                Thread.Sleep(500);
                while (m_event_queue_.Count != 0)
                {
                    handle();
                }
            }
        }
        private void handle()
        {
           Event evt = m_event_queue_.Dequeue();
            if (evt.Message != default(string))
            {
                Console.WriteLine("{0}", evt.Message);
            }
        }
        public void queue(Event evt)
        {
            m_event_queue_.Enqueue(evt);
        }
        public void raise(Event evt)
        {
            EventManager.get_instance().raise(evt);
        }
        public void sign()
        {
            EventManager.get_instance().sign(this);
        }
    }

    public class hostAPI
    {
        private static hostAPI m_instance_ = new hostAPI();
        public static hostAPI get_instance() { return m_instance_; }

        private hostAPI() {
            m_event_handler_ = EventHandler.get_instance();
        }

        private EventHandler m_event_handler_;

        public Command parse(string user_input)
        {

            Command command = default(Command);
            user_input = user_input.Trim();
            string[] args = user_input.Split(' ');
            switch (args[0])
            {
                // login $user_id $password 
                case "login":
                    if (args.Length == 3)
                    {
                        command = new Command(args, UserManager.get_instance(), UserManager.get_instance().login);
                    }
                    break;
                // adduser $user_id $password
                case "adduser":
                    if (args.Length == 3)
                    {
                        command = new Command(args, UserManager.get_instance(), UserManager.get_instance().sign_up);
                    }
                    break;
                // logout 
                case "logout":
                    if (args.Length == 1)
                    {
                        command = new Command(args, UserManager.get_instance(), UserManager.get_instance().logout);
                    }
                    break;
                // rmuser
                case "rmuser":
                    if (args.Length == 1)
                    {
                        command = new Command(args, UserManager.get_instance(), UserManager.get_instance().delete_user);
                    }
                    break;
                // addsurvey $file_name
                case "addsurvey":
                    if (args.Length == 2)
                    {
                        command = new Command(args, SurveyManager.get_instance(), SurveyManager.get_instance().create_survey);
                    }
                    break;
                // rmsurvey $survey_id
                case "rmsurvey":
                    if (args.Length == 2)
                    {
                        command = new Command(args, SurveyManager.get_instance(), SurveyManager.get_instance().delete_survey);
                    }
                    break;
                // vote $survey_id 0,0,0,...,0 (index is question number, value is answer number)
                case "vote":
                    if (args.Length == 3)
                    {
                        command = new Command(args, SurveyManager.get_instance(), SurveyManager.get_instance().vote_survey);
                    }
                    break;
                // view $survey_id
                case "view":
                    if (args.Length == 2)
                    {
                        command = new Command(args, SurveyManager.get_instance(), SurveyManager.get_instance().view_survey);
                    }
                    break;
                // clear $survey_id
                case "clear":
                    if (args.Length == 2)
                    {
                        command = new Command(args, SurveyManager.get_instance(), SurveyManager.get_instance().clear_survey);
                    }
                    break;
                // admin $passcode
                case "admin":
                    if (args.Length == 2)
                    {
                        command = new Command(args, AdminSpace.get_instance(), AdminSpace.get_instance().login);
                    }
                    break;
                // addtable $name $type
                case "addtable":
                    if (args.Length == 3)
                    {
                        command = new Command(args, AdminSpace.get_instance(), AdminSpace.get_instance().create_table);
                    }
                    break;
                // rmtable $name
                case "rmtable":
                    if (args.Length == 2)
                    {
                        command = new Command(args, AdminSpace.get_instance(), AdminSpace.get_instance().delete_table);
                    }
                    break;
                default:
                    break;
            }
            return command;
        }

        private class EventHandler : IEventHandler
        {
            private static EventHandler m_instance_ = new EventHandler();
            public static EventHandler get_instance() { return m_instance_; }

            private EventHandler()
            {
                sign();
                Thread handler = new Thread(handler_task);
                handler.Start();
            }
            private Queue<Event> m_event_queue_ = new Queue<Event>();

            public void handler_task()
            {
                while (true)
                {
                    Thread.Sleep(500);
                    while (m_event_queue_.Count != 0)
                    {
                        handle();
                    }
                }
            }
            private void handle()
            {
                Event evt = m_event_queue_.Dequeue();
            }
            public void queue(Event evt)
            {
                m_event_queue_.Enqueue(evt);
            }
            public void raise(Event evt)
            {
                EventManager.get_instance().raise(evt);
            }
            public void sign()
            {
                EventManager.get_instance().sign(this);
            }
        }

    }

    public class AdminSpace : IExecuter
    {
        private static AdminSpace m_instance_ = new AdminSpace();
        public static AdminSpace get_instance() { return m_instance_; }
        IEventHandler m_event_handler_;
        private AdminSpace()
        {
            m_event_handler_ = EventHandler.get_instance();
        }

        private string m_key_hash_ = "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92";

        public Event login(string[]args, Requester requester) {
            Event evt = new Event();
            evt.setTitle(event_title.ADMIN_LOGIN_EVENT).setStatus(status.FAIL).setDatetime(DateTime.Now).setSID(requester.Item2);
            if (Hash.ComputeSha256Hash(args[1]) == m_key_hash_)
            {
                evt.setStatus(status.SUCCESS).setMessage("Admin logged in");
            }
            return evt;
        }
        public Event create_table(string[] args, Requester requester) {
            Event evt = new Event();
            status status = status.FAIL;
            evt.setTitle(event_title.NEW_TABLE_EVENT).setDatetime(DateTime.Now).setSID(requester.Item2);
            if (requester.Item1 != (ID) UID.ADMIN_UID)
            {
                evt.setStatus(status);
            }
            else
            {
                status = DataBase.get_instance().create_table(args[1], "sturvey_app.Users." + args[2]);
                evt.setStatus(status);
                if (status == status.SUCCESS)
                {
                    evt.setMessage("Created Table");
                }
            }
            return evt;
        }
        public Event delete_table(string[] args,Requester requester) {
            Event evt = new Event();
            status status = status.FAIL;
            evt.setTitle(event_title.DELETE_TABLE_EVENT).setDatetime(DateTime.Now).setSID(requester.Item2);
            if (requester.Item1 != (ID) UID.ADMIN_UID)
            {
                evt.setStatus(status);
            }
            else
            {
                status = DataBase.get_instance().delete_table(args[1]);
                evt.setStatus(status);
                if (status == status.SUCCESS)
                {
                    evt.setMessage("Deleted Table");
                }
            }
            return evt;
        }

        public Event execute(Command command, Requester requester)
        {
            return command.Request(command.Args, requester);
        }

        private class EventHandler : IEventHandler
        {
            private static EventHandler m_instance_ = new EventHandler();
            public static EventHandler get_instance() { return m_instance_; }

            private EventHandler()
            {
                sign();
                Thread handler = new Thread(handler_task);
                handler.Start();
            }
            private Queue<Event> m_event_queue_ = new Queue<Event>();

            public void handler_task()
            {
                while (true)
                {
                    Thread.Sleep(500);
                    while (m_event_queue_.Count != 0)
                    {
                        handle();
                    }
                }
            }
            private void handle()
            {
                Event evt = m_event_queue_.Dequeue();
            }
            public void queue(Event evt)
            {
                m_event_queue_.Enqueue(evt);
            }
            public void raise(Event evt)
            {
                EventManager.get_instance().raise(evt);
            }
            public void sign()
            {
                EventManager.get_instance().sign(this);
            }
        }
    } // Executer
}
