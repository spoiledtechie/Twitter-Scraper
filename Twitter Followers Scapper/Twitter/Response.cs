using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Twitter
{
    [DataContract]
    class Response
    {
        [DataMember (Name = "min_position", Order = 0)]
        public string MinPosition { get; set; }
        [DataMember (Name = "has_more_items", Order = 1)]
        public bool HasMoreItems { get; set; }
        [DataMember(Name = "items_html", Order = 2)]
        public string ItemsHtml { get; set; }
        [DataMember(Name = "new_latent_count", Order = 3)]
        public int NewLatentCount { get; set; }

        public Response()
        {

        }

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

        
        public override string ToString()
        {
            return "MinPosition: " + MinPosition + "\n" +
                    "HasMoreItems: " + HasMoreItems + "\n" +
                    "ItemsHtml: " + ItemsHtml + "\n" +
                    "NewLatentCount: " + NewLatentCount;
        }
    }
}
