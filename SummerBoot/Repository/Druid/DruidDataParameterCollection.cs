using System;
using System.Collections;
using System.Data;

namespace SummerBoot.Repository.Druid
{
    public class DruidDataParameterCollection : IDataParameterCollection
    {
        public object this[string parameterName] { get => DataParameterCollection[parameterName]; set => DataParameterCollection[parameterName]=value; }
        public object this[int index] { get => DataParameterCollection[index]; set => DataParameterCollection[index]=value; }

        public bool IsFixedSize => DataParameterCollection.IsFixedSize;

        public bool IsReadOnly => DataParameterCollection.IsReadOnly;

        public int Count => DataParameterCollection.Count;

        public bool IsSynchronized => DataParameterCollection.IsSynchronized;

        public object SyncRoot => DataParameterCollection.SyncRoot;

        private IDataParameterCollection DataParameterCollection { set; get; }

        public DruidDataParameterCollection(IDataParameterCollection dataParameterCollection)
        {
            DataParameterCollection = dataParameterCollection;
        }

        public int Add(object value)
        {
            return DataParameterCollection.Add(value);
        }

        public void Clear()
        {
            DataParameterCollection.Clear();
        }

        public bool Contains(string parameterName)
        {
            return DataParameterCollection.Contains(parameterName);
        }

        public bool Contains(object value)
        {
            return DataParameterCollection.Contains(value);
        }

        public void CopyTo(Array array, int index)
        {
            DataParameterCollection.CopyTo(array,index);
        }

        public IEnumerator GetEnumerator()
        {
           return DataParameterCollection.GetEnumerator();
        }

        public int IndexOf(string parameterName)
        {
            return DataParameterCollection.IndexOf(parameterName);
        }

        public int IndexOf(object value)
        {
            return DataParameterCollection.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            DataParameterCollection.Insert(index, value);
        }

        public void Remove(object value)
        {
            DataParameterCollection.Remove(value);
        }

        public void RemoveAt(string parameterName)
        {
            DataParameterCollection.RemoveAt(parameterName);
        }

        public void RemoveAt(int index)
        {
            DataParameterCollection.RemoveAt(index);
        }
    }
}