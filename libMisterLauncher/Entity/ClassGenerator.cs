using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace libMisterLauncher.Entity
{
    

    public class ClassGenerator
    {
        public string Name { get; set; }
        public Dictionary<int, PropertyGenerator> Properties { get; set; }
        public ClassGenerator(string name) 
        {
            Name = name;
            Properties = new Dictionary<int, PropertyGenerator>();
        }

        public bool AddProperty (PropertyGenerator prop)
        {
            if (!Properties.ContainsKey(prop.Id))
            {
                Properties.Add(prop.Id, prop);
                return true;
            }
            return false;
        }

        public string GetPropertiesKey ()
        {
            var result = new StringBuilder();
            foreach (var key in Properties.Keys)
            {
                result.Append("[" + key + "]");
            }
            return result.ToString();
        }

        public bool IsEqual (ClassGenerator c)
        {
            return GetPropertiesKey() == c.GetPropertiesKey();
        }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.AppendLine("public class " + Name);
            result.AppendLine("{");
            foreach (var prop in Properties.Keys)
            {
                result.AppendLine(Properties[prop].ToString());
            }
            result.AppendLine("}");

            return result.ToString();

        }


    }

    public class PropertyGenerator
    {
        public int Id
        {
            get
            {
               return getId().GetHashCode();
            }
        }
        public string Name { get; set; }
        public string PropertyType { get; set; }


        public PropertyGenerator (string name, string type)
        {
            Name = name;
            PropertyType = "";
            CastPropertyType(type);
        }
        public void CastPropertyType (string type)
        {
            switch(type)
            {
                case "BsonString":
                    PropertyType = "string";
                    break;
                case "BsonInt32":
                    PropertyType = "int";
                    break;
                case "BsonBoolean":
                    PropertyType = "bool";
                    break;
                case "BsonDateTime":
                    PropertyType = "DateTime";
                    break;
                default:
                    PropertyType = type;
                    break;
            }
        }

        private string getId()
        {
            return  Name + "(" + PropertyType  + ")";
        }

        public override string ToString()
        {
            return "\tpublic " + PropertyType + " " + Name + " { get; set; }";
        }



    }

}
