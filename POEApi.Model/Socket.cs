using System;
namespace POEApi.Model
{
    [Serializable]
    public class Socket
    {
        public string Attribute { get; set; }
        public int Group { get; set; }

        internal Socket(JSONProxy.Socket s)
        {
            this.Attribute = s.Attribute;
            this.Group = s.Group;
        }
    }
}
