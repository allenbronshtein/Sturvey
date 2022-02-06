using sturvey_app.Comands;
using sturvey_app.Data;
using sturvey_app.Surveys;
using sturvey_app.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ID = System.Int32;
using Request = System.Func<string[], sturvey_app.Comands.status>;
using Requester = System.Tuple<int, int>;

namespace sturvey_app.Components
{
    public interface IExecuter
    {
        status execute(Command command, Requester tuple);
    }
    public interface IEventHandler {
        void sign();
        void queue(Event evt);
        void raise(Event evt);
        void handle();
    }

    public class UserManager : IExecuter
    {
        private static UserManager m_instance_ = new UserManager();
        public static UserManager get_instance() { return m_instance_; }
        private EventHandler m_event_handler_;
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
                Thread.Sleep(500);
                while (m_event_queue_.Count != 0)
                {
                    handle();
                }
            }
            public void handle()
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
        private UserManager()
        {
            m_event_handler_ = EventHandler.get_instance();
        }
        public status sign_up(string[] args) { return status.SUCCESS; }
        public status login(string[] args) { return status.SUCCESS; }
        public status logout(string[] args) { return status.SUCCESS; }
        public status delete_user(string[] args) { return status.SUCCESS; }

        public status execute(Command command,Requester requester)
        {
            return command.Request(command.Args);
        }
    }

    public class SurveyManager : IExecuter
    {
        private static SurveyManager m_instance_ = new SurveyManager();
        public static SurveyManager get_instance() { return m_instance_; }
        private EventHandler m_event_handler_;
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
                Thread.Sleep(500);
                while (m_event_queue_.Count != 0)
                {
                    handle();
                }
            }
            public void handle()
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
        private SurveyManager() {
            m_event_handler_ = EventHandler.get_instance();
        }

        public status create_survey(string[] args) { return status.SUCCESS; }
        public status vote_survey(string[] args) { return status.SUCCESS; }
        public status view_survey(string[] args) { return status.SUCCESS; }
        public status delete_survey(string[] args) { return status.SUCCESS; }
        public status clear_survey(string[] args) { return status.SUCCESS; }

        public status execute(Command command,Requester requester)
        {
            return command.Request(command.Args);
        }
    }

    public class EventManager
    {
        private static EventManager m_instance_ = new EventManager();
        public static EventManager get_instance() { return m_instance_; }

        private EventManager() { }

        List<IEventHandler> handlers = new List<IEventHandler>();

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
            private User m_user_;
            private ID m_session_id_;
            private Byte[] m_in_;
            public Session(TcpClient client, int session_id)
            {
                m_session_id_ = session_id;
                m_client_ = client;
                m_stream_ = client.GetStream();
                m_in_ = new byte[1024];
                m_user_ = default(User);
            }

            public void run_task()
            {
                int i;
                while ((i = m_stream_.Read(m_in_, 0, m_in_.Length)) != 0)
                {
                    Command command = hostAPI.get_instance().parse(Encoding.ASCII.GetString(m_in_, 0, i));
                    if (command != default(Command))
                    {
                        command.Executer.execute(command, new Requester(m_user_.id(), m_session_id_));
                    }
                }
                m_session_id_ = -1;
            }

            public int id()
            {
                return m_session_id_;
            }
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
                Thread.Sleep(500);
                while (m_event_queue_.Count != 0)
                {
                    handle();
                }
            }
            public void handle()
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
        private static SessionManager m_instance_ = new SessionManager();
        public static SessionManager get_instance() { return m_instance_; }
        private Dictionary<ID, Session> sessions = new Dictionary<ID, Session>();
        private Mutex m_createM_, m_deleteM_;
        private Queue<ID> queue_idx = new Queue<ID>();
        private int count_sidx = 0;
        private Thread session_cleaner;
        private EventHandler m_event_handler_;
        private SessionManager() {
            m_event_handler_ = EventHandler.get_instance();
            m_createM_ = new Mutex();
            m_deleteM_ = new Mutex();
            session_cleaner = new Thread(sessions_cleaner_task);
            session_cleaner.Start();
        }

        private void sessions_cleaner_task() {
            while (true)
            {
                List<ID> remove_list = new List<ID>();
                foreach(KeyValuePair<ID,Session> pair in sessions)
                {
                    ID session_id = pair.Key;
                    Session session = pair.Value;
                    if(session.id() == -1 || session.id() != session_id)
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
        public void create_session(TcpClient client)
        {
            m_createM_.WaitOne();
            ID session_id = gen_session_id();
            if (session_id != -1)
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
        private ID gen_session_id()
        {
            ID session_id = -1;
            if (sessions.Count < MAX_SESSIONS)
            {
                if (queue_idx.Count > 0)
                {
                    session_id = queue_idx.Dequeue();
                    if (sessions.ContainsKey(session_id))
                    {
                        session_id = -1;
                    }
                }
                else
                {
                    session_id = count_sidx++;
                }
            }
            return session_id;
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
            Thread.Sleep(500);
            while (m_event_queue_.Count != 0)
            {
                handle();
            }
        }
        public void handle()
        {
           Event evt = m_event_queue_.Dequeue();
           Console.WriteLine("Logger: {0}", evt.Message);
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
