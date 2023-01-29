using EasyModbus;
using System;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ARMA_MODBUS_HMI.Properties;
using ScottPlot;
using System.Linq;

namespace ARMA_MODBUS_HMI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ModbusClient modbusClient;
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
        int[] readHoldingRegisters;
        bool alarm = false;
        int sp_value = 0;

        double[] level_trand = new double[190];
        double[] rpm_trand = new double[190];

        public MainWindow()
        {
            InitializeComponent();
            lbl_unloading.Visibility = Visibility.Collapsed;
            WpfPlot1.Configuration.LockHorizontalAxis = true;
            WpfPlot1.Configuration.LockVerticalAxis = true;
            WpfPlot1.Plot.Style(System.Drawing.Color.Transparent);
            WpfPlot1.BorderBrush = new SolidColorBrush(Colors.Black);
            WpfPlot1.Plot.SetAxisLimits(yMin: 0, yMax: 100, xMin:0, xMax: 100);
            
            WpfPlot1.Plot.Title("");
            WpfPlot1.Plot.XLabel(null);
            WpfPlot1.Plot.YLabel(null);
            WpfPlot1.Plot.Legend(true, location: ScottPlot.Alignment.UpperLeft);
            WpfPlot1.Plot.Frameless(true);
            WpfPlot1.Plot.Style(ScottPlot.Style.Seaborn);
  


            // plt.Grid(false);

            var signalLevel =  WpfPlot1.Plot.AddSignal(level_trand, sampleRate: 2,  label: "LVL", color: System.Drawing.Color.Green);
            // WpfPlot1.Plot.AddSignal(level_trand, label: "LVL",sampleRate: 5,color: System.Drawing.Color.Green);
            signalLevel.Label = "LVL";
            signalLevel.Color = System.Drawing.Color.Green;
            signalLevel.MarkerSize = 4;
            signalLevel.LineWidth = 2;

            var signalRPM = WpfPlot1.Plot.AddSignal(rpm_trand, sampleRate: 2, label: "RPM", color: System.Drawing.Color.Black);
            // WpfPlot1.Plot.AddSignal(rpm_trand,label:"RPM",  sampleRate: 5, color: System.Drawing.Color.Black);
            signalRPM.Label = "RPM";
            signalRPM.Color = System.Drawing.Color.Black;
            signalRPM.MarkerSize = 4;
            signalRPM.LineWidth = 2;
            WpfPlot1.Refresh();


        }


        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            txb_ip.Text = Properties.Settings.Default.IP;
            modbusClient = new ModbusClient(txb_ip.Text, 502);
            modbusClient.ConnectionTimeout = 1000;

            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
           // connect();
        }

        private void btn_connect_Click(object sender, RoutedEventArgs e)
        {
            connect();
            Properties.Settings.Default.IP = txb_ip.Text;
        }

        private void btn_disconnect_Click(object sender, RoutedEventArgs e)
        {
            dispatcherTimer.Stop();
            try
            {
                modbusClient.Disconnect();
                lbl_level_PV.Content = "###";
                lbl_valve.Content = "###";
                lbl_RPM.Content = "###";
                lbl_flow.Content = "###";
                txb_SetPoint.Content = "###";
                pb_level.Value = 0;
            }
            catch
            {

            }
        }


        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            dispatcherTimer.Stop();
            //bool status = modbusClient.Connected;
            

            if (modbusClient.Connected)
            {
             
                try
                {
                    readHoldingRegisters = modbusClient.ReadHoldingRegisters(12294, 7);
                    lbl_level_PV.Content = readHoldingRegisters[1].ToString() + " %";              
                    lbl_valve.Content = readHoldingRegisters[4].ToString() + " %";
                    lbl_RPM.Content = readHoldingRegisters[5].ToString() + " RPM";
                    lbl_flow.Content = readHoldingRegisters[6].ToString() + " SCFM";
                    pb_level.Value = readHoldingRegisters[1];

                    sp_value = readHoldingRegisters[0];
                    txb_SetPoint.Content = sp_value.ToString();

                    if (readHoldingRegisters[1] >= 80)
                    {
                        lbl_unloading.Visibility = Visibility.Visible;
                        alarm = true;
                    }
                    else if (alarm == true && readHoldingRegisters[1] <= 79)
                    {
                        lbl_unloading.Visibility = Visibility.Collapsed;
                        alarm = false;
                    }

                     
                    



                    // trand
                    Array.Copy(level_trand, 1, level_trand, 0, level_trand.Length - 1);
                    Array.Copy(rpm_trand, 1, rpm_trand, 0, rpm_trand.Length - 1);
                    level_trand[level_trand.Length - 1] = readHoldingRegisters[1];
                    rpm_trand[rpm_trand.Length - 1] = readHoldingRegisters[5] * 100 / 1800;
                 //   WpfPlot1.Plot.SetAxisLimits(yMin: 0, yMax: level_trand.Max() * 1.2);
                  

                }
                catch
                {
                    lbl_level_PV.Content = "###";
                    lbl_valve.Content = "###";
                    lbl_RPM.Content = "###";
                    lbl_flow.Content = "###";

                    txb_SetPoint.Content = "###";
                    
                    pb_level.Value = 0;
                    dispatcherTimer.Stop();
                    modbusClient.Disconnect();
            }

            }

           else
            {
                lbl_level_PV.Content = "###";
                lbl_valve.Content = "###";
                lbl_RPM.Content = "###";
                lbl_flow.Content = "###";
                if (!txb_SetPoint.IsFocused)
                {
                    txb_SetPoint.Content = "###";
                }
                pb_level.Value = 0;
                dispatcherTimer.Stop();
                modbusClient.Disconnect();
            }
            WpfPlot1.Refresh();
            dispatcherTimer.Start();
        }



        private void connect() 
        {
            dispatcherTimer.Stop();
            try
            {
                modbusClient.Disconnect();
                modbusClient.IPAddress = txb_ip.Text;
                modbusClient.Connect();
            }
            catch 
            {
              
            }

            dispatcherTimer.Start();

        }



        private void txb_SetPoint_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsDigit(e.Text, 0)) e.Handled = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void up_setpoint(object sender, RoutedEventArgs e)
        {
            try
            {
           
                dispatcherTimer.Stop();
                if (sp_value < 80 )
                {
                    modbusClient.WriteSingleRegister(12294, sp_value + 5);

                }
            }
            catch
            {

            }

            dispatcherTimer.Start();
        }

        private void down_setpoint(object sender, RoutedEventArgs e)
        {
            try
            {
             
                dispatcherTimer.Stop();
                if (sp_value > 10)
                {
                    modbusClient.WriteSingleRegister(12294, sp_value - 5);

                }
            }
            catch
            {

            }

            dispatcherTimer.Start();
        }
    }
}
