using sturvey_app.Data;
using sturvey_app.Uniques;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ID = System.Int32;
namespace sturvey_app.Backend.Parsers
{
    public interface IParser
    {
        Survey parse(ID id, string file);
    }
    public class XMLParser : IParser
    {
        public Survey parse(ID id, string file)
        {
            Survey survey = default(Survey);
            if (!File.Exists(file) || DataBase.get_instance().read_from_table("Surveys", id) == default(IUnique))
            {
                return survey;
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            ID admin;
            List<ID> users = new List<ID>();
            List<KeyValuePair<string, List<string>>> _survey = new List<KeyValuePair<string, List<string>>>() ;
            try
            {
                admin = Int32.Parse(doc.GetElementsByTagName("Admin")[0].InnerText);
                IUnique u  = DataBase.get_instance().read_from_table("Users", admin);
                if(u == default(IUnique))
                {
                    return survey;
                }
                XmlNodeList xml_users = doc.GetElementsByTagName("User");
                foreach(XmlNode user_node in xml_users)
                {
                    ID _id = Int32.Parse(user_node.InnerText);
                    u = DataBase.get_instance().read_from_table("Users", _id);
                    if (u == default(IUnique))
                    {
                        continue;
                    }
                    users.Add(Int32.Parse(user_node.InnerText));
                }
            }
            catch (FormatException)
            {
                return survey;
            }
            XmlNodeList questioniers = doc.GetElementsByTagName("Questionier");
            foreach(XmlNode xml_questionier in questioniers)
            {
                string question = xml_questionier.SelectNodes("Question")[0].InnerText;
                List<string> options = new List<string>();
                XmlNodeList xml_options = xml_questionier.SelectNodes("Option");
                foreach(XmlNode option in xml_options)
                {
                    options.Add(option.InnerText);
                }
                KeyValuePair<string, List<string>> questionier = new KeyValuePair<string, List<string>>(question,options);
                _survey.Add(questionier);
            }
            survey = new Survey(id, _survey, admin, users);
            return survey;
        }
    }
}
