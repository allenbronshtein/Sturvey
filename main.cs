using sturvey_app.Components;
using sturvey_app.Data;
using sturvey_app.Security;
using sturvey_app.Users;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace main
{
    class main
    {
        static void boot()
        {
            DataBase dataBase = DataBase.get_instance();
            SurveyManager survey_manager = SurveyManager.get_instance();
            UserManager user_manager = UserManager.get_instance();
        }
        static void Main(string[] args)
        {
            boot();
        }
    }
}
