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
        ISurvey parse(ID user_id,ID survey_id, string file);
    }
    public class XMLParser : IParser
    {
        public ISurvey parse(ID user_id,ID survey_id, string file)
        {
            ISurvey survey = default(ISurvey);
            if (!File.Exists(file) || DataBase.get_instance().read_from_table("Surveys", survey_id) != default(IUnique))
            {
                return survey;
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(file);
            List<ID> users = new List<ID>();
            List<Question> _survey = new List<Question>() ;
            try
            {
                IUnique u  = DataBase.get_instance().read_from_table("Users", user_id);
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
                Question questionier = new Question(question,options);
                _survey.Add(questionier);
            }
            survey = new Survey(survey_id, _survey, user_id, users);
            return survey;
        }
    }
}
