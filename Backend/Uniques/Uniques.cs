using Newtonsoft.Json;
using sturvey_app.Data;
using sturvey_app.Security;
using System.Collections.Generic;
using ID = System.Int32;

namespace sturvey_app.Uniques
{
    public class User : IUnique
    {
        private ID m_id_;
        private string m_password_;
        private List<int> m_surveys_by_me_ = new List<int>();
        private List<int> m_surveys_to_me_ = new List<int>();

        public User() { } // Empty Constructor for loader
        public User(ID id, string password)
        {
            m_id_ = id;
            m_password_ = Hash.ComputeSha256Hash(password);
        } // Constructor
        public IUnique loader(DataBlock data)
        {
            User_Data user = (User_Data)data;
            m_id_ = user.id;
            m_password_ = user.password;
            m_surveys_by_me_ = user.surveys_by_me;
            m_surveys_to_me_ = user.surveys_to_me;
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
        public List<int> SurveysByMe
        {
            get
            {
                return m_surveys_by_me_;
            }
            set
            {
                m_surveys_by_me_ = value;
            }
        }
        public List<int> SurveysToMe
        {
            get
            {
                return m_surveys_to_me_;
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
    public class User_Data : DataBlock
    {
        public ID id;
        public string password;
        public List<int> surveys_by_me = new List<int>();
        public List<int> surveys_to_me = new List<int>();

        [JsonConstructor]
        public User_Data() { }
    }
    public class Survey : IUnique
    {
        private ID m_id_;
        private List<KeyValuePair<string, List<string>>> m_survey_ = new List<KeyValuePair<string, List<string>>>(); //pair[0] is question, pair[1] is list of answers
        private List<List<int>> m_votes_ = new List<List<int>>(); //index is question number, the value is list of votes for that question (index 0 number of votes for answer 0 ...)
        private ID m_admin_;
        private List<ID> m_users_ = new List<ID>();
        public Survey(ID id, List<KeyValuePair<string, List<string>>> survey, ID admin, List<ID> users)
        {
            m_id_ = id;
            m_survey_ = survey;
            foreach (KeyValuePair<string, List<string>> pair in survey)
            {
                int num_of_answers = pair.Value.Count;
                List<int> votes = new List<int>();
                for (int i = 0; i < num_of_answers; i++)
                {
                    votes.Add(0);
                }
                m_votes_.Add(votes);
            }
            m_admin_ = admin;
            m_users_ = users;
        } // Constructor

        public Survey() { } // Empty Constructor for loader
        public IUnique loader(DataBlock data)
        {
            Survey_Data survey = (Survey_Data)data;
            m_id_ = survey.id;
            m_survey_ = survey.survey;
            m_votes_ = survey.votes;
            m_admin_ = survey.admin;
            m_users_ = survey.users;
            return this;
        }// loading

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
        public List<KeyValuePair<string, List<string>>> _Survey
        {
            get
            {
                return m_survey_;
            }
        }
        public List<List<int>> Votes
        {
            get
            {
                return m_votes_;
            }
        }
        public List<ID> Users
        {
            get { return m_users_; }
            set { m_users_ = value; }
        }
    }
    public class Survey_Data : DataBlock
    {
        public ID id;
        public List<KeyValuePair<string, List<string>>> survey = new List<KeyValuePair<string, List<string>>>();
        public List<List<int>> votes = new List<List<int>>();
        public ID admin;
        public List<ID> users;

        [JsonConstructor]
        public Survey_Data() { }
    }
}