using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
namespace HandlebarsDotNet
{
    public class Program 
    {
        public static void Main(string[] args)
        {
            // string val;
            // Console.Write("Enter template file name: ");
            // val = Console.ReadLine();
            string template = File.ReadAllText(args[0]);
            int recordNumber= 0;

            using (FileStream s = File.Open(args[1], FileMode.Open))
            using (StreamReader sr = new StreamReader(s))
            using (JsonReader reader = new JsonTextReader(sr))
            {   
                int beginIndex= 0;
                int endIndex= 0;
                int beginJson= 0;
                string thisJson= "";
                while (!sr.EndOfStream)
                {var jsonInput = sr.ReadLine();
                while(!String.IsNullOrEmpty(jsonInput))
                {
                    if(beginJson == 0)
                  {   
                        if (jsonInput.Contains("{"))
                        {
                            beginIndex= jsonInput.IndexOf("{");
                            beginJson= 1;
                            jsonInput= jsonInput.Substring(beginIndex, jsonInput.Length - beginIndex);
                        }else
                        {
                            jsonInput= "";
                        }      
                  }     
                  else
                  {
                        if (jsonInput.Contains("}"))
                        {
                            endIndex= jsonInput.IndexOf("}");
                            thisJson= thisJson + jsonInput.Substring(0, endIndex + 1);
                            jsonInput= jsonInput.Substring(endIndex + 1, jsonInput.Length - endIndex - 1);
                            thisJson= getBetween(thisJson, "_recordNumber", ",", recordNumber);
                            recordNumber++;
                            handleBars(thisJson, template);
                            thisJson= "";
                            beginJson= 0;
                        }else
                        {
                            thisJson= thisJson + jsonInput;
                            jsonInput= "";
                        }     
                    }   

                }
                
                }   
            }
            
        }
        public static string getBetween(string strSource, string strStart, string strEnd, int value)
{
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart, 0);
                End = strSource.IndexOf(strEnd, Start);
                string s1= strSource.Substring(Start, End - Start);
                string sNew= strStart + "\":" + value;
                return strSource.Replace(s1, sNew);
            }
            else
            {
                return strSource;
            }

            
}
        static void handleBars(string jsonInput, string template)
        {   
            var Template= template;
            string json= jsonInput;
            
            Handlebars.RegisterHelper ("trim", (writer, context, arguments) => 
            writer.Write (arguments[0].ToString().Trim()));

            Handlebars.RegisterHelper("equals", (writer, options, context, arguments) =>
            {   if (arguments[0].Equals(arguments[1]))
                {
                    options.Template(writer, (object)context);
                }
                else
                {
                    options.Inverse(writer, (object)context);
                }
            });

            Handlebars.RegisterHelper("StartsWith", (writer, options, context, arguments) =>
            {   var v1= arguments[0].ToString();
                var v2= arguments[1].ToString();
                if (v1.StartsWith(v2))
                {
                    options.Template(writer, (object)context);
                }
                else
                {
                    options.Inverse(writer, (object)context);
                }
            });

            Handlebars.RegisterHelper("forceToLength", (writer, options, context, arguments) =>
            {   var value= arguments[0].ToString();
                var lenght= arguments[1].ToString().Length;
                var paddingCharacter= arguments[2].ToString();
                char paddingCharacter1= char.Parse(paddingCharacter);
                if (arguments[0] == null || arguments[0].GetType().Name == "UndefinedBindingResult")
                {
                writer.Write(arguments[0]);
                return;
                } 
                else
                {  
                    if (value.Length < lenght) {
                        value= value.PadRight(lenght, paddingCharacter1);
                        value= value.Substring(0, lenght);
                    } else if (value.Length > lenght) {
                        value= value.Substring(0, lenght);
                    }   
                    writer.Write(value);   
                }           
            });

            Handlebars.RegisterHelper ("substring", (writer, context, arguments) => 
            writer.Write (arguments[0].ToString().Substring(int.Parse(arguments[1].ToString()), int.Parse(arguments[2].ToString()))));

            Handlebars.RegisterHelper("right", (writer, options, context, arguments) =>
            {   var numberOfCharacters= int.Parse(arguments[0].ToString()); 
                var value = arguments[1].ToString();
                var lenght = value.Length;
                if (numberOfCharacters <= 0)
                {
                writer.Write("");
                return;
                
                } 
                if(numberOfCharacters > lenght){
                    writer.Write(arguments[1]);
                }
                else { 
                    writer.Write(value.Substring(lenght -(lenght - numberOfCharacters))); 
                }

            });

            Handlebars.RegisterHelper("array", (writer, options, context, arguments) =>
             {   var token = JToken.Parse(arguments[0].ToString());
                 
                 if (token is JArray) 
                {
                    options.Template(writer, (object)context);
                }
                else
                {
                    options.Inverse(writer, (object)context);
                }
            });

            Output(Template, json);
            Console.WriteLine(json);
        }     
        static void Output(string Template, string json)
        {
            var compile= Handlebars.Compile(Template);
            var context = JsonConvert.DeserializeObject(json);
            var resultList= compile(context);
            // var jsonStr= "{";
            // for (int i = 0; i < resultList.Length; i++) 
            //     {
            //     jsonStr= jsonStr + resultList[i] + ", ";
            //     }
            // jsonStr= jsonStr + "}";    
            using StreamWriter file = new("WriteLines2.txt", append: true);
            file.WriteLineAsync(resultList);

        }
        
    }
}