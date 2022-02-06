using Newtonsoft.Json;
using sturvey_app.Surveys;
using sturvey_app.Users;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ID = System.Int32;
using sturvey_app.Security;
using System.Threading;
using sturvey_app.Comands;
using sturvey_app.Components;

namespace sturvey_app.Data
{
    //----------Interfaces for storable objects--------------//
    public interface IUnique
    {
        ID id(); // Return instance ID 
        IUnique loader(DataBlock dataBlock);
    } //Unserializable-Unserializable objects. (Private members)
    public interface DataBlock { } // Serialable-Deserializable objects. (Public members)
    //-------------------------------------------------------//

    public class DataBase 
    {
        private static DataBase m_instance_ = new DataBase();
        public static DataBase get_instance() { return m_instance_; }
        private DataBase()
        {
            m_tables_ = new Dictionary<string, Table>();
            m_createM_ = new Mutex();
            m_deleteM_ = new Mutex();
            m_event_handler_ = EventHandler.get_instance();
            load_from_disk();
        }

        private class Table
        {
            private IDictionary m_data_ = new Dictionary<ID, IUnique>();
            private string m_valT_;
            private Mutex m_addM_, m_updateM_, m_deleteM_;
            public Table(string value_type)
            {
                m_valT_ = value_type;
                m_addM_ = new Mutex();
                m_updateM_ = new Mutex();
                m_deleteM_ = new Mutex();
            }

            public status add(IUnique value)
            {
                m_addM_.WaitOne();
                status status = status.FAIL;
                ID key = value.id();
                if (!m_data_.Contains(key) && value.GetType().FullName == m_valT_)
                {
                    m_data_[key] = value;
                    status = status.SUCCESS;
                }
                m_addM_.ReleaseMutex();
                return status;            
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
            public status update(IUnique value)
            {
                m_updateM_.WaitOne();
                status status = status.FAIL;
                ID key = value.id();
                if (m_data_.Contains(key) && value.GetType().FullName == m_valT_)
                {
                    m_data_.Remove(key);
                    m_data_[key] = value;
                    status = status.SUCCESS;
                }
                m_updateM_.ReleaseMutex();
                return status;
            }
            public status delete(ID key)
            {
                m_deleteM_.WaitOne();
                status status = status.FAIL;
                if (m_data_.Contains(key))
                {
                    m_data_.Remove(key);
                    status = status.SUCCESS;
                }
                m_deleteM_.ReleaseMutex();
                return status;
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
                    else if (m_valT_ == typeof(Survey).FullName)
                    {
                        load<Survey, Survey_Data>(typeof(Survey), typeof(Survey_Data));
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

                void _switch()
                {
                    if (m_valT_ == typeof(User).FullName) // case
                    {
                        dup = new Dictionary<ID, User_Data>();
                        foreach (var key in m_data_.Keys)
                        {
                            dup[key] = new User_Data((User)m_data_[key]);
                        }
                    }// end case
                    if (m_valT_ == typeof(Survey).FullName) // case
                    {
                        dup = new Dictionary<ID, Survey_Data>();
                        foreach (var key in m_data_.Keys)
                        {
                            dup[key] = new Survey_Data((Survey)m_data_[key]);
                        }
                    }// end case
                }

                _switch();
                string location = dir + file_name + ".json";
                if (File.Exists(location))
                {
                    File.Delete(location);
                }
                var serialized = JsonConvert.SerializeObject(dup);
                if (serialized != "null")
                {
                    string content = serialized + "\n" + m_valT_.ToString() + "\n" + file_name + "\n";
                    content += Hash.ComputeSha256Hash(content);
                    File.WriteAllText(location, content);
                }
            }
        }

        //-----------------Fields-------------------------//
        private const string DIR = "../../Data/Saved Tables/";
        private readonly Dictionary<string, Table> m_tables_;
        Mutex m_createM_, m_deleteM_;
        private EventHandler m_event_handler_;
        //-----------------------------------------------//

        //---------------API--------------//
        public status create_table(string table_name, Type table_type) {
            m_createM_.WaitOne();
            if (!m_tables_.ContainsKey(table_name))
            {
                m_tables_[table_name] = new Table(table_type.FullName);
            }
            m_createM_.ReleaseMutex();
            return status.SUCCESS;
        }
        public status delete_table(string table_name) {
            m_deleteM_.WaitOne();
            if (m_tables_.ContainsKey(table_name))
            {
                m_tables_.Remove(table_name);
            }
            m_deleteM_.ReleaseMutex();
            return status.SUCCESS;
        }
        public status add_to_table(string table_name, IUnique value)
        {
            status status = status.FAIL;
            Table table = get_table(table_name);
            if(table != default(Table))
            {
                status = table.add(value);
            }
            return status;
        }
        public IUnique read_from_table(string table_name, ID key)
        {
            IUnique value = default(IUnique);
            Table table = get_table(table_name);
            if (table != default(Table))
            {
                value = table.read(key);                
            }
            return value;
        }
        public status update_table_value(string table_name, IUnique value)
        {
            status status = status.SUCCESS;
            Table table = get_table(table_name);
            if (table != default(Table))
            {
                status = table.update(value);
            }
            return status;
        }
        public status delete_from_table(string table_name, ID key)
        {
            status status = status.SUCCESS;
            Table table = get_table(table_name);
            if (table != default(Table))
            {
                status = table.delete(key);
            }
            return status;
        }
        //--------------------------------//

        private void load_from_disk() {
            foreach (var file in Directory.EnumerateFiles(DIR, "*.json"))
            {
                string serialized_data = File.ReadAllText(file);
                string[] parts = serialized_data.Split('\n'); // [0] == serialized table data, [1] == type of table, [2] == table name, [3] == hash
                if (Hash.ComputeSha256Hash(parts[0] + '\n' + parts[1] + '\n' + parts[2] + '\n') == parts[3])
                {
                    Table table = new Table(Type.GetType(parts[1]).FullName);
                    table.load_from_disk(parts[0]);
                    if (!m_tables_.ContainsKey(parts[2]))
                    {
                        m_tables_[parts[2]] = table;
                    }
                }
            }
        }
        private void save_to_disk() {
            foreach(KeyValuePair<string,Table> entry in m_tables_)
            {
                Table table = entry.Value;
                table.save_to_disk(DIR, entry.Key);                
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


        private class EventHandler : IEventHandler
        {
            private static EventHandler m_instance_ = new EventHandler();
            public static EventHandler get_instance() { return m_instance_; }

            private EventHandler()
            {
                sign();
                Thread handler = new Thread(handler_task);
                handler.Start();
            }
            private Queue<Event> m_event_queue_ = new Queue<Event>();

            public void handler_task()
            {
                Thread.Sleep(500);
                while (m_event_queue_.Count != 0)
                {
                    handle();
                }
            }
            public void handle()
            {
                Event evt = m_event_queue_.Dequeue();
            }
            public void queue(Event evt)
            {
                m_event_queue_.Enqueue(evt);
            }
            public void raise(Event evt)
            {
                EventManager.get_instance().raise(evt);
            }
            public void sign()
            {
                EventManager.get_instance().sign(this);
            }
        }
    }
}
