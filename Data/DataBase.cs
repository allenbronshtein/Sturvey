using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace sturvey_app.Data
{
    public class DataBase
    {
        private static DataBase m_instance_ = new DataBase();
        private Dictionary<string,ITable> m_tables_;
        private DataBase() {
            m_tables_ = new Dictionary<string, ITable>();
        }
        public static DataBase get_instance(){return m_instance_;}
        

        //-------------User API--------------//
        public void create_table<keyT,valT>(string table_name) {
            if (!m_tables_.ContainsKey(table_name))
            {
                m_tables_.Add(table_name, new Table<keyT, valT>());
            }
        }

        public void delete_table(string table_name) {
            if (m_tables_.ContainsKey(table_name))
            {
                m_tables_.Remove(table_name);
            }
        }
        //-----------------------------------//


        //-------------Backend--------------//
        private void load_from_disk() { }
        public void save_to_disk() {}
        //----------------------------------//

    }
}
