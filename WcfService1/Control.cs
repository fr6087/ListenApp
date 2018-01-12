using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WcfService1
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Control
    {
        //e.g. "input" -> textbox or ? -> radiobutton
        [JsonProperty]
        public string Type { get; set; }

        // Attributes of html node
        [JsonProperty]
        public List<KeyValuePair<String, String>> Attributes { get; set; }

        // containing the text inbetweeen <span> someText</span> elements
        [JsonProperty]
        public string Text { get; set; }
    }
}