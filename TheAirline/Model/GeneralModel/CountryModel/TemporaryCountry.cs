﻿namespace TheAirline.Model.GeneralModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    /// <summary>
    ///     The class for a temporary country
    /// </summary>
    [Serializable]
    public class TemporaryCountry : Country
    {
        #region Constructors and Destructors

        public TemporaryCountry(TemporaryType type, Country country, DateTime startDate, DateTime endDate)
            : base(Section, country.Uid, country.ShortName, country.Region, country.TailNumberFormat)
        {
            this.Type = type;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.Countries = new List<OneToManyCountry>();
            this.CountryAfter = this;
            this.CountryBefore = this;
        }

        private TemporaryCountry(SerializationInfo info, StreamingContext ctxt)
            : base(info, ctxt)
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

        #region Enums

        public enum TemporaryType
        {
            OneToMany,

            ManyToOne
        }

        #endregion

        #region Public Properties

        [Versioning("countries")]
        public List<OneToManyCountry> Countries { get; set; }

        [Versioning("after")]
        public Country CountryAfter { get; set; }

        [Versioning("before")]
        public Country CountryBefore { get; set; }

        [Versioning("enddate")]
        public DateTime EndDate { get; set; }

        [Versioning("startdate")]
        public DateTime StartDate { get; set; }

        [Versioning("type")]
        public TemporaryType Type { get; set; }

        #endregion

        #region Public Methods and Operators

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
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

            base.GetObjectData(info, context);
        }

        public Country getCurrentCountry(DateTime date, Country originalCountry)
        {
            if (this.Type == TemporaryType.ManyToOne)
            {
                if (date < this.StartDate)
                {
                    return this.CountryBefore;
                }
                if (date >= this.StartDate && date <= this.EndDate)
                {
                    return this;
                }
                if (date > this.EndDate)
                {
                    return this.CountryAfter;
                }
            }
            if (this.Type == TemporaryType.OneToMany)
            {
                OneToManyCountry tCountry = this.Countries.Find(c => c.Country == originalCountry);

                if (tCountry == null)
                {
                    return originalCountry;
                }

                if (date >= tCountry.StartDate && date <= tCountry.EndDate)
                {
                    return this;
                }
                return originalCountry;
            }
            return null;
        }

        #endregion
    }

    [Serializable]
    //the class for a one to many temporary country
    public class OneToManyCountry : ISerializable
    {
        #region Constructors and Destructors

        public OneToManyCountry(Country country, DateTime startDate, DateTime endDate)
        {
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.Country = country;
        }

        private OneToManyCountry(SerializationInfo info, StreamingContext ctxt)
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

        [Versioning("country")]
        public Country Country { get; set; }

        [Versioning("endate")]
        public DateTime EndDate { get; set; }

        [Versioning("startdate")]
        public DateTime StartDate { get; set; }

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
    }

    //the list of temporary countries
    public class TemporaryCountries
    {
        #region Static Fields

        private static readonly List<TemporaryCountry> tCountries = new List<TemporaryCountry>();

        #endregion

        //adds a country to the list

        #region Public Methods and Operators

        public static void AddCountry(TemporaryCountry country)
        {
            tCountries.Add(country);
        }

        public static void Clear()
        {
            tCountries.Clear();
        }

        //returns all temporary countries
        public static List<Country> GetCountries()
        {
            var lCountries = new List<Country>();
            foreach (TemporaryCountry country in tCountries)
            {
                lCountries.Add(country);
            }
            return lCountries;
        }

        //returns a country
        public static Country GetCountry(string uid)
        {
            TemporaryCountry country = tCountries.Find(t => t.Uid == uid);
            return country;
        }

        //returns a temporary country which a country is a part of
        public static TemporaryCountry GetTemporaryCountry(Country country, DateTime date)
        {
            if (country == null)
            {
                return null;
            }

            TemporaryCountry tCountry =
                tCountries.Find(
                    c =>
                        c.StartDate < date && c.EndDate > date
                        && (c.CountryBefore == country || c.CountryAfter == country
                            || c.Countries.Find(tc => tc.Country.Uid == country.Uid) != null));
            return tCountry;
        }

        #endregion

        //clears the list
    }
}