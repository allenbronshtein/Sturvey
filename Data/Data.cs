using Newtonsoft.Json;
using sturvey_app.Users;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ID = System.Int32;

namespace sturvey_app.Data
{
    public interface IUnique
    {
        ID id(); // Return instance ID 
        IUnique clone(); // Returns copy of instance   
        IUnique loader(DataBlock dataBlock);
    }
    public interface DataBlock { } //Deserializable

    public class DataBase 
    {
        private static DataBase m_instance_ = new DataBase();
        public static DataBase get_instance() { return m_instance_; }

        //---------------Inner Class Table--------------//
        private class Table
        {
            private IDictionary m_data_ = new Dictionary<ID, IUnique>();
            private string m_valT_;
            public Table(string value_type)
            {
                m_valT_ = value_type;
            }

            public void add(IUnique value)
            {
                ID key = value.id();
                if (!m_data_.Contains(key) && value.GetType().FullName == m_valT_)
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
                ID key = value.id();
                if (m_data_.Contains(key) && value.GetType().FullName == m_valT_)
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
                void load<UT,UdataT>(Type UniqueT, Type Unique_dataT)
                {
                    Dictionary<ID, UdataT> dup = JsonConvert.DeserializeObject<Dictionary<ID, UdataT>>(serialized_data);
                    foreach (ID key in dup.Keys)
                    {
                        m_data_[key] = (IUnique)UniqueT.GetMethod("loader").Invoke((UT)Activator.CreateInstance(UniqueT), new object[] { dup[key] });
                    }
                }
                void _switch()
                {
                    if (m_valT_ == typeof(User).FullName)
                    {
                        load<User, User_Data>(typeof(User), typeof(User_Data));
                    }
                }

                if (m_data_.Count == 0)
                {
                    _switch();
                }
            }
            public void save_to_disk(string dir, string file_name)
            {
                IDictionary dup = default(IDictionary);
                if (m_valT_ == typeof(User).FullName) // case
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
        //----------------------------------------------//

        private const string m_dir_ = "../../Data/Saved Tables/";
        private readonly Dictionary<string, Table> m_tables_ = new Dictionary<string, Table>();

        private DataBase()
        {
            load_from_disk();
        }

        //---------------API--------------//
        public void create_table(string table_name, Type table_type) {
            if (!m_tables_.ContainsKey(table_name))
            {
                m_tables_[table_name] = new Table(table_type.FullName);
            }
        }
        public void delete_table(string table_name) {
            if (m_tables_.ContainsKey(table_name))
            {
                m_tables_.Remove(table_name);
            }
        }
        public void add_to_table(string table_name, IUnique value)
        {
            Table table = get_table(table_name);
            if(table != default(Table))
            {
                table.add(value);
            }
        }
        public IUnique read_from_table(string table_name, ID key)
        {
            IUnique value = default(IUnique);
            Table table = get_table(table_name);
            if (table != default(Table))
            {
                value = table.read(key).clone();                
            }
            return value;
        }
        public void update_table_value(string table_name, IUnique value)
        {
            Table table = get_table(table_name);
            if (table != default(Table))
            {
                table.update(value);
            }
        }
        public void delete_from_table(string table_name, ID key)
        {
            Table table = get_table(table_name);
            if (table != default(Table))
            {
                table.delete(key);
            }
        }
        //--------------------------------//

        private void load_from_disk() {
            foreach (var file in Directory.EnumerateFiles(m_dir_, "*.json"))
            {
                string serialized_data = File.ReadAllText(file);
                string[] parts = serialized_data.Split('\n'); // [0] == serialized table data, [1] == type of table, [2] == table name
                Table table = new Table(Type.GetType(parts[1]).FullName);
                table.load_from_disk(parts[0]);
                if (!m_tables_.ContainsKey(parts[2]))
                {
                    m_tables_[parts[2]] = table;
                }
            }
        }
        private void save_to_disk() {
            foreach(KeyValuePair<string,Table> entry in m_tables_)
            {
                Table table = entry.Value;
                table.save_to_disk(m_dir_, entry.Key);                
            }
        }
        private Table get_table(string table_name)
        {
            Table table = null;
            if (m_tables_.ContainsKey(table_name))
            {
                table = m_tables_[table_name];
            }
            return table;
        }

        ~DataBase()
        {
            save_to_disk();
        }
    } // Singleton
}
