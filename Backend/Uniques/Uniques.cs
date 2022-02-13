using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using sturvey_app.Data;
using sturvey_app.Security;
using System.Collections.Generic;
using ID = System.Int32;
public interface ISurvey : IUnique
{
    List<ID> get_users();
    ID get_admin();
    int get_number_of_questions();
    void set_votes();
    List<Question> get_survey();
}
public class Question
{
    private string m_question_;
    private List<string> m_options_ = new List<string>();
    private int m_options_count_ = 0;
    private bool m_is_multi_optional_ = false;
    public Question(string question, List<string> options)
    {
        m_question_ = question;
        m_options_ = options;
        m_options_count_ = options.Count;
        if(m_options_count_ > 0)
        {
            m_is_multi_optional_ = true;
        }
    }
    public string _Question
    {
        get { return m_question_; }
    }
    public List<string> Options
    {
        get
        {
            List<string> clone = new List<string>();
            foreach(string option in m_options_)
            {
                clone.Add(option);
            }
            return clone;
        }
    }
    public int OptionCount
    {
        get { return m_options_count_; }
    }
    public bool isMultiOptional
    {
        get { return m_is_multi_optional_; }
    }
}
namespace sturvey_app.Uniques
{
    public class User : IUnique
    {
        private ID m_id_;
        private string m_password_;
        private List<ID> m_surveys_by_me_ = new List<ID>();
        private List<ID> m_surveys_to_me_ = new List<ID>();

        public User(ID id, string password)
        {
            m_id_ = id;
            m_password_ = Hash.ComputeSha256Hash(password);
        } // Constructor
        public User() { }
        public IUnique loader(DBlock data)
        {
            m_id_ = (ID)data.props[0].Value;
            m_password_ = (string)data.props[3].Value;
            foreach(JToken item in data.props[1].Value)
            {
                m_surveys_by_me_.Add((ID)item);
            }
            foreach (JToken item in data.props[2].Value)
            {
                m_surveys_to_me_.Add((ID)item);
            }
            return this;
        }//loading

        //----------IUnique Interface-------------//
        public ID id()
        {
            return m_id_;
        }
        //----------------------------------------//
        public ID Id
        {
            get { return m_id_; }
        }
        public List<ID> SurveysByMe
        {
            get
            {
                List<ID> clone = new List<ID>();
                foreach(ID id in m_surveys_by_me_)
                {
                    clone.Add(id);
                }
                return clone;
            }
            set
            {
                m_surveys_by_me_ = value;
            }
        }
        public List<ID> SurveysToMe
        {
            get
            {
                List<ID> clone = new List<ID>();
                foreach (ID id in m_surveys_to_me_)
                {
                    clone.Add(id);
                }
                return clone;
            }
            set
            {
                m_surveys_to_me_ = value;
            }
        }
        public string Password
        {
            get { return m_password_; }
        }
    }
    public class Survey : ISurvey
    {
        private ID m_id_;
        private List<Question> m_survey_ = new List<Question>();
        private List<List<string>> m_votes_ = new List<List<string>>();
        private ID m_admin_;
        private List<ID> m_users_ = new List<ID>();
        public Survey(ID id, List<Question> survey, ID admin, List<ID> users)
        {
            m_id_ = id;
            m_survey_ = survey;
            m_admin_ = admin;
            m_users_ = users;
        } // Constructor
        public Survey() { }
        public IUnique loader(DBlock data)
        {
            m_id_ = (ID)data.props[0].Value;
            foreach(JToken token in data.props[1].Value)
            {
                string _question = (string)token.First;
                List<string> options = new List<string>();
                foreach(JToken ltoken in token.First.Next.First)
                {
                    options.Add((string)ltoken);
                }
                Question question = new Question(_question, options);
                m_survey_.Add(question);
            }
            m_admin_ = (ID)data.props[4].Value;
            foreach (JToken token in data.props[3].Value)
            {
                m_users_.Add((ID)token);
            }
            foreach(JToken list in data.props[2].Value)
            {
                List<string> votes = new List<string>();
                foreach(JToken item in list)
                {
                    votes.Add(item.ToString());
                }
                m_votes_.Add(votes);
            }
            return this;
        }// loading

        //----------IUnique/ISurvey Interface-------------//
        public ID id()
        {
            return Id;
        }
        public List<ID> get_users()
        {
            return Users;
        }
        public ID get_admin()
        {
            return Admin;
        }
        public void set_votes()
        {
            m_votes_ = Votes;
        }

        public int get_number_of_questions()
        {
            return m_survey_.Count;
        }

        public List<Question> get_survey()
        {
            List<Question> questions = new List<Question>();
            foreach(Question question in m_survey_)
            {
                Question question_clone = new Question(question._Question, question.Options);
            }
            return questions;
        }

        //----------------------------------------//
        public ID Id
        {
            get { return m_id_; }
        }
        public List<Question> _Survey
        {
            get
            {
                return m_survey_;
            }
        }
        public List<List<string>> Votes
        {
            get
            {
                return m_votes_;
            }
        }
        public List<ID> Users
        {
            get {
                List<ID> clone = new List<ID>();
                foreach(ID id in m_users_)
                {
                    clone.Add(id);
                }
                return clone;
            }
            set { m_users_ = value; }
        }
        public ID Admin
        {
            get { return m_admin_; }
        }
    }
}

public class DBlock
{
    public List<JProperty> props = new List<JProperty>();
}