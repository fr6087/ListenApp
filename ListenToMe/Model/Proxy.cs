using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace ListenToMe.Model
{
    /// <summary>
    /// trying to query LUISbotAi with techniques of Collin Blake from https://www.youtube.com/watch?v=ziLkj4PmcCE
    /// </summary>
    public class Proxy
    {
        public async static Task<Rootobject> GetJSON(string query)

        {

            var http = new HttpClient();
            //toDo: Bots für unterschiedliche Sprachen referenzieren
            var response = await http.GetAsync("https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/57c99374-f736-4f77-a952-1a1fe90500be?subscription-key=a5de48ea62014f2bbdc4ad05943f2081&timezoneOffset=0&verbose=true&q=" +
                query);

            var result = await response.Content.ReadAsStringAsync();
            Debug.Write(result);
            var serializer = new DataContractJsonSerializer(typeof(Rootobject));



            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));

            var data = (Rootobject)serializer.ReadObject(ms);

            return data;

        }

    }
    [DataContract]
    public class Rootobject
    {   [DataMember]
        public string query { get; set; }
        [DataMember]
        public Topscoringintent topScoringIntent { get; set; }
        [DataMember]
        public Intent[] intents { get; set; }
        [DataMember]
        public Entity[] entities { get; set; }
    }

    [DataContract]
    public class Topscoringintent
    {
        [DataMember]
        public string intent { get; set; }
        [DataMember]
        public float score { get; set; }
    }

    [DataContract]
    public class Intent
    {
        [DataMember]
        public string intent { get; set; }
        [DataMember]
        public float score { get; set; }
    }

    [DataContract]
    public class Entity
    {
        [DataMember]
        public string entity { get; set; }
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public int startIndex { get; set; }
        [DataMember]
        public int endIndex { get; set; }
        [DataMember]
        public float score { get; set; }
    }

}
