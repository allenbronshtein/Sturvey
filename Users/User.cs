using Newtonsoft.Json;
using sturvey_app.Data;
using ID = System.Int32;

namespace sturvey_app.Users
{
    public class User: IUnique
    {
        private ID m_id_;
        public User(ID id)
        {
            m_id_ = id;
        }
        public User(User_Data data)
        {
            m_id_ = data.id;

        }

        public ID id()
        {
            return m_id_;
        }
        public IUnique clone()
        {
            User_Data data = new User_Data(this);
            return new User(data);
        }
    }
    public class User_Data
    {
        public ID id;
        public User_Data(ID id)
        {
            this.id = id;
        }
        public User_Data(User user)
        {
            id = user.id();
        }


        [JsonConstructor]
        public User_Data() { }
    }
}
