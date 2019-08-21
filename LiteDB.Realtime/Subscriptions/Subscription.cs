﻿using LiteDB.Engine;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LiteDB.Realtime.Subscriptions
{
    internal class Subscription<T> : ISubscription where T : class
    {
        private LiteDatabase _database;
        public string Collection { get; set; }
        public BsonValue Id { get; set; }
        public Type Type => typeof(T);
        public object ObserverObject { get; set; }
        public bool IsCollection { get; set; } = false;
        public IObserver<T> AsDocumentObserver() => ObserverObject as IObserver<T>;
        public IObserver<List<T>> AsCollectionObserver() => ObserverObject as IObserver<List<T>>;

        public Subscription(LiteDatabase database)
        {
            _database = database;
        }

        public void OnNext()
        {
            if (IsCollection)
            {
                var nextValue = _database.GetCollection<T>(Collection).Query().ToList();
                Task.Run(() => AsCollectionObserver()?.OnNext(nextValue));
            }
            else
            {
                var nextValue = _database.GetCollection<T>(Collection).Query()
                    .Where("_id = @0", Id)
                    .SingleOrDefault();
                Task.Run(() => AsDocumentObserver()?.OnNext(nextValue));
            }
        }
    }
}