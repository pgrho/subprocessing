using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Shipwreck.Subprocessing
{
    internal static class XmlHelper
    {
        public static XmlElement GetOrPrepend(this XmlElement parent, string localName, params string[] attributeNameAndValues)
        {
            return GetOrCreateChild(parent, localName, localName, true, attributeNameAndValues);
        }
        public static XmlElement GetOrAppend(this XmlElement parent, string localName, params string[] attributeNameAndValues)
        {
            return GetOrCreateChild(parent, localName, localName, false, attributeNameAndValues);
        }
        public static XmlElement GetByNameOrAppend(this XmlElement parent, string localName, string nameAttribute, params string[] attributeNameAndValues)
        {
            return GetOrCreateChild(parent, localName + "[@name=\"" + nameAttribute + "\"]", localName, false, attributeNameAndValues);
        }

        public static XmlElement GetOrCreateChild(this XmlElement parent, string selector, string localName, bool isPrepend, params string[] attributeNameAndValues)
        {
            var e = (XmlElement)parent.SelectSingleNode(selector);
            if (e == null)
            {
                e = parent.OwnerDocument.CreateElement(localName);
                if (isPrepend)
                {
                    parent.PrependChild(e);
                }
                else
                {
                    parent.AppendChild(e);
                }
            }
            for (var i = 0; i + 1 < attributeNameAndValues.Length; i += 2)
            {
                e.SetAttribute(attributeNameAndValues[i], attributeNameAndValues[i + 1]);
            }
            return e;
        }


        public static bool RemoveByName(this XmlElement parent, string localName, string nameAttribute)
        {
            var e = parent.SelectSingleNode(localName + "[@name=\"" + nameAttribute + "\"]");
            if (e == null)
            {
                return false;
            }
            parent.RemoveChild(e);
            return true;
        }
    }
}
