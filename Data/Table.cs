using System;
using System.Collections.Generic;

namespace sturvey_app.Data
{
    public class Table<keyT,valT> : ITable
    {
        private Dictionary<keyT,valT> m_data_;
        private Type m_key_type_;
        private Type m_val_type_;
        public Table()
        {
            m_key_type_ = typeof(keyT);
            m_val_type_ = typeof(valT);
            m_data_ = new Dictionary<keyT, valT>();
        }

        //-------------User API--------------//
        public void create(keyT key, valT val) {
            if (!m_data_.ContainsKey(key))
            {
                m_data_.Add(key, val);
            }
        }
        public valT read(keyT key) {
            if (m_data_.ContainsKey(key))
            {
                return m_data_[key];
            }
            return default(valT);
        }
        public void update(keyT key, valT val) {
            if (m_data_.ContainsKey(key))
            {
                m_data_[key] = val;
            }
        }
        public void delete(keyT key) {
            if (m_data_.ContainsKey(key))
            {
                m_data_.Remove(key);
            }
        }
        public string table_type()
        {
            return m_key_type_.ToString() + ',' + m_val_type_.ToString() ;
        }
        //-----------------------------------//
    }
}
