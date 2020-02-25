using System;
using Microsoft.Xrm.Sdk;

namespace CrmOrganizationParamsTool
{
    public class OrganizationParam
    {
        public string Name { get; set; }

        public object Value { get; set; }

        public OrganizationParam(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string GetValueString()
        {
            if (Value is EntityReference)
            {
                var value = (EntityReference)Value;
                return value.LogicalName + " " + value.Id;
            }
            else if (Value is OptionSetValue)
            {
                var value = (OptionSetValue)Value;
                return value.Value.ToString();
            }
            else
            {
                return Value.ToString();
            }
        }

        public string GetValueTypeString()
        {
            return Value.GetType().ToString().Replace("Microsoft.Xrm.Sdk.", string.Empty).Replace("System.", string.Empty);
        }

        public void SetValue(string valueString)
        {
            if (Value is EntityReference)
            {
                if (valueString.Length >= 36)
                {
                    var idString = valueString.Substring(valueString.Length - 36);
                    var textString = valueString.Replace(idString, string.Empty).Trim();
                    Guid id;
                    if (Guid.TryParse(idString, out id))
                    {
                        Value = new EntityReference(textString, id);
                    }
                    else
                    {
                        throw new Exception("Wrong value type format for " + GetValueTypeString() + Environment.NewLine + "Value format must be: entity_name entity_guid" + Environment.NewLine);
                    }
                }
                else
                {
                    throw new Exception("Wrong value type format for " + GetValueTypeString() + Environment.NewLine + "Value format must be: entity_name entity_guid" + Environment.NewLine);
                }
            }
            else if (Value is OptionSetValue)
            {
                int number;
                if (int.TryParse(valueString, out number))
                {
                    Value = new OptionSetValue(number);
                }
                else
                {
                    throw new Exception("Wrong value type format for " + GetValueTypeString() + Environment.NewLine + "Value must be an integer number" + Environment.NewLine);
                }
            }
            else if (Value is int)
            {
                int number;
                if (int.TryParse(valueString, out number))
                {
                    Value = number;
                }
                else
                {
                    throw new Exception("Wrong value type format for " + GetValueTypeString() + Environment.NewLine + "Value must be an integer number" + Environment.NewLine);
                }
            }
            else if (Value is bool)
            {
                bool boolean;
                if (bool.TryParse(valueString, out boolean))
                {
                    Value = boolean;
                }
                else
                {
                    throw new Exception("Wrong value type format for " + GetValueTypeString() + Environment.NewLine + "Value must be True or False" + Environment.NewLine);
                }
            }
            else
            {
                Value = valueString;
            }
        }

        public override string ToString()
        {
            return Name + " = " + Value.ToString();
        }
    }
}