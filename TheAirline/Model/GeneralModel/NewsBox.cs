﻿namespace TheAirline.Model.GeneralModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    //the class for a news box
    [Serializable]
    public class NewsBox : INotifyPropertyChanged, ISerializable
    {
        #region Fields

        [Versioning("news")]
        private readonly List<News> News;

        [Versioning("hasunread")]
        private Boolean _hasunreadnews;

        #endregion

        #region Constructors and Destructors

        public NewsBox()
        {
            this.News = new List<News>();
        }

        private NewsBox(SerializationInfo info, StreamingContext ctxt)
        {
            int version = info.GetInt16("version");

            IEnumerable<FieldInfo> fields =
                this.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props =
                new List<PropertyInfo>(
                    this.GetType()
                        .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            IEnumerable<MemberInfo> propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (SerializationEntry entry in info)
            {
                MemberInfo prop =
                    propsAndFields.FirstOrDefault(
                        p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Name == entry.Name);

                if (prop != null)
                {
                    if (prop is FieldInfo)
                    {
                        ((FieldInfo)prop).SetValue(this, entry.Value);
                    }
                    else
                    {
                        ((PropertyInfo)prop).SetValue(this, entry.Value);
                    }
                }
            }

            IEnumerable<MemberInfo> notSetProps =
                propsAndFields.Where(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Version > version);

            foreach (MemberInfo notSet in notSetProps)
            {
                var ver = (Versioning)notSet.GetCustomAttribute(typeof(Versioning));

                if (ver.AutoGenerated)
                {
                    if (notSet is FieldInfo)
                    {
                        ((FieldInfo)notSet).SetValue(this, ver.DefaultValue);
                    }
                    else
                    {
                        ((PropertyInfo)notSet).SetValue(this, ver.DefaultValue);
                    }
                }
            }
        }

        #endregion

        #region Public Events

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Public Properties

        public Boolean HasUnreadNews
        {
            get
            {
                return this._hasunreadnews;
            }
            set
            {
                this._hasunreadnews = value;
                this.NotifyPropertyChanged("HasUnreadNews");
            }
        }

        #endregion

        #region Public Methods and Operators

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = this.GetType();

            IEnumerable<FieldInfo> fields =
                myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props =
                new List<PropertyInfo>(
                    myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            IEnumerable<MemberInfo> propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

                if (member is FieldInfo)
                {
                    propValue = ((FieldInfo)member).GetValue(this);
                }
                else
                {
                    propValue = ((PropertyInfo)member).GetValue(this, null);
                }

                var att = (Versioning)member.GetCustomAttribute(typeof(Versioning));

                info.AddValue(att.Name, propValue);
            }
        }

        //adds a news to the news box
        public void addNews(News news)
        {
            this.HasUnreadNews = true;
            this.News.Add(news);
        }

        public void clear()
        {
            this.News.Clear();
        }

        //removes a news from the news box

        //returns all new
        public List<News> getNews()
        {
            return this.News;
        }

        //returns all news for a specific type
        public List<News> getNews(News.NewsType type)
        {
            return this.News.FindAll((delegate(News n) { return n.Type == type; }));
        }

        //returns all news for a specific period
        public List<News> getNews(DateTime fromDate, DateTime toDate)
        {
            return this.News.FindAll((delegate(News n) { return n.Date >= fromDate && n.Date <= toDate; }));
        }

        //returns all unread news
        public List<News> getUnreadNews()
        {
            return this.News.FindAll((delegate(News n) { return !n.IsRead; }));
        }

        public void removeNews(News news)
        {
            this.News.Remove(news);
            this.HasUnreadNews = this.News.Exists(n => n.IsUnRead);
        }

        #endregion

        //clears the list of news

        #region Methods

        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }

    //the class for a news
    [Serializable]
    public class News : ISerializable
    {
        #region Constructors and Destructors

        public News(NewsType type, DateTime date, string subject, string body, Boolean isactionnews = false)
        {
            this.Type = type;
            this.Date = date;
            this.Subject = subject;
            this.Body = body;
            this.IsRead = false;
            this.IsActionNews = isactionnews;
        }

        private News(SerializationInfo info, StreamingContext ctxt)
        {
            int version = info.GetInt16("version");

            IEnumerable<FieldInfo> fields =
                this.GetType()
                    .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props =
                new List<PropertyInfo>(
                    this.GetType()
                        .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            IEnumerable<MemberInfo> propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (SerializationEntry entry in info)
            {
                MemberInfo prop =
                    propsAndFields.FirstOrDefault(
                        p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Name == entry.Name);

                if (prop != null)
                {
                    if (prop is FieldInfo)
                    {
                        ((FieldInfo)prop).SetValue(this, entry.Value);
                    }
                    else
                    {
                        ((PropertyInfo)prop).SetValue(this, entry.Value);
                    }
                }
            }

            IEnumerable<MemberInfo> notSetProps =
                propsAndFields.Where(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Version > version);

            foreach (MemberInfo notSet in notSetProps)
            {
                var ver = (Versioning)notSet.GetCustomAttribute(typeof(Versioning));

                if (ver.AutoGenerated)
                {
                    if (notSet is FieldInfo)
                    {
                        ((FieldInfo)notSet).SetValue(this, ver.DefaultValue);
                    }
                    else
                    {
                        ((PropertyInfo)notSet).SetValue(this, ver.DefaultValue);
                    }
                }
            }
            if (version == 1)
            {
                this.IsActionNews = false;
            }
        }

        #endregion

        #region Delegates

        public delegate void ActionHandler(object o);

        #endregion

        #region Public Events

        public event ActionHandler Action;

        #endregion

        #region Enums

        public enum NewsType
        {
            Standard_News,

            Airport_News,

            Flight_News,

            Fleet_News,

            Airline_News,

            Alliance_News,

            Airliner_News
        }

        #endregion

        #region Public Properties

        [Versioning("actionobject", Version = 2)]
        public object ActionObject { get; set; }

        [Versioning("body")]
        public string Body { get; set; }

        [Versioning("date")]
        public DateTime Date { get; set; }

        [Versioning("actionnews", Version = 2)]
        public Boolean IsActionNews { get; set; }

        [Versioning("isread")]
        public Boolean IsRead { get; set; }

        public Boolean IsUnRead
        {
            get
            {
                return !this.IsRead;
            }
            set
            {
                ;
            }
        }

        [Versioning("subject")]
        public string Subject { get; set; }

        [Versioning("type")]
        public NewsType Type { get; set; }

        #endregion

        #region Public Methods and Operators

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 2);

            Type myType = this.GetType();

            IEnumerable<FieldInfo> fields =
                myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props =
                new List<PropertyInfo>(
                    myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                        .Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            IEnumerable<MemberInfo> propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

                if (member is FieldInfo)
                {
                    propValue = ((FieldInfo)member).GetValue(this);
                }
                else
                {
                    propValue = ((PropertyInfo)member).GetValue(this, null);
                }

                var att = (Versioning)member.GetCustomAttribute(typeof(Versioning));

                info.AddValue(att.Name, propValue);
            }
        }

        public void executeNews()
        {
            if (this.Action != null)
            {
                this.Action(this.ActionObject);
            }
        }

        #endregion
    }
}