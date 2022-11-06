using EasyModbus;
using System;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Data;
//using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ARMA_MODBUS_HMI.Properties;
//using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
//using System.Windows.Threading;
//using System.Reflection.Emit;

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

        
 

        public MainWindow()
        {
            InitializeComponent();

        }



        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            dispatcherTimer.Stop();

           

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

                    if (!txb_SetPoint.IsFocused)
                    {
                        txb_SetPoint.Text = readHoldingRegisters[0].ToString();
                    }
               
                }
                catch
                {

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
                    txb_SetPoint.Text = "###";
                }
                pb_level.Value = 0;
           
               // connect();
            }
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
            try
            {
                int sp_value = Int16.Parse(txb_SetPoint.Text);
                dispatcherTimer.Stop();
                if (modbusClient.Connected)
                {
                    if (sp_value < 80 && sp_value > 9)
                    {
                        modbusClient.WriteSingleRegister(12294, sp_value);

                    }
                }
            }
            catch
            {

            }

            dispatcherTimer.Start();
        }


        private void btn_change_ip_Click(object sender, RoutedEventArgs e)
        {
            connect();
            Properties.Settings.Default.IP = txb_ip.Text;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            txb_ip.Text = Properties.Settings.Default.IP;
            modbusClient = new ModbusClient(txb_ip.Text, 502);
            modbusClient.ConnectionTimeout = 200;

            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
            connect();
        }
    }
}
