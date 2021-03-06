using sturvey_app.Components;
using System;
using Request = System.Func<string[], sturvey_app.Comands.Requester, sturvey_app.Comands.Event>; //First argument args array, second argument requester, last is return value
using ID = System.Int32;
namespace sturvey_app.Comands
{
    public enum question_type
    {
        OPEN,
        UNI,
        MULTI,
        LAST,
    }
    public enum UID
    {
        ADMIN_UID = -1,
        LAST,
    }
    public enum SID {
        UNAVAILABLE_SID = -1,
        USER_INIT_SID = 1,
        LAST,
    }
    public enum status
    {
        SUCCESS,
        FAIL,
        LAST,
    } // Workflow related methods should return status if there is no other return value
    public enum event_title
    {
        //System Suites Events
        FIRST_SYSTEM_EVENT,// <----BORDER

        SYSTEM_OFF_EVENT,
        SYSTEM_ON_EVENT,
        SERVER_OFF_EVENT,
        SERVER_ON_EVENT,
        USER_MANAGER_OFF_EVENT,
        USER_MANAGER_ON_EVENT,
        SESSION_MANAGER_OFF_EVENT,
        SESSION_MANAGER_ON_EVENT,
        HOST_MANAGER_OFF_EVENT,
        HOST_MANAGER_ON_EVENT,
        USER_MANAGER_HANDLER_OFF_EVENT,
        USER_MANAGER_HANDLER_ON_EVENT,
        EVENT_MANAGER_OFF_EVENT,
        EVENT_MANAGER_ON_EVENT,
        LOGGER_MANAGER_OFF_EVENT,
        LOGGER_MANAGER_ON_EVENT,

        LAST_SYSTEM_EVENT,// <----BORDER

        //User Suites Events
        FIRST_USER_EVENT,// <----BORDER

        LOGIN_EVENT,
        NEW_USER_EVENT,
        LOGOUT_EVENT,
        DELETE_USER_EVENT,

        LAST_USER_EVENT,// <----BORDER

        //Survey Suites Events
        FIRST_SURVEY_EVENT,// <----BORDER

        NEW_SURVEY_EVENT,
        VOTE_SURVEY_EVENT,
        VIEW_SURVEY_EVENT,
        DELETE_SURVEY_EVENT,
        CLEAR_SURVEY_EVENT,

        LAST_SURVEY_EVENT,// <----BORDER

        //Session Suites Events
        FIRST_SESSION_EVENT,// <----BORDER

        NEW_SESSION_EVENT,
        DELETE_SESSION_EVENT,

        LAST_SESSION_EVENT,// <----BORDER

        //Host API Suites Events
        FIRST_HOSTAPI_EVENT,// <----BORDER

        NEW_COMMAND_EVENT,
        GET_MANUAL_EVENT,

        LAST_HOSTAPI_EVENT,// <----BORDER
        //DataBase Suites Events
        FIRST_DATABASE_EVENT,// <----BORDER


        NEW_TABLE_EVENT,
        DELETE_TABLE_EVENT,
        LOAD_DATA_EVENT,
        SAVA_DATA_EVENT,

        LAST_DATABASE_EVENT,// <----BORDER
        //Admin Suites Events
        FIRST_ADMIN_EVENT,// <----BORDER

        ADMIN_LOGIN_EVENT,

        LAST_ADMIN_EVENT,// <----BORDER

        LAST,
    } 
    public enum event_suite
    {
        SYSTEM_EVENT_SUITE,
        USER_EVENT_SUITE,
        SURVEY_EVENT_SUITE,
        SESSION_EVENT_SUITE,
        HOSTAPI_EVENT_SUITE,
        DATABASE_EVENT_SUITE,
        LAST,
    }

    public class Command
    {
        private string[] m_args_;
        private IExecuter m_executer_;
        private Request m_request_;
        public Command(string[] args, IExecuter executer,Request request)
        {
            m_args_ = args;
            m_executer_ = executer;
            m_request_ = request;
        }
        public string[] Args
        {
            get { return m_args_; }
        }
        public IExecuter Executer
        {
            get { return m_executer_; }
        }
        public Request Request
        {
            get { return m_request_; }
        }
    }

    public class Event
    {
        private event_title m_title_ = default(event_title); //event title
        private status m_status_ = default(status) ; // status (Success, Fail)
        private string m_msg_ = default(string); // Message to be printed by logger
        private DateTime m_datetime_ = default(DateTime); // Time of event
        private string m_reporter_ = default(string); // Which component reported the event
        private ID m_sid_ = default(ID); // Session ID that caused the event
        private ID m_uid_ = default(ID); // User ID that caused the event
        private string m_executer_response_ = default(string); //Executer answer, additional data
        public event_title Title
        {
            get { return m_title_; }
        }
        public status Status
        {
            get { return m_status_; }
        }
        public string Message
        {
            get { return m_msg_; }
        }
        public DateTime Datetime
        {
            get { return m_datetime_; }
        }
        public string Reporter
        {
            get { return m_reporter_; }
        }
        public ID SID
        {
            get { return m_sid_; }
        }
        public ID UID
        {
            get { return m_uid_; }
        }
        public string ExecuterResponse
        {
            get { return m_executer_response_; }
        }

        public Event setTitle(event_title title)
        {
            m_title_ = title;
            return this;
        }
        public Event setStatus(status status)
        {
            m_status_ = status;
            return this;
        }
        public Event setMessage(string message)
        {
            m_msg_ = message;
            return this;
        }
        public Event setDatetime(DateTime dateTime)
        {
            m_datetime_ = dateTime;
            return this;
        }
        public Event setReporter(IEventHandler reporter)
        {
            m_reporter_ = reporter.GetType().ToString();
            return this;
        }
        public Event setSID(ID id)
        {
            m_sid_ = id;
            return this;
        }
        public Event setUID(ID id)
        {
            m_uid_ = id;
            return this;
        }
        public Event setResponse(string reponse)
        {
            m_executer_response_ = reponse;
            return this;
        }
    }

    public class Requester
    {
        private ID m_uid_;
        private ID m_sid_;
        public Requester(ID uid, ID sid)
        {
            m_uid_ = uid;
            m_sid_ = sid;
        }
        public ID UID
        {
            get { return m_uid_; }
        }
        public ID SID
        {
            get { return m_sid_; }
        }
    }
}
