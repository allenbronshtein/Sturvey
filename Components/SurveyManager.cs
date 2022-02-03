using sturvey_app.Surveys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ID = System.Int32;
namespace sturvey_app.Components
{
    public class SurveyManager
    {
        private static SurveyManager m_instance_ = new SurveyManager();
        private Dictionary<ID, Survey> surveys;
        public static SurveyManager get_instance() { return m_instance_; }

        private SurveyManager() { }
    }
}
