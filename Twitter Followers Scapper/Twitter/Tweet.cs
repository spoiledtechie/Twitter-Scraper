using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Twitter
{
    [DataContract]
    class Tweet
    {
        [DataMember (Order = 0)]
        public string Name { get; set; }
        [DataMember(Order = 1)]
        public string Handle { get; set; }
        [DataMember(Order = 2)]
        public DateTime Time { get; set; }
        [DataMember(Order = 3)]
        public string Text { get; set; }
        [DataMember(Order = 4)]
        public int Likes { get; set; }
        [DataMember(Order = 5)]
        public int Retweets { get; set; }
        [DataMember(Order = 6)]
        public int Replies { get; set; }

        public static Response ParseFromJson(String Json)
        {
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(Json));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Response));
            Response r = (Response)serializer.ReadObject(stream);
            stream.Close();
            return r;
        }

        public string ToJson()
        {
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Response));
            serializer.WriteObject(stream, this);
            stream.Position = 0;
            StreamReader streamReader = new StreamReader(stream);
            string json = streamReader.ReadToEnd();
            streamReader.Close();
            stream.Close();
            return json;
        }

        public static string GetCsvHeader()
        {
            return "Name,Handle,Time,Text,Likes,Retweets,Replies";
        }

        public string ToCsv()
        {
            return $"{Name},{Handle},{Time},{Text},{Likes},{Retweets},{Replies}";
        }
    }
}
