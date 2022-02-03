using sturvey_app.Surveys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ID = System.Int32;
namespace sturvey_app.Components
{
    class Session
    {


    }

    class UserManager
    {
        private static UserManager m_instance_ = new UserManager();
        public static UserManager get_instance() { return m_instance_; }

        private UserManager() { }

        public void sign_up() { }
        public void login() { }
        public void logout() { }
        public void delete_user() { }
    }

    public class SurveyManager
    {
        private static SurveyManager m_instance_ = new SurveyManager();
        public static SurveyManager get_instance() { return m_instance_; }

        private SurveyManager() { }
    }
}
