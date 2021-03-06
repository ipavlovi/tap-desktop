﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.GraphicsModel.UserControlModel.MessageBoxModel;
using TheAirline.GraphicsModel.UserControlModel.PopUpWindowsModel;
using TheAirline.Model.AirlinerModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.CountryModel.TownModel;
using TheAirline.Model.GeneralModel.Helpers;
using TheAirline.Model.PilotModel;

namespace TheAirline.GUIModel.PagesModel.PilotsPageModel
{
    /// <summary>
    /// Interaction logic for PageShowFlightSchool.xaml
    /// </summary>
    public partial class PageShowFlightSchool : Page
    {
        public FlightSchoolMVVM FlightSchool { get; set; }
        public List<string> AirlinerFamilies { get; set; }
        public ObservableCollection<Instructor> Instructors { get; set; }
        private Random rnd = new Random();

        public PageShowFlightSchool(FlightSchool fs)
        {
            this.FlightSchool = new FlightSchoolMVVM(fs);
            this.Instructors = new ObservableCollection<Instructor>();
            this.AirlinerFamilies = AirlinerTypes.GetTypes(t => t.Produced.From.Year <= GameObject.GetInstance().GameTime.Year && t.Produced.To > GameObject.GetInstance().GameTime.AddYears(-30)).Select(t => t.AirlinerFamily).Distinct().OrderBy(a => a).ToList();
            
            this.DataContext = this.FlightSchool;

            setHireStudentsStatus();

            InitializeComponent();

        }
        private void btnSellAircraft_Click(object sender, RoutedEventArgs e)
        {
            TrainingAircraft aircraft = (TrainingAircraft)((Button)sender).Tag;
            
            var aircrafts = new List<TrainingAircraft>(this.FlightSchool.Aircrafts);
            aircrafts.Remove(aircraft);

            
            Dictionary<TrainingAircraftType,int> types = this.FlightSchool.Aircrafts.GroupBy(a=>a.Type).
                     Select(group =>
                         new
                         {
                             Type = group.Key,
                             Count = group.Sum(g=>g.Type.MaxNumberOfStudents)
                         }).ToDictionary(g => g.Type, g => g.Count); ;


            foreach (PilotStudent student in this.FlightSchool.Students)
            {
                var firstAircraft = student.Rating.Aircrafts.OrderBy(a=>a.TypeLevel).First(a=>types.ContainsKey(a) && types[a] > 0);

                if (types.ContainsKey(firstAircraft))
                    types[firstAircraft]--;

            }

            Boolean canSellAircraft = aircrafts.Sum(a => a.Type.MaxNumberOfStudents) >= this.FlightSchool.Students.Count && types[aircraft.Type]>1;

            if (canSellAircraft)
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2809"), Translator.GetInstance().GetString("MessageBox", "2809", "message"), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.FlightSchool.removeTrainingAircraft(aircraft);
                    
                    double price = aircraft.Type.Price * 0.75;
                    AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline,GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, price);
                }
            }
            else
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2810"), Translator.GetInstance().GetString("MessageBox", "2810", "message"), WPFMessageBoxButtons.Ok);
        }
        private void btnHire_Click(object sender, RoutedEventArgs e)
        {
          
            var aircraftsTypesFree = this.FlightSchool.Aircrafts.Select(a => a.Type);

            Dictionary<TrainingAircraftType,int> types = this.FlightSchool.Aircrafts.GroupBy(a=>a.Type).
                     Select(group =>
                         new
                         {
                             Type = group.Key,
                             Count = group.Sum(g=>g.Type.MaxNumberOfStudents)
                         }).ToDictionary(g => g.Type, g => g.Count); ;


            foreach (PilotStudent student in this.FlightSchool.Students)
            {
                var firstAircraft = student.Rating.Aircrafts.OrderBy(a=>a.TypeLevel).FirstOrDefault(a=>types.ContainsKey(a) && types[a] > 0);

                if (firstAircraft != null && types.ContainsKey(firstAircraft))
                    types[firstAircraft]--;

            }

            List<PilotRating> possibleRatings = new List<PilotRating>();
            
            foreach (PilotRating rating in PilotRatings.GetRatings())
            {
                if (rating.Aircrafts.Exists(a => types.ContainsKey(a) && types[a] > 0))
                    possibleRatings.Add(rating);
            }

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2811"), string.Format(Translator.GetInstance().GetString("MessageBox", "2811", "message")), WPFMessageBoxButtons.YesNo);
            
            if (result == WPFMessageBoxResult.Yes)
            {
              
                List<Town> towns = Towns.GetTowns(this.FlightSchool.FlightSchool.Airport.Profile.Country);

                Town town = towns[rnd.Next(towns.Count)];
                DateTime birthdate = MathHelpers.GetRandomDate(GameObject.GetInstance().GameTime.AddYears(-35), GameObject.GetInstance().GameTime.AddYears(-23));
                PilotProfile profile = new PilotProfile(Names.GetInstance().getRandomFirstName(town.Country), Names.GetInstance().getRandomLastName(town.Country), birthdate, town);

                Instructor instructor = (Instructor)cbInstructor.SelectedItem;
                string airlinerFamily = cbTrainAircraft.SelectedItem.ToString();

                PilotStudent student = new PilotStudent(profile, GameObject.GetInstance().GameTime,instructor ,GeneralHelpers.GetPilotStudentRating(instructor,possibleRatings),airlinerFamily);

                TrainingAircraft aircraft = getStudentAircraft(student);

                student.Aircraft = aircraft;

                this.FlightSchool.addStudent(student);
                instructor.addStudent(student);

                setHireStudentsStatus();

                double studentPrice = GeneralHelpers.GetInflationPrice(PilotStudent.StudentCost);

                AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -studentPrice);
            }

        }
        private void btnDeleteInstructor_Click(object sender, RoutedEventArgs e)
        {
            Instructor instructor = (Instructor)((Button)sender).Tag;

            if (instructor.Students.Count > 0)
                WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2805"), string.Format(Translator.GetInstance().GetString("MessageBox", "2805", "message"), instructor.Profile.Name), WPFMessageBoxButtons.Ok);
            else
            {
                WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2806"), string.Format(Translator.GetInstance().GetString("MessageBox", "2806", "message"), instructor.Profile.Name), WPFMessageBoxButtons.YesNo);

                if (result == WPFMessageBoxResult.Yes)
                {
                    this.FlightSchool.removeInstructor(instructor);

                    instructor.FlightSchool = null;
                
                }
            }
        }
        private void btnBuyAircraft_Click(object sender, RoutedEventArgs e)
        {
            
            ComboBox cbAircraft = new ComboBox();
            cbAircraft.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbAircraft.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbAircraft.ItemTemplate = this.Resources["TrainingAircraftTypeItem"] as DataTemplate;
            cbAircraft.Width = 300;

            foreach (TrainingAircraftType type in TrainingAircraftTypes.GetAircraftTypes().FindAll(t => GeneralHelpers.GetInflationPrice(t.Price) < GameObject.GetInstance().HumanAirline.Money))
                cbAircraft.Items.Add(type);

            cbAircraft.SelectedIndex = 0;
            
            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PageShowFlightSchool", "1014"), cbAircraft) == PopUpSingleElement.ButtonSelected.OK && cbAircraft.SelectedItem != null)
            {

                TrainingAircraftType aircraft = (TrainingAircraftType)cbAircraft.SelectedItem;
                double price = aircraft.Price;

                this.FlightSchool.addTrainingAircraft(new TrainingAircraft(aircraft, GameObject.GetInstance().GameTime, this.FlightSchool.FlightSchool));

                AirlineHelpers.AddAirlineInvoice(GameObject.GetInstance().HumanAirline, GameObject.GetInstance().GameTime, Invoice.InvoiceType.Airline_Expenses, -price);

                setHireStudentsStatus();

            }
        }
        private void btnChangeInstructor_Click(object sender, RoutedEventArgs e)
        {
            PilotStudent student = (PilotStudent)((Hyperlink)sender).Tag;

            ComboBox cbInstructor = new ComboBox();
            cbInstructor.SetResourceReference(ComboBox.StyleProperty, "ComboBoxTransparentStyle");
            cbInstructor.Width = 200;
            cbInstructor.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            cbInstructor.DisplayMemberPath = "Profile.Name";
            cbInstructor.SelectedValuePath = "Profile.Name";

            foreach (Instructor instructor in this.FlightSchool.Instructors.Where(i => i.Students.Count <  Model.PilotModel.FlightSchool.MaxNumberOfStudentsPerInstructor && i != student.Instructor))
                cbInstructor.Items.Add(instructor);

            cbInstructor.SelectedIndex = 0;

            if (PopUpSingleElement.ShowPopUp(Translator.GetInstance().GetString("PanelFlightSchool", "1005"), cbInstructor) == PopUpSingleElement.ButtonSelected.OK && cbInstructor.SelectedItem != null)
            {
                student.Instructor.removeStudent(student);
                student.Instructor = (Instructor)cbInstructor.SelectedItem;

                ICollectionView view = CollectionViewSource.GetDefaultView(lvStudents.ItemsSource);
                view.Refresh();
            }
        }
        private void btnDeleteStudent_Click(object sender, RoutedEventArgs e)
        {
            PilotStudent student = (PilotStudent)((Button)sender).Tag;

            WPFMessageBoxResult result = WPFMessageBox.Show(Translator.GetInstance().GetString("MessageBox", "2807"), string.Format(Translator.GetInstance().GetString("MessageBox", "2807", "message"), student.Profile.Name), WPFMessageBoxButtons.YesNo);

            if (result == WPFMessageBoxResult.Yes)
            {
                this.FlightSchool.removeStudent(student);
                student.Instructor.removeStudent(student);
                student.Instructor = null;

                setHireStudentsStatus();
                
            }
        }
        //sets the status for hiring of students
        private void setHireStudentsStatus()
        {
            double studentPrice = GeneralHelpers.GetInflationPrice(PilotStudent.StudentCost);

            int studentsCapacity = Math.Min(this.FlightSchool.Instructors.Count * Model.PilotModel.FlightSchool.MaxNumberOfStudentsPerInstructor, this.FlightSchool.FlightSchool.TrainingAircrafts.Sum(f => f.Type.MaxNumberOfStudents));

            this.FlightSchool.HireStudents = studentsCapacity > this.FlightSchool.Students.Count && GameObject.GetInstance().HumanAirline.Money > studentPrice;

            this.Instructors.Clear();

            foreach (Instructor instructor in this.FlightSchool.Instructors.Where(i => i.Students.Count < Model.PilotModel.FlightSchool.MaxNumberOfStudentsPerInstructor))
                this.Instructors.Add(instructor);
        }
        //returns the aircraft for a student
        private TrainingAircraft getStudentAircraft(PilotStudent student)
        {
            Dictionary<TrainingAircraftType, int> types = this.FlightSchool.Aircrafts.GroupBy(a => a.Type).
                     Select(group =>
                         new
                         {
                             Type = group.Key,
                             Count = group.Sum(g => g.Type.MaxNumberOfStudents)
                         }).ToDictionary(g => g.Type, g => g.Count); ;


            foreach (PilotStudent ps in this.FlightSchool.Students)
            {
                types[ps.Aircraft.Type]--;

            }

            var freeTypes = types.Where(t => t.Value > 0).Select(t => t.Key).OrderBy(t=>t.TypeLevel);

            return this.FlightSchool.Aircrafts.First(a=>a.Type ==  freeTypes.First());
        }
       
    }
}
