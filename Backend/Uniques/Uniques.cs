﻿using Newtonsoft.Json;
using sturvey_app.Data;
using sturvey_app.Security;
using System.Collections.Generic;
using ID = System.Int32;

namespace sturvey_app.Users
{
    public class User : IUnique
    {
        private ID m_id_;
        private string m_password_;
        private List<int> m_surveys_by_me_ = new List<int>();
        private List<int> m_surveys_to_me_ = new List<int>();

        public User() { } // Empty Constructor for loader
        public IUnique loader(DataBlock data)
        {
            User_Data user = (User_Data)data;
            m_id_ = user.id;
            m_password_ = user.password;
            m_surveys_by_me_ = user.surveys_by_me;
            m_surveys_to_me_ = user.surveys_to_me;
            return this;
        }//loading
        public User(ID id,string password)
        {
            m_id_ = id;
            m_password_ = Hash.ComputeSha256Hash(password);
        } // Constructor

        //----------IUnique Interface-------------//
        public ID id()
        {
            return m_id_;
        }
        //----------------------------------------//

        //------------User API--------------//
        public void create_survey() { }
        public void vote_survey() { }
        public void view_survey() { }
        public void delete_survey() { }
        public void clear_survey() { }
        //----------------------------------//
        public List<int> Surveys_by_me
        {
            get{
                return m_surveys_by_me_;
            }
        }
        public List<int> Surveys_to_me
        {
            get
            {
              return m_surveys_to_me_;
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
        public User_Data(User user)
        {
            id = user.id();
            password = user.Password;
            surveys_by_me = user.Surveys_by_me;
            surveys_to_me = user.Surveys_to_me;
        } // Saving

        [JsonConstructor]
        public User_Data() { }
    }
}

namespace sturvey_app.Surveys
{
    public class Survey : IUnique
    {
        private ID m_id_;
        private List<KeyValuePair<string,List<string>>> m_survey_ = new List<KeyValuePair<string, List<string>>>();
        private List<KeyValuePair<string, List<int>>> m_votes_ = new List<KeyValuePair<string, List<int>>>();
        public Survey() { } // Empty Constructor for loader
        public IUnique loader(DataBlock data)
        {
            Survey_Data survey = (Survey_Data)data;
            m_id_ = survey.id;
            m_survey_ = survey.survey;
            m_votes_ = survey.votes;
            return this;
        }
        public Survey(ID id)
        {
            m_id_ = id;
        } // Constructor

        //----------IUnique Interface-------------//
        public ID id()
        {
            return m_id_;
        }
        //----------------------------------------//

        public List<KeyValuePair<string, List<string>>> survey
        {
            get {
                return m_survey_;
            }
        }
        public List<KeyValuePair<string, List<int>>> votes
        {
            get
            {
                return m_votes_;
            }
        }
    }
    public class Survey_Data : DataBlock
    {
        public ID id;
        public List<KeyValuePair<string, List<string>>> survey = new List<KeyValuePair<string, List<string>>>();
        public List<KeyValuePair<string, List<int>>> votes = new List<KeyValuePair<string, List<int>>>();
        public Survey_Data(Survey survey_)
        {
            id = survey_.id();
            survey = survey_.survey;
            votes = survey_.votes;
        } // Saving

        [JsonConstructor]
        public Survey_Data() { }
    }
}