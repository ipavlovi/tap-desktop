﻿namespace TheAirline.Model.GeneralModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    /*! GameTimeZone.
* This is used for a time zone in the game
* The class needs parameters names of time zone and the utc offset
     */

    [Serializable]
    public class GameTimeZone : ISerializable
    {
        #region Constructors and Destructors

        public GameTimeZone(string name, string shortName, TimeSpan utcOffset)
        {
            this.Name = name;
            this.ShortName = shortName;
            this.UTCOffset = utcOffset;
        }

        private GameTimeZone(SerializationInfo info, StreamingContext ctxt)
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

        #region Public Properties

        public string DisplayName
        {
            get
            {
                return this.getDisplayName();
            }
            set
            {
                ;
            }
        }

        [Versioning("name")]
        public string Name { get; set; }

        public string ShortDisplayName
        {
            get
            {
                return this.getShortDisplayName();
            }
            set
            {
                ;
            }
        }

        [Versioning("shortname")]
        public string ShortName { get; set; }

        [Versioning("offset")]
        public TimeSpan UTCOffset { get; set; }

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

        #endregion

        //returns the display name

        #region Methods

        private string getDisplayName()
        {
            return string.Format(
                "{0} (UTC{1}{2:D2}:{3:D2})",
                this.Name,
                this.UTCOffset.Hours < 0 ? "" : "+",
                this.UTCOffset.Hours,
                Math.Abs(this.UTCOffset.Minutes));
        }

        //returns the short display name
        private string getShortDisplayName()
        {
            return string.Format(
                "{0} (UTC{1}{2:D2}:{3:D2})",
                this.ShortName,
                this.UTCOffset.Hours < 0 ? "" : "+",
                this.UTCOffset.Hours,
                Math.Abs(this.UTCOffset.Minutes));
        }

        #endregion
    }

    //the list of time zones
    public class TimeZones
    {
        #region Static Fields

        private static readonly List<GameTimeZone> timeZones = new List<GameTimeZone>();

        #endregion

        //clears the list

        //adds a time zone to the list

        #region Public Methods and Operators

        public static void AddTimeZone(GameTimeZone tz)
        {
            timeZones.Add(tz);
        }

        public static void Clear()
        {
            timeZones.Clear();
        }

        //returns the list of time zones
        public static List<GameTimeZone> GetTimeZones()
        {
            return timeZones;
        }

        #endregion
    }
}