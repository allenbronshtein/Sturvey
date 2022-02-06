using sturvey_app.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Request = System.Func<string[], sturvey_app.Comands.status>;
namespace sturvey_app.Comands
{
    public enum status
    {
        SUCCESS,
        FAIL,
    }

    public class Command
    {
        private string[] m_args_;
        private 
IExecuter m_executer_;
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
        private event_title m_title_;
        private status m_status_;
        private string m_msg_;
        public Event(event_title title, status status, string msg)
        {
            m_title_ = title;
            m_status_ = status;
            m_msg_ = msg;
        }
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
    }

    public enum event_title
    {
        //System Events
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
        //User Events
        LOGIN_EVENT,
        NEW_USER_EVENT,
        LOGOUT_EVENT,
        DELETE_USER_EVENT,
        //Survey Events
        NEW_SURVEY_EVENT,
        VOTE_SURVEY_EVENT,
        VIEW_SURVEY_EVENT,
        DELETE_SURVEY_EVENT,
        // Session Events
        NEW_SESSION_EVENT,
        DELETE_SESSION_EVENT,
        // Host API Events
        NEW_COMMAND_EVENT,
        // DataBase Events
        NEW_TABLE_EVENT,
        DELETE_TABLE_EVENT,
        LOAD_DATA_EVENT,
        SAVA_DATA_EVENT,
        LAST,
    }

    public class hostAPI
    {
        private static hostAPI m_instance_ = new hostAPI();
        public static hostAPI get_instance() { return m_instance_; }

        private hostAPI() { }

        public Command parse(string user_input)
        {

            Command command = default(Command);
            user_input = user_input.Trim();
            string[] args = user_input.Split(' ');
            switch (args[0])
            {
                case "login":
                    if (args.Length == 3)
                    {
                        command = new Command(args, UserManager.get_instance(), UserManager.get_instance().login);
                    }
                    break;
                case "adduser":
                    if (args.Length == 3)
                    {
                        command = new Command(args, UserManager.get_instance(), UserManager.get_instance().sign_up);
                    }
                    break;
                case "logout":
                    if (args.Length == 1)
                    {
                        command = new Command(args, UserManager.get_instance(), UserManager.get_instance().logout);
                    }
                    break;
                case "rmuser":
                    if (args.Length == 2)
                    {
                        command = new Command(args, UserManager.get_instance(), UserManager.get_instance().delete_user);
                    }
                    break;
                case "addsurvey":
                    if (args.Length == 3)
                    {
                        command = new Command(args, SurveyManager.get_instance(), SurveyManager.get_instance().create_survey);
                    }
                    break;
                case "rmsurvey":
                    if (args.Length == 3)
                    {
                        command = new Command(args, SurveyManager.get_instance(), SurveyManager.get_instance().delete_survey);
                    }
                    break;
                case "vote":
                    if (args.Length == 3)
                    {
                        command = new Command(args, SurveyManager.get_instance(), SurveyManager.get_instance().vote_survey);
                    }
                    break;
                case "view":
                    if (args.Length == 3)
                    {
                        command = new Command(args, SurveyManager.get_instance(), SurveyManager.get_instance().view_survey);
                    }
                    break;
                case "clear":
                    if (args.Length == 3)
                    {
                        command = new Command(args, SurveyManager.get_instance(), SurveyManager.get_instance().clear_survey);
                    }
                    break;
                default:
                    break;
            }
            return command;
        }
    }
}
