using System;
using System.Collections;
using ID = System.Int32;
namespace sturvey_app.Data
{
    public interface ITable
    {
        void add(IUnique val);
        IUnique read(ID key); 
        void update(IUnique val);
        void delete(ID key);
        void load_from_disk(string serialized_data);
        void save_to_disk(string dir, string file_name);
    }
}
