#region Using directives

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using System.Xml.XPath;
using System.IO;

#endregion

namespace Eq2VpkTool
{
    public class Configuration
    {
        #region Constructors
        static Configuration()
        {
            instance = new Configuration();
        }


        protected Configuration()
        {
        }
        #endregion


        public static Configuration Instance
        {
            get { return instance; }
        }


        public void Load(Stream stream)
        {
            reader    = new XPathDocument(stream);
            navigator = reader.CreateNavigator();
            loaded    = true;
        }


        public string GetValue(string key)
        {
            #region Preconditions
            Debug.Assert(reader != null && navigator != null, "No XML file loaded, call Load() first.");
            #endregion

            XPathNavigator nodeNavigator;
            
            // Select the first node that matches the query.
            // If the query is invalid, XPathNavigator.SelectSingleNode() throws an exception and we return null.
            try               { nodeNavigator = navigator.SelectSingleNode(key); }
            catch (Exception) { return null; }

            // If the query is valid but it yields no results, return null.
            if (nodeNavigator == null) return null;

            // Otherwise, return the first node found.
            return nodeNavigator.Value;
        }


        /*public T? GetValue<T>(string key)
        {
            #region Preconditions
            Debug.Assert(typeof(T).IsValueType, "Type " + typeof(T).Name + " is not a value type. Use of GetValue<T>() requires a value type. Use GetValue() instead.");
            Debug.Assert(reader != null && navigator != null, "No XML file loaded, call Load() first.");
            #endregion

            XPathNavigator nodeNavigator;

            // Select the first node that matches the query.
            // If the query is invalid, XPathNavigator.SelectSingleNode() throws an exception and we return null.
            try               { nodeNavigator = navigator.SelectSingleNode(key); }
            catch (Exception) { return null; }

            // If the query is valid but yields no results, return null.
            if (nodeNavigator == null) return null;

            T? value;

            // Read the first result as a value of the required type.
            // If the format or the cast is invalid we return null.
            try               { value = (T)nodeNavigator.ValueAs(typeof(T)); }
            catch (Exception) { return null; }

            // Otherwise, we return the converted value.
            return value;
        }*/


        public IEnumerator<string> GetValues(string key)
        {
            #region Preconditions
            Debug.Assert(reader != null && navigator != null, "No XML file loaded");
            #endregion

            XPathNodeIterator nodes;

            // Select all the nodes that match the query.
            // If the query is invalid XPathNavigator.Select() throws an exception and we return an empty enumerator.
            try               { nodes = navigator.Select(key); }
            catch (Exception) { yield break; }

            // Yield values until we consume all of them.
            foreach (XPathNavigator node in nodes) yield return node.Value;
        }


        /*public IEnumerator<T> GetValues<T>(string key)
        {
            // First check whether the type parameter T is actually class String.
            // If it is, redirect this method call to GetValues().
            // Note: This workaround makes it possible to use GetValues() and GetValues<string>() interchangeably.

            if (typeof(T) == typeof(string))
            {
                // This cast is safe because we've made sure T is String.
                IEnumerator<T> iterator = GetValues(key) as IEnumerator<T>;

                // We can't just return the iterator, we have to yield all the elements.
                while (iterator.MoveNext()) yield return iterator.Current;
            }
            else
            {
                #region Preconditions
                Debug.Assert(typeof(T).IsValueType, "Type " + typeof(T).Name + " is not a value type. Use of GetValue<T>() requires a value type. Use GetValue() instead.");
                Debug.Assert(reader != null && navigator != null, "No XML file loaded, call Load() first.");
                #endregion

                XPathNodeIterator nodes;

                // Select all the nodes that match the query.
                // If the query is invalid XPathNavigator.Select throws an exception and we return null.
                try               { nodes = navigator.Select(key); }
                catch (Exception) { yield break; }

                // Yield values until we consume all of them.
                foreach (XPathNavigator node in nodes)
                {
                    T? value = null;

                    // If the format or the cast to type T is invalid we don't yield this value.
                    // Note: If none of the values have valid conversions to T, an empty enumerator is returned.
                    try               { value = (T)node.ValueAs(typeof(T)); }
                    catch (Exception) {}

                    if (value.HasValue) yield return value.Value;
                }
            }
        }*/


        public bool IsLoaded
        {
            get { return loaded; }
        }


        private bool           loaded = false;
        private XPathDocument  reader;
        private XPathNavigator navigator;

        private static Configuration instance;
    }
}

/* EOF */