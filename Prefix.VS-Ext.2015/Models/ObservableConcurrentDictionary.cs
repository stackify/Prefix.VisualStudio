using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Prefix.VSExt2015.Models
{
    public class ObservableConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private const string CountString = "Count";
        private const string IndexerName = "Item[]";
        private ConcurrentDictionary<TKey, TValue> _Dictionary;
        protected ConcurrentDictionary<TKey, TValue> Dictionary => _Dictionary;

        #region Constructors
        public ObservableConcurrentDictionary()
        {
            _Dictionary = new ConcurrentDictionary<TKey, TValue>();
        }
        public ObservableConcurrentDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _Dictionary = new ConcurrentDictionary<TKey, TValue>(dictionary);
        }
        public ObservableConcurrentDictionary(IEqualityComparer<TKey> comparer)
        {
            _Dictionary = new ConcurrentDictionary<TKey, TValue>(comparer);
        }
        public ObservableConcurrentDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
        {
            _Dictionary = new ConcurrentDictionary<TKey, TValue>(dictionary, comparer);
        }
        #endregion

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value) => Insert(key, value, true);

        public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);

        public ICollection<TKey> Keys => Dictionary.Keys;

        public bool Remove(TKey key)
        {
            if (key == null) throw new ArgumentNullException("key");

            TValue value;
            var removed = Dictionary.TryRemove(key, out value);
            if (removed) OnCollectionChanged(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value));
            return removed;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values => Dictionary.Values; 

        public TValue this[TKey key]
        {
            get
            {
                return Dictionary[key];
            }
            set
            {
                Insert(key, value, false);
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Insert(item.Key, item.Value, true);
        }

        public void Clear()
        {
            if (Dictionary.Count <= 0) return;
            Dictionary.Clear();
            OnCollectionChanged();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return Dictionary.ContainsKey(item.Key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count => Dictionary.Count;

        public bool IsReadOnly => false;

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }


        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return Dictionary.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void AddRange(IDictionary<TKey, TValue> items)
        {
            foreach (var value in items)
            {
                Dictionary.AddOrUpdate(value.Key, value.Value, (key, value1) => value.Value);
            }
            OnCollectionChanged();
        }

        private void Insert(TKey key, TValue value, bool add)
        {
            if (key == null) throw new ArgumentNullException("key");

            TValue item;
            if (Dictionary.TryGetValue(key, out item))
            {
                if (add) throw new ArgumentException("An item with the same key has already been added.");
                if (Equals(item, value)) return;
                Dictionary[key] = value;

                OnCollectionChanged(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(key, value), new KeyValuePair<TKey, TValue>(key, item));
            }
            else
            {
                Dictionary[key] = value;

                OnCollectionChanged(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        private void OnPropertyChanged()
        {
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
        }

        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void OnCollectionChanged()
        {
            OnPropertyChanged();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> changedItem)
        {
            OnPropertyChanged();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, changedItem));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> newItem, KeyValuePair<TKey, TValue> oldItem)
        {
            OnPropertyChanged();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, IList newItems)
        {
            OnPropertyChanged();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, newItems));
        }
    }
}
