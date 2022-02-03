using Newtonsoft.Json;
using sturvey_app.Users;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using ID = System.Int32;

namespace sturvey_app.Data
{
    public class DataBase
    {
        private class Table : ITable
        {
            private IDictionary m_data_ = new Dictionary<ID, IUnique>();
            private Type m_valT_;
            public Table(Type value_type)
            {
                m_valT_ = value_type;
            }

            public void add(IUnique value)
            {
                ID key = value.Id();
                if (!m_data_.Contains(key) && value.GetType() == m_valT_)
                {
                    m_data_[key] = value;
                }
            }
            public IUnique read(ID key)
            {
                IUnique value = default(IUnique);
                if (m_data_.Contains(key))
                {
                    value = (IUnique)m_data_[key];
                }
                return value;
            }
            public void update(IUnique value)
            {
                ID key = value.Id();
                if (m_data_.Contains(key) && value.GetType() == m_valT_)
                {
                    m_data_[key] = value;
                }
            }
            public void delete(ID key)
            {
                if (m_data_.Contains(key))
                {
                    m_data_.Remove(key);
                }
            }

            public void load_from_disk(string serialized_data)
            {
                if (m_data_.Count == 0)
                {
                    if (m_valT_ == typeof(User)) // case
                    {
                        IDictionary dup = JsonConvert.DeserializeObject<Dictionary<ID, User_Data>>(serialized_data);
                        foreach (KeyValuePair<ID, User_Data> pair in dup)
                        {
                            m_data_[pair.Key] = new User(pair.Value);
                        }
                    }// end case
                }
            }
            public void save_to_disk(string dir, string file_name)
            {
                IDictionary dup = default(IDictionary);
                if (m_valT_ == typeof(User)) // case
                {
                    dup = new Dictionary<ID, User_Data>();
                    foreach (var key in m_data_.Keys)
                    {
                        dup[key] = new User_Data((User)m_data_[key]);
                    }
                }// end case
                string location = dir + file_name + ".json";
                var serialized = JsonConvert.SerializeObject(dup);
                if (File.Exists(location))
                {
                    File.Delete(location);
                }
                string content = serialized + "\n" + m_valT_.ToString() + "\n" + file_name;
                File.WriteAllText(location, content);
            }
        }

        private const string m_dir_ = "../../Data/Saved Tables/";
        private static DataBase m_instance_ = new DataBase();
        private Dictionary<string,ITable> m_tables_ = new Dictionary<string, ITable>();
        private DataBase() {}
        public static DataBase get_instance(){return m_instance_;}
        

        //-------------User API--------------//
        public void create_table(string table_name, Type table_type) {
            if (!m_tables_.ContainsKey(table_name))
            {
                m_tables_[table_name] = new Table(table_type);
            }
        }
        public ITable get_table(string table_name)
        {
            ITable table = null;
            if (m_tables_.ContainsKey(table_name))
            {
                table = m_tables_[table_name];
            }
            return table;
        }
        public void delete_table(string table_name) {
            if (m_tables_.ContainsKey(table_name))
            {
                m_tables_.Remove(table_name);
            }
        }
        //-----------------------------------//


        //-------------Backend--------------//
        public void load_from_disk() {
            foreach (var file in Directory.EnumerateFiles(m_dir_, "*.json"))
            {
                string serialized_data = File.ReadAllText(file);
                string[] parts = serialized_data.Split('\n'); // [0] == serialized table data, [1] == type of table, [2] == table name
                ITable table = new Table(Type.GetType(parts[1]));
                table.load_from_disk(parts[0]);
                if (!m_tables_.ContainsKey(parts[2]))
                {
                    m_tables_[parts[2]] = table;
                }
            }
        }
        public void save_to_disk() {
            foreach(KeyValuePair<string,ITable> entry in m_tables_)
            {
                ITable table = entry.Value;
                table.save_to_disk(m_dir_, entry.Key);                
            }
        }
        //----------------------------------//

    }
}
