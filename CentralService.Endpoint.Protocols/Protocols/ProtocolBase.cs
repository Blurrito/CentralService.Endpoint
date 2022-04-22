using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CentralService.Endpoint.Protocols.Protocols
{
    internal static class ProtocolBase
    {
        public  static List<KeyValuePair<string, string>> GetStructProperties<TType>(TType Struct) where TType : struct
        {
            List<KeyValuePair<string, string>> ReturnList = new List<KeyValuePair<string, string>>();
            PropertyInfo[] StructProperties = Struct.GetType().GetProperties();
            foreach (PropertyInfo Property in StructProperties)
            {
                object PropertyValue = Property.GetValue(Struct);
                if (PropertyValue != null)
                    ReturnList.Add(new KeyValuePair<string, string>(Property.Name, Property.GetType() == typeof(float) ? PropertyValue.ToString().Replace(',', '.') : PropertyValue.ToString()));
            }
            return ReturnList;
        }

        public static TType SetStructProperties<TType>(List<KeyValuePair<string, string>> ObjectProperties) where TType : struct
        {
            object ReturnObject = new TType();
            PropertyInfo[] StructProperties = ReturnObject.GetType().GetProperties();
            foreach (PropertyInfo Property in StructProperties)
            {
                KeyValuePair<string, string> PropertyValue = ObjectProperties.FirstOrDefault(x => x.Key == Property.Name.ToLower());
                if (PropertyValue.Value != null && Property.CanWrite)
                    Property.SetValue(ReturnObject, Convert.ChangeType(Property.GetType() == typeof(float) ? PropertyValue.Value.Replace('.', ',') : PropertyValue.Value, Property.PropertyType));
            }
            return (TType)ReturnObject;
        }

        public static string GetResponseString(List<KeyValuePair<string, string>> StructProperties, string ItemSeparator, string KeyValueSeparator, bool UseBase64 = false, bool AllowEmptyValues = false)
        {
            string Output = string.Empty;
            foreach (KeyValuePair<string, string> Property in StructProperties)
            {
                string CombinedProperty = Combine(Property.Key, Property.Value, KeyValueSeparator, UseBase64, AllowEmptyValues);
                Output = Combine(Output, CombinedProperty, ItemSeparator);
            }
            return Output;
        }

        public static List<KeyValuePair<string, string>> GetRequestProperties(string Object, string ItemSeparator, string KeyValueSeparator, bool UseBase64 = false)
        {
            List<KeyValuePair<string, string>> ReturnList = new List<KeyValuePair<string, string>>();
            string[] SplitObject = Object.Split(ItemSeparator);
            if (ItemSeparator == KeyValueSeparator)
            {
                if (SplitObject.Length % 2 != 0)
                    throw new ArgumentException("Incomplete or corrupted request.");
                for (int i = 0; i < SplitObject.Length; i += 2)
                    //Dirty solution, re-evaluate this later
                    ReturnList.Add(Split(SplitObject[i] + ItemSeparator + SplitObject[i + 1], ItemSeparator, UseBase64));
            }
            else
                foreach (string Parameter in SplitObject)
                    ReturnList.Add(Split(Parameter, KeyValueSeparator, UseBase64));
            return ReturnList;
        }

        private static KeyValuePair<string, string> Split(string Value, string Separator, bool UseBase64 = false)
        {
            string[] SplitValue = Value.Split(Separator);
            if (SplitValue.Length > 1)
            {
                if (UseBase64)
                    SplitValue[1] = FromBase64String(SplitValue[1]);
                return new KeyValuePair<string, string>(SplitValue[0], SplitValue[1]);
            }
            return new KeyValuePair<string, string>(SplitValue[0], string.Empty);
        }

        private static string Combine(string Value1, string Value2, string Separator, bool UseBase64 = false, bool AllowEmptyValue = false)
        {
            if (!AllowEmptyValue && Value1 == string.Empty)
                return UseBase64 ? ToBase64String(Value2) : Value2;
            else if (!AllowEmptyValue && Value2 == string.Empty)
                return Value1;
            else if (!AllowEmptyValue && Value1 == string.Empty && Value2 == string.Empty)
                return string.Empty;
            return Value1 + Separator + (UseBase64 ? ToBase64String(Value2) : Value2);
        }

        public static string FromBase64String(string Input)
        {
            string ReturnString = Input.Replace('*', '=');
            ReturnString = ReturnString.Replace(".", "") + new string('=', Input.Count(x => x == '.'));
            byte[] ConvertedOutput = Convert.FromBase64String(ReturnString);
            return Encoding.UTF8.GetString(ConvertedOutput);
        }

        public static string ToBase64String(string Input)
        {
            byte[] ConvertedInput = Encoding.UTF8.GetBytes(Input);
            string Output = Convert.ToBase64String(ConvertedInput);
            return Output.Replace('=', '*');
        }
    }
}
