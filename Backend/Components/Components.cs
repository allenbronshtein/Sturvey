using sturvey_app.Comands;
using sturvey_app.Data;
using sturvey_app.Security;
using sturvey_app.Uniques;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ID = System.Int32;
using Newtonsoft.Json;
using sturvey_app.Backend.Parsers;
using System.IO;

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
            evt.setTitle(event_title.NEW_USER_EVENT).setStatus(status.FAIL).setDatetime(DateTime.Now).setSID(requester.SID);
            if(requester.UID != default(ID))
            {
                evt.setResponse("Already logged in");
                return evt;
            }
            ID user_id;
            try
            {
                user_id = Int32.Parse(args[1]);
                if(user_id <= 0)
                {
                    evt.setResponse("User id cannot be less than 1");
                    return evt;
                }
            }
            catch (FormatException)
            {
                evt.setResponse("User id must contain only numbers");
                return evt;
            }            
            string password = args[2];
            IUnique item = DataBase.get_instance().read_from_table("Users", user_id);
            if(item != default(IUnique))
            {
                evt.setResponse("User already exists");
                return evt;
            }
            User user = new User(user_id, password);
            status st = DataBase.get_instance().add_to_table("Users", user);
            if(st == status.FAIL)
            {
                evt.setResponse("Unknown error - Failed adding user");
                return evt;
            }
            evt.setStatus(status.SUCCESS).setUID(user_id).setResponse("Welcome");
            return evt;
        }
        public Event login(string[] args, Requester requester)
        {
            Event evt = new Event();
            ID user_id;
            if (requester.UID != default(ID))
            {
                evt.setResponse("Already logged in");
                return evt;
            }
            evt.setTitle(event_title.NEW_USER_EVENT).setStatus(status.FAIL).setDatetime(DateTime.Now).setSID(requester.SID);
            try
            {
                user_id = Int32.Parse(args[1]);
                if (user_id <= 0)
                {
                    evt.setResponse("Can not find user");
                    return evt;
                }
            }
            catch (FormatException)
            {
                evt.setResponse("Can not find user");
                return evt;
            }
            string password = args[2];
            IUnique item = DataBase.get_instance().read_from_table("Users", user_id);
            if (item == default(IUnique))
            {
                evt.setResponse("Can not find user");
                return evt;
            }
            User user = (User)item;
            if(user.Password != Hash.ComputeSha256Hash(password))
            {
                evt.setResponse("Incorrect password");
                return evt;
            }
            evt.setStatus(status.SUCCESS).setResponse("Welcome!").setUID(user_id);



            return evt;
        }
        public Event logout(string[] args, Requester requester)
        {
            Event evt = new Event();
            evt.setTitle(event_title.LOGOUT_EVENT).setStatus(status.FAIL).setDatetime(DateTime.Now).setSID(requester.SID);
            if(requester.UID == default(ID))
            {
                evt.setResponse("Not logged in");
                return evt;
            }
            evt.setStatus(status.SUCCESS).setResponse("Logged out");
            return evt;
        }
        public Event delete_user(string[] args, Requester requester)
        {
            Event evt = new Event();
            evt.setTitle(event_title.DELETE_USER_EVENT).setStatus(status.FAIL).setDatetime(DateTime.Now).setSID(requester.SID);
            if(requester.UID == default(ID))
            {
                evt.setResponse("Not logged in");
                return evt;
            }
            if (requester.UID == (ID)UID.ADMIN_UID)
            {
                evt.setResponse("Cannot remove user");
                return evt;
            }
            User user = (User) DataBase.get_instance().read_from_table("Users", requester.UID);
            foreach(ID survey_id in user.SurveysByMe)
            {
                string[] _args = {"rmsurvey",survey_id.ToString()};
                SurveyManager.get_instance().delete_survey(_args, requester);
            }
            foreach(ID survey_id in user.SurveysToMe)
            {
                Survey _survey = (Survey)DataBase.get_instance().read_from_table("Surveys", survey_id);
                List<ID> users = _survey.Users;
                users.Remove(requester.UID);
                _survey.Users = users;
            }
            DataBase.get_instance().delete_from_table("Users", requester.UID);
            evt.setStatus(status.SUCCESS).setResponse("User removed");
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
            m_survey_creator_ = SurveyCreator.get_instance();
        }

        private EventHandler m_event_handler_;
        private SurveyCreator m_survey_creator_;

        //------------------API---------------------//
        public Event create_survey(string[] args, Requester requester)
        {
            Event evt = new Event();
            evt.setTitle(event_title.NEW_SURVEY_EVENT).setStatus(status.FAIL).setDatetime(DateTime.Now).setSID(requester.SID);
            if(requester.UID == default(ID))
            {
                evt.setResponse("Please login to create survey");
                return evt;
            }
            User current_user = (User) DataBase.get_instance().read_from_table("Users", requester.UID);
            Survey survey = m_survey_creator_.create(args[1]);
            if(survey != default(Survey))
            {
                DataBase.get_instance().add_to_table("Surveys", survey);
                List<ID> surveys = current_user.SurveysByMe;
                surveys.Add(survey.id());
                current_user.SurveysByMe = surveys;
                List<ID> users = survey.Users;
                foreach(ID id in users)
                {
                    IUnique user = DataBase.get_instance().read_from_table("Users", id);
                    if(user != default(IUnique))
                    {
                        User _user = (User)user;
                        List<ID> user_surveys = _user.SurveysToMe;
                        user_surveys.Add(survey.id());
                    }
                }
                evt.setStatus(status.SUCCESS).setResponse("Survey created");
            }
            return evt;
        }
        public Event vote_survey(string[] args, Requester requester)
        {
            Event evt = new Event();
            evt.setTitle(event_title.VOTE_SURVEY_EVENT).setStatus(status.FAIL).setDatetime(DateTime.Now).setSID(requester.SID);
            return evt;
        }
        public Event view_survey(string[] args, Requester requester)
        {
            Event evt = new Event();
            evt.setTitle(event_title.VIEW_SURVEY_EVENT).setStatus(status.FAIL).setDatetime(DateTime.Now).setSID(requester.SID);
            return evt;
        }
        public Event delete_survey(string[] args, Requester requester)
        {
            Event evt = new Event();
            ID id;
            evt.setTitle(event_title.DELETE_SURVEY_EVENT).setStatus(status.FAIL).setDatetime(DateTime.Now).setSID(requester.SID);
            User user = (User)DataBase.get_instance().read_from_table("Users", requester.UID);
            try
            {
                id = Int32.Parse(args[1]);
            }
            catch(Exception) {
                evt.setResponse("Survey id must be a number");
                return evt;
            }
            if (!user.SurveysByMe.Contains(id))
            {
                evt.setResponse("You are not allowed to remove that survey!");
                return evt;
            }
            if(DataBase.get_instance().read_from_table("Surveys", id) == default(IUnique))
            {
                evt.setResponse("No such survey");
                return evt;
            }
            Survey survey = (Survey) DataBase.get_instance().read_from_table("Surveys", id);
            List<ID> surveys_by_me = user.SurveysByMe;
            surveys_by_me.Remove(id);
            user.SurveysByMe = surveys_by_me;
            foreach(ID user_id in survey.Users)
            {
                User _user = (User)DataBase.get_instance().read_from_table("Users", user_id);
                List<ID> _user_surveys = _user.SurveysToMe;
                _user_surveys.Remove(id);
            }
            evt.setStatus(status.SUCCESS).setResponse("Survey removed");
            return evt;
        }
        public Event clear_survey(string[] args, Requester requester)
        {
            Event evt = new Event();
            evt.setTitle(event_title.CLEAR_SURVEY_EVENT).setStatus(status.FAIL).setDatetime(DateTime.Now).setSID(requester.SID);
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

        public Event login(string[] args, Requester requester)
        {
            Event evt = new Event();
            if (requester.UID != default(ID))
            {
                evt.setResponse("Already logged in");
                return evt;
            }
            evt.setTitle(event_title.ADMIN_LOGIN_EVENT).setStatus(status.FAIL).setDatetime(DateTime.Now).setSID(requester.SID);
            if (Hash.ComputeSha256Hash(args[1]) == m_key_hash_)
            {
                evt.setStatus(status.SUCCESS).setResponse("Welcome!");
                return evt;
            }
            evt.setResponse("Incorrect password");
            return evt;
        }
        public Event create_table(string[] args, Requester requester)
        {
            Event evt = new Event();
            evt.setTitle(event_title.NEW_TABLE_EVENT).setStatus(status.FAIL).setDatetime(DateTime.Now).setSID(requester.SID);
            if (requester.UID != (ID)UID.ADMIN_UID)
            {
                evt.setResponse("Must be admin to invoke command");
            }
            else
            {
                status status = DataBase.get_instance().create_table(args[1], "sturvey_app.Uniques." + args[2]);
                if (status == status.SUCCESS)
                {
                    evt.setStatus(status.SUCCESS).setResponse("Table created");
                }
                else
                {
                    evt.setResponse("Couldn't create table");
                }
            }
            return evt;
        }
        public Event delete_table(string[] args, Requester requester)
        {
            Event evt = new Event();
            evt.setTitle(event_title.DELETE_TABLE_EVENT).setStatus(status.FAIL).setDatetime(DateTime.Now).setSID(requester.SID);
            if (requester.UID != (ID)UID.ADMIN_UID)
            {
                evt.setResponse("Must be admin to invoke command");
            }
            else
            {
                status status = DataBase.get_instance().delete_table(args[1]);
                if (status == status.SUCCESS)
                {
                    evt.setStatus(status.SUCCESS).setResponse("Deleted Table");
                }
                else
                {
                    evt.setResponse("Couldn't delete table");
                }
            }
            return evt;
        }
        public Event stop(string[] args, Requester requester)
        {
            Event evt = new Event();
            evt.setTitle(event_title.DELETE_TABLE_EVENT).setStatus(status.FAIL).setDatetime(DateTime.Now).setSID(requester.SID);
            if (requester.UID != (ID)UID.ADMIN_UID)
            {
                evt.setResponse("Must be admin to invoke command");
            }
            else
            {
                Environment.Exit(0);
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
            private Byte[] m_out_;

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
                try
                {
                    while ((i = m_stream_.Read(m_in_, 0, m_in_.Length)) != 0 && m_client_.Connected)
                    {
                        Command command = hostAPI.get_instance().parse_command(Encoding.ASCII.GetString(m_in_, 0, i));
                        Event evt = new Event();
                        if (command != default(Command))
                        {
                            evt = command.Executer.execute(command, new Requester(m_user_id_, m_session_id_));
                            EventManager.get_instance().raise(evt);
                        }
                        else
                        {
                            evt.setTitle(event_title.NEW_COMMAND_EVENT).setStatus(status.FAIL).setDatetime(DateTime.Now).setResponse("Received bad command");
                        }
                        evt.setUID(m_user_id_).setSID(m_session_id_);
                        m_out_ = Encoding.ASCII.GetBytes(hostAPI.get_instance().parse_resonse(evt));
                        m_stream_.Write(m_out_, 0, m_out_.Length);
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    m_session_id_ = (ID)SID.UNAVAILABLE_SID;
                }
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
            }

            private Queue<Event> m_event_queue_ = new Queue<Event>();
            //-----------Handled Class fields--------------//
            private Dictionary<ID, Session> m_sessions_;
            public void setHandlerField(Dictionary<ID,Session> sessions)
            {
                m_sessions_ = sessions;
            }
            //---------------------------------------------//
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
                    case (int)event_title.NEW_USER_EVENT:
                        if (evt.Status == status.SUCCESS)
                        {
                            m_sessions_[evt.SID].UserID = evt.UID;
                        }
                        break;
                    case (int)event_title.LOGIN_EVENT:
                        if (evt.Status == status.SUCCESS)
                        {
                            m_sessions_[evt.SID].UserID = evt.UID;
                        }
                        break;
                    case (int)event_title.LOGOUT_EVENT:
                        if (evt.Status == status.SUCCESS)
                        {
                            m_sessions_[evt.SID].UserID = default(ID);
                        }
                        break;
                    case (int)event_title.DELETE_USER_EVENT:
                        if (evt.Status == status.SUCCESS)
                        {
                            m_sessions_[evt.SID].UserID = default(ID);
                        }
                        break;
                    default:
                        break;
                }
            }
            public void queue(Event evt)
            {
                m_event_queue_.Enqueue(evt);
                handle();
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

        public Command parse_command(string user_input)
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
                case "stop":
                    if (args.Length == 1)
                    {
                        command = new Command(args, AdminSpace.get_instance(), AdminSpace.get_instance().stop);
                    }
                    break;
                default:
                    break;
            }
            return command;
        }
        public string parse_resonse(Event evt)
        {
            if (evt.UID != (ID)default(UID) && evt.UID != (ID) UID.ADMIN_UID) {
                User user = (User)DataBase.get_instance().read_from_table("Users",evt.UID);
                List<ID> by_me = user.SurveysByMe;
                string by_me_str = "Surveys create by me: ";
                List<ID> to_me = user.SurveysToMe;
                string to_me_str = "Surveys to me: ";
                if (by_me.Count == 0)
                {
                    by_me_str += "---";
                }
                else
                {
                    foreach (ID survey_id in by_me)
                    {
                        by_me_str += survey_id.ToString() + " ";
                    }
                }
                if (to_me.Count == 0)
                {
                    to_me_str += "---";
                }
                else
                {
                    foreach (ID survey_id in to_me)
                    {
                        to_me_str += survey_id.ToString() + " ";
                    }
                }
                evt.setResponse(evt.ExecuterResponse + "\n" + by_me_str + "\n" + to_me_str);
            }
            string output = JsonConvert.SerializeObject(evt);
            return output;
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

    public class SurveyCreator
    {
        private static SurveyCreator m_instance_ = new SurveyCreator();
        public static SurveyCreator get_instance() { return m_instance_; }

        private SurveyCreator()
        {
            try
            {
                id = Int32.Parse(File.ReadAllText(ID_DIR));
            }
            catch (FormatException)
            {
                throw new Exception("Loaded id is not a number");
            }
        }
        private string DIR = "../../CreateSurveyFiles/";
        private string ID_DIR = "../../Backend/Components/load_id.txt";
        private ID id = 1;
        public Survey create(string file_name)
        {

            Survey survey = default(Survey);
            string[] parts = file_name.Split('.');
            string suffix = parts[parts.Length - 1];
            IParser parser = default(IParser);
            switch (suffix)
            {
                case "xml":
                    parser = new XMLParser();
                    break;
                default:
                    break;
            }
            if (parser != default(IParser))
            {
                survey = parser.parse(id,DIR+file_name);
                if(survey != default(Survey))
                {
                    id++;
                }
            }
            return survey;
        }
        ~SurveyCreator()
        {
            if (File.Exists(ID_DIR))
            {
                File.Delete(ID_DIR);
            }
            File.WriteAllText(ID_DIR, id.ToString());
        }
    }
}
