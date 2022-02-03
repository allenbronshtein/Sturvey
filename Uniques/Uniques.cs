using Newtonsoft.Json;
using sturvey_app.Data;
using ID = System.Int32;

namespace sturvey_app.Users
{
    public class User : IUnique
    {
        private ID m_id_;

        public User() { } // Empty Constructor for loader
        public IUnique loader(DataBlock data)
        {
            User_Data user = (User_Data)data;
            m_id_ = user.id;
            return this;
        }
        public User(ID id)
        {
            m_id_ = id;
        } // Constructor
        public User(User_Data data)
        {
            m_id_ = data.id;

        } // Copy Constructor

        //----------IUnique Interface-------------//
        public ID id()
        {
            return m_id_;
        }
        public IUnique clone()
        {
            User_Data data = new User_Data(this);
            return new User(data);
        }
        //----------------------------------------//
    }
    public class User_Data : DataBlock
    {
        public ID id;
        public User_Data(User user)
        {
            id = user.id();
        } // Constructor builds struct data

        [JsonConstructor]
        public User_Data() { }
    }
}

namespace sturvey_app.Surveys
{
    public class Survey : IUnique
    {
        private ID m_id_;

        public Survey() { } // Empty Constructor for loader
        public IUnique loader(DataBlock data)
        {
            Survey_Data Survey = (Survey_Data)data;
            m_id_ = Survey.id;
            return this;
        }
        public Survey(ID id)
        {
            m_id_ = id;
        } // Constructor
        public Survey(Survey_Data data)
        {
            m_id_ = data.id;

        } // Copy Constructor

        //----------IUnique Interface-------------//
        public ID id()
        {
            return m_id_;
        }
        public IUnique clone()
        {
            Survey_Data data = new Survey_Data(this);
            return new Survey(data);
        }
        //----------------------------------------//
    }
    public class Survey_Data : DataBlock
    {
        public ID id;
        public Survey_Data(Survey Survey)
        {
            id = Survey.id();
        } // Constructor builds struct data

        [JsonConstructor]
        public Survey_Data() { }
    }
}
