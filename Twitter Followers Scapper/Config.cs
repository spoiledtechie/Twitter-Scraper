using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

[DataContract]
class Config
{
    [DataMember(Order = 0)]
    public string Target { get; set; } = "";

    public Config()
    {
    }

    public static Config ParseFromJson(String Json)
    {
        MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(Json));
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Config));
        Config r = (Config)serializer.ReadObject(stream);
        stream.Close();
        return r;
    }

    public string ToJson()
    {
        MemoryStream stream = new MemoryStream();
        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Config));
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
        return $"Target: {Target}";
    }
}