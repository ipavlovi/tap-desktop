﻿namespace TheAirline.Model.AirlinerModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    using TheAirline.Model.AirlineModel;
    using TheAirline.Model.GeneralModel;

    //the class for an airliner
    [Serializable]
    public class Airliner : ISerializable
    {
        #region Fields

        private readonly Random rnd = new Random();

        #endregion

        #region Constructors and Destructors

        public Airliner(string id, AirlinerType type, string tailNumber, DateTime builtDate)
        {
            this.ID = id;
            this.BuiltDate = new DateTime(builtDate.Year, builtDate.Month, builtDate.Day);
            this.Type = type;
            this.LastServiceCheck = 0;
            this.TailNumber = tailNumber;
            this.Flown = 0;
            this.Condition = this.rnd.Next(90, 100);
            this.Classes = new List<AirlinerClass>();

            if (this.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Passenger)
            {
                var aClass = new AirlinerClass(
                    AirlinerClass.ClassType.Economy_Class,
                    ((AirlinerPassengerType)this.Type).MaxSeatingCapacity);
                aClass.createBasicFacilities(this.Airline);
                this.Classes.Add(aClass);
            }

            if (this.Type.TypeAirliner == AirlinerType.TypeOfAirliner.Cargo)
            {
                var aClass = new AirlinerClass(AirlinerClass.ClassType.Economy_Class, 0);
                aClass.createBasicFacilities(this.Airline);
                this.Classes.Add(aClass);
            }
        }

        private Airliner(SerializationInfo info, StreamingContext ctxt)
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

            this.Classes.RemoveAll(c => c == null);

            var doubleClasses =
                new List<AirlinerClass.ClassType>(
                    this.Classes.Where(c => this.Classes.Count(cc => cc.Type == c.Type) > 1).Select(c => c.Type));

            foreach (AirlinerClass.ClassType doubleClassType in doubleClasses)
            {
                AirlinerClass dClass = this.Classes.Last(c => c.Type == doubleClassType);
                this.Classes.Remove(dClass);
            }
        }

        #endregion

        #region Public Properties

        public int Age
        {
            get
            {
                return this.getAge();
            }
            private set
            {
            }
        }

        [Versioning("airline")]
        public Airline Airline { get; set; }

        [Versioning("built")]
        public DateTime BuiltDate { get; set; }

        [Versioning("classes")]
        public List<AirlinerClass> Classes { get; set; }

        [Versioning("condition")]
        public double Condition { get; set; }

        [Versioning("flown")]
        public double Flown { get; set; }

        [Versioning("fuelcapacity")]
        public long FuelCapacity { get; set; }

        [Versioning("id")]
        public string ID { get; set; }

        //distance flown by the airliner

        [Versioning("lastservice")]
        public double LastServiceCheck { get; set; } //the km were the airliner was last at service

        public long LeasingPrice
        {
            get
            {
                return this.getLeasingPrice();
            }
            private set
            {
            }
        }

        public long Price
        {
            get
            {
                return this.getPrice();
            }
            private set
            {
            }
        }

        public Country Registered
        {
            get
            {
                return Countries.GetCountryFromTailNumber(this.TailNumber);
            }
            private set
            {
                ;
            }
        }

        [Versioning("tailnumber")]
        public string TailNumber { get; set; }

        [Versioning("type")]
        public AirlinerType Type { get; set; }

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

        //adds a new airliner class to the airliner
        public void addAirlinerClass(AirlinerClass airlinerClass)
        {
            if (airlinerClass != null && !this.Classes.Exists(c => c.Type == airlinerClass.Type))
            {
                this.Classes.Add(airlinerClass);

                if (airlinerClass.getFacilities().Count == 0)
                {
                    airlinerClass.createBasicFacilities(this.Airline);
                }
            }
        }

        //removes an airliner class from the airliner

        //clear the list of airliner classes
        public void clearAirlinerClasses()
        {
            this.Classes.Clear();
        }

        public int getAge()
        {
            return MathHelpers.CalculateAge(this.BuiltDate, GameObject.GetInstance().GameTime);
        }

        //returns the total amount of seat capacity

        //returns the airliner class for the airliner
        public AirlinerClass getAirlinerClass(AirlinerClass.ClassType type)
        {
            if (this.Classes.Exists(c => c.Type == type))
            {
                return this.Classes.Find(c => c.Type == type);
            }
            return this.Classes[0];
        }

        public double getCargoCapacity()
        {
            if (this.Type is AirlinerCargoType)
            {
                return ((AirlinerCargoType)this.Type).CargoSize;
            }
            return 0;
        }

        public long getLeasingPrice()
        {
            double months = 12 * 15;
            double rate = 1.30;

            double leasingPrice = (this.getPrice() * rate / months);
            return Convert.ToInt64(leasingPrice);
        }

        public long getPrice()
        {
            double basePrice = this.Type.Price;

            double facilityPrice = 0;

            var classes = new List<AirlinerClass>(this.Classes);

            foreach (AirlinerClass aClass in classes)
            {
                AirlinerFacility audioFacility = aClass.getFacility(AirlinerFacility.FacilityType.Audio);
                AirlinerFacility videoFacility = aClass.getFacility(AirlinerFacility.FacilityType.Video);
                AirlinerFacility seatFacility = aClass.getFacility(AirlinerFacility.FacilityType.Seat);

                double audioPrice = audioFacility.PricePerSeat * audioFacility.PercentOfSeats * aClass.SeatingCapacity;
                double videoPrice = videoFacility.PricePerSeat * videoFacility.PercentOfSeats * aClass.SeatingCapacity;
                double seatPrice = seatFacility.PricePerSeat * seatFacility.PercentOfSeats * aClass.SeatingCapacity;

                facilityPrice += audioPrice + videoPrice + seatPrice;
            }

            basePrice += facilityPrice;

            int age = this.getAge();
            double devaluationPercent = 1 - (0.02 * age);

            return Convert.ToInt64(basePrice * devaluationPercent * (this.Condition / 100));
        }

        public int getTotalSeatCapacity()
        {
            int capacity = 0;
            foreach (AirlinerClass aClass in this.Classes)
            {
                capacity += aClass.SeatingCapacity;
            }

            return capacity;
        }

        public long getValue()
        {
            if (this.getAge() < 25)
            {
                return this.getPrice() * (1 - (long)this.getAge() * (3 / 100));
            }
            return this.getPrice() * (20 / 100);
        }

        public void removeAirlinerClass(AirlinerClass airlinerClass)
        {
            this.Classes.Remove(airlinerClass);
        }

        #endregion
    }

    //the list of airliners
    public class Airliners
    {
        #region Static Fields

        private static List<Airliner> airliners = new List<Airliner>();

        #endregion

        //clears the list

        //adds an airliner to the list

        #region Public Methods and Operators

        public static void AddAirliner(Airliner airliner)
        {
            lock (airliners)
            {
                //if (airliners.Exists(a => a.ID == airliner.ID))
                //  throw new Exception("Airliner element already exists exception");

                airliners.Add(airliner);
            }
        }

        public static void Clear()
        {
            airliners = new List<Airliner>();
        }

        //returns an airliner
        public static Airliner GetAirliner(string tailnumber)
        {
            return airliners.Find(delegate(Airliner airliner) { return airliner.TailNumber == tailnumber; });
        }

        //returns the list of airliners

        //returns the list of airliners for sale
        public static List<Airliner> GetAirlinersForSale()
        {
            return airliners.FindAll((delegate(Airliner airliner) { return airliner.Airline == null; }));
        }

        //returns the list of airliners for sale
        public static List<Airliner> GetAirlinersForSale(Predicate<Airliner> match)
        {
            return airliners.FindAll(a => a.Airline == null).FindAll(match);
        }

        public static List<Airliner> GetAllAirliners()
        {
            return airliners;
        }

        //removes an airliner from the list
        public static void RemoveAirliner(Airliner airliner)
        {
            airliners.Remove(airliner);
        }

        #endregion
    }
}