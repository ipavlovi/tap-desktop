﻿namespace TheAirline.Model.PilotModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    using TheAirline.Model.AirportModel;
    using TheAirline.Model.GeneralModel;

    //the class for a flight school
    [Serializable]
    public class FlightSchool : ISerializable
    {
        #region Constants

        public const int MaxNumberOfInstructors = 15;

        public const int MaxNumberOfStudentsPerInstructor = 2;

        #endregion

        #region Constructors and Destructors

        public FlightSchool(Airport airport)
        {
            Guid id = Guid.NewGuid();

            this.Airport = airport;
            this.Name = string.Format("Flight School {0}", this.Airport.Profile.Town.Name);
            this.Students = new List<PilotStudent>();
            this.Instructors = new List<Instructor>();
            this.TrainingAircrafts = new List<TrainingAircraft>();
            this.ID = id.ToString();
        }

        private FlightSchool(SerializationInfo info, StreamingContext ctxt)
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

        [Versioning("airport")]
        public Airport Airport { get; set; }

        [Versioning("id")]
        public string ID { get; set; }

        [Versioning("instructors")]
        public List<Instructor> Instructors { get; set; }

        [Versioning("name")]
        public string Name { get; set; }

        public int NumberOfInstructors
        {
            get
            {
                return this.Instructors.Count;
            }
            set
            {
                ;
            }
        }

        public int NumberOfStudents
        {
            get
            {
                return this.Students.Count;
            }
            set
            {
                ;
            }
        }

        [Versioning("students")]
        public List<PilotStudent> Students { get; set; }

        [Versioning("aircrafts")]
        public List<TrainingAircraft> TrainingAircrafts { get; set; }

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

        //adds an instructor to the flight school
        public void addInstructor(Instructor instructor)
        {
            this.Instructors.Add(instructor);
        }

        //removes an instructor from the flight school

        //adds a student to the flight school
        public void addStudent(PilotStudent student)
        {
            this.Students.Add(student);
        }

        public void addTrainingAircraft(TrainingAircraft aircraft)
        {
            this.TrainingAircrafts.Add(aircraft);
        }

        public void removeInstructor(Instructor instructor)
        {
            this.Instructors.Remove(instructor);
        }

        //removes a student from the flight school
        public void removeStudent(PilotStudent student)
        {
            this.Students.Remove(student);
        }

        public void removeTrainingAircraft(TrainingAircraft aircraft)
        {
            this.TrainingAircrafts.Remove(aircraft);
        }

        #endregion
    }
}