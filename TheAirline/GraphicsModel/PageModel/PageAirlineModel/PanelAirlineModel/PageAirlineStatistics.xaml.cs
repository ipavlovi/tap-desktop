﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheAirline.Model.AirlineModel;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;
using TheAirline.GraphicsModel.PageModel.GeneralModel;

namespace TheAirline.GraphicsModel.PageModel.PageAirlineModel.PanelAirlineModel
{
    /// <summary>
    /// Interaction logic for PageAirlineStatistics.xaml
    /// </summary>
    public partial class PageAirlineStatistics : Page
    {
        private Airline Airline;
        private ListBox lbStats;
        public PageAirlineStatistics(Airline airline)
        {
            InitializeComponent();

            this.Airline = airline;

            StackPanel panelStatistics = new StackPanel();
            panelStatistics.Margin = new Thickness(0, 10, 50, 0);

            TextBlock txtHeader = new TextBlock();
            txtHeader.Uid = "1001";
            txtHeader.Margin = new Thickness(0, 0, 0, 0);
            txtHeader.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            txtHeader.SetResourceReference(TextBlock.BackgroundProperty, "HeaderBackgroundBrush2");
            txtHeader.FontWeight = FontWeights.Bold;
            txtHeader.Text = Translator.GetInstance().GetString("PageAirlineStatistics", txtHeader.Uid);

            panelStatistics.Children.Add(txtHeader);

          
            ContentControl ccHeader = new ContentControl();
            ccHeader.ContentTemplate = this.Resources["StatHeader"] as DataTemplate;
            ccHeader.Content = new KeyValuePair<int, int>(GameObject.GetInstance().GameTime.Year - 1, GameObject.GetInstance().GameTime.Year);

            panelStatistics.Children.Add(ccHeader);

            lbStats = new ListBox();
            lbStats.ItemContainerStyleSelector = new ListBoxItemStyleSelector();
            lbStats.ItemTemplate = this.Resources["StatItem"] as DataTemplate;

            panelStatistics.Children.Add(lbStats);

            showStats();

            //GameTimer.GetInstance().OnTimeChanged += new GameTimer.TimeChanged(PageAirlineStatistics_OnTimeChanged);

            //this.Unloaded += new RoutedEventHandler(PageAirlineStatistics_Unloaded);

            this.Content = panelStatistics;
        }

        private void PageAirlineStatistics_Unloaded(object sender, RoutedEventArgs e)
        {
            GameTimer.GetInstance().OnTimeChanged -= new GameTimer.TimeChanged(PageAirlineStatistics_OnTimeChanged);

        }
        //shows the stats
        private void showStats()
        {
            lbStats.Items.Clear();

            lbStats.Items.Add(new KeyValuePair<Airline, StatisticsType>(this.Airline, StatisticsTypes.GetStatisticsType("Passengers")));
            lbStats.Items.Add(new KeyValuePair<Airline, StatisticsType>(this.Airline, StatisticsTypes.GetStatisticsType("Passengers%")));
            lbStats.Items.Add(new KeyValuePair<Airline, StatisticsType>(this.Airline, StatisticsTypes.GetStatisticsType("Departures")));

        }
        private void PageAirlineStatistics_OnTimeChanged()
        {
            if (this.IsLoaded)
            {
                showStats();
                 
            }
        }
    }
    //the converter for a statistics type
    public class AirlineStatConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            KeyValuePair<Airline, StatisticsType> sa = (KeyValuePair<Airline, StatisticsType>)value;

            int year = Int16.Parse(parameter.ToString());

            if (year == 0 || year == -1)
            {
                int currentYear = GameObject.GetInstance().GameTime.Year + year;
                return string.Format("{0:0,0}", sa.Key.Statistics.getStatisticsValue(currentYear, sa.Value));
            }
            else
            {
                double currentYearValue = sa.Key.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year, sa.Value);
                double lastYearValue = sa.Key.Statistics.getStatisticsValue(GameObject.GetInstance().GameTime.Year - 1, sa.Value);

                if (year == 0)
                    return currentYearValue;
                else if (year == -1)
                    return lastYearValue;
                else
                {
                    if (lastYearValue == 0)
                        return "100.00 %";
                    double changePercent = System.Convert.ToDouble(currentYearValue - lastYearValue) / lastYearValue;

                    if (double.IsInfinity(changePercent))
                        return "100.00 %";
                    if (double.IsNaN(changePercent))
                        return "-";

                    return string.Format("{0:0.00} %", changePercent * 100);
                }

            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
