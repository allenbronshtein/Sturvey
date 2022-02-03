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
        public ID Id()
        {
            return m_id_;
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
            id = user.Id();
        }
    }
}
