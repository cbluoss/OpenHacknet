using System.Xml;

namespace Hacknet
{
    public struct UserDetail
    {
        public string name;
        public string pass;
        public byte type;
        public bool known;

        public UserDetail(string user, string password, byte accountType)
        {
            name = user;
            pass = password;
            type = accountType;
            known = false;
        }

        public UserDetail(string user)
        {
            name = user;
            pass = PortExploits.getRandomPassword();
            type = 1;
            known = false;
        }

        public string getSaveString()
        {
            return "<user name=\"" + (object) name + "\" pass=\"" + pass + "\" type=\"" + type + "\" known=\"" + known +
                   "\" />";
        }

        public static UserDetail loadUserDetail(XmlReader reader)
        {
            reader.MoveToAttribute("name");
            var user = reader.ReadContentAsString();
            reader.MoveToAttribute("pass");
            var password = reader.ReadContentAsString();
            reader.MoveToAttribute("type");
            var accountType = (byte) reader.ReadContentAsInt();
            reader.MoveToAttribute("known");
            var flag = reader.ReadContentAsString().ToLower().Equals("true");
            return new UserDetail(user, password, accountType)
            {
                known = flag
            };
        }

        public override bool Equals(object obj)
        {
            var nullable = obj as UserDetail?;
            if (nullable.HasValue && name == nullable.Value.name &&
                (pass == nullable.Value.pass && type == nullable.Value.type))
                return known == nullable.Value.known;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}