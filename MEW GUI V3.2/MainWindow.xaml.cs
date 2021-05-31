using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Diagnostics;

using Microsoft.Win32;

using System.IO.Ports;

using LiveCharts;
using LiveCharts.Wpf;

namespace MEW_GUI_V3._1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    static class Globals
    {
        public static int NudgeDist = 0;
    }

    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ApplyPressureValue_Click(object sender, RoutedEventArgs e)
        //Applies the value entered into the textbox field.
        {
            string PressureValue = EditPressureValue.Text;
            //IF to check Value is numeric and apply, else do not change value & output error.
            TargetPressure.Text = PressureValue;
            //Send G-CODE of applied value to Arduino. - Requires definition of Serial Port.
        }
        //LOOP FOR ALL OTHER PARAMETERS.

        private void NumericOnly(object sender, TextCompositionEventArgs e)
        //Used to define Value textboxes such that they only accept numeric inputs.
        {
            e.Handled = new Regex("[^0-9.]+").IsMatch(e.Text);
            //Add protection to only accept valid numbers (e.g. no more than 1 '.')
            //Limit number of decimal places?
        }

        public void HandleCheck(object sender, RoutedEventArgs e)
        //Manages the value of NudgeDist, controlling the distance moved during nudge operations.
        {
            //int NudgeDist = NudgeDist;
            if (Nudge100mm.IsChecked == true)
            {
                Globals.NudgeDist = 100;
                Nudge100mm.IsEnabled = false;
                Nudge10mm.IsEnabled = true;
                Nudge1mm.IsEnabled = true;
                Nudge100mm.IsChecked = false;
                Nudge10mm.IsChecked = false;
                Nudge1mm.IsChecked = false;
            }
            else if (Nudge10mm.IsChecked == true)
            {
                Globals.NudgeDist = 10;
                Nudge100mm.IsEnabled = true;
                Nudge10mm.IsEnabled = false;
                Nudge1mm.IsEnabled = true;
                Nudge100mm.IsChecked = false;
                Nudge10mm.IsChecked = false;
                Nudge1mm.IsChecked = false;
            }
            else if (Nudge1mm.IsChecked == true)
            {
                Globals.NudgeDist = 1;
                Nudge100mm.IsEnabled = true;
                Nudge10mm.IsEnabled = true;
                Nudge1mm.IsEnabled = false;
                Nudge100mm.IsChecked = false;
                Nudge10mm.IsChecked = false;
                Nudge1mm.IsChecked = false;
            }
        }


        SerialPort sp = new SerialPort();
        private void ConnectionsCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //Manages selection of Serial Port Connections, i.e. Printer connection.
        {
            var SelectedConnection = sender as ComboBox;
            string PortName = SelectedConnection.SelectedItem as string;
        }
        private void ConnectPort_Click(object sender, RoutedEventArgs e)
        //As above, manages Serial Port Connections, and allows for refresh of ports. - ADD Enable/Disable Controls function.
        {
            int PortNum = SerialPort.GetPortNames().Length;
            if (PortNum >= 1)
            {
                try
                {
                    String portName = PortConnectionsCombo.Text;
                    sp.PortName = portName;
                    sp.BaudRate = 250000;
                    sp.Open();
                    MessageBox.Show("Connected to Port " + portName);
                    OutputText.AppendText("Connected to Port " + portName + "\r");
                    OutputText.ScrollToEnd();
                }
                catch
                {
                    MessageBox.Show("Please select a valid port connection, or check serial connection and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    OutputText.AppendText("Failed to connect to a port. Check connection and try again." + "\r");
                    OutputText.ScrollToEnd();
                }
            }
            else
            {
                MessageBox.Show("No devices found. Please check connection and try again.");
            }
        }

        private void RefreshPorts_Click(object sender, RoutedEventArgs e)
        //Refreshes serial ports for connection.
        {
            sp.Close();
            PortConnectionsCombo.Text = "";
            PortConnectionsCombo.ItemsSource = SerialPort.GetPortNames();
        }

        public void StartPrintButton_Click(object sender, RoutedEventArgs e)
        {
            bool PrintRunning = true;
            //while (PrintRunning == true)
            //{
            //    string Line = sp.ReadLine();
            //    TestingTextbox.Text = TestingTextbox.Text + Line + "\r";
            //}
        }
        //PAUSE PRINT BUTTON


        private void StopPrintButton_Click(object sender, RoutedEventArgs e)
        //Runs Emergency stop procedure. - MORE TO ADD
        {
            sp.WriteLine("M112" + "\n");
            sp.WriteLine("G0 X0 Y0" + "\n");
            OutputText.AppendText("Emergency Stopping current procedure" +"\r");
            OutputText.ScrollToEnd();
        }

        private void LoadGCodeButton_Click(object sender, RoutedEventArgs e)
        //Loads G-Code Files, currently allowing for Text files, AND All files. - Causes issues in loading other filetypes.
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                FileNameText.Text = openFileDialog.FileName;
                string FileName = FileNameText.Text;
                OutputText.AppendText("Loaded file " + FileName + "\r");
                OutputText.ScrollToEnd();
            }

        }

        private void OpenGCodeButton_Click(object sender, RoutedEventArgs e)
        //Opens text editor to preview/edit the selected G-Code.
        {
            //IF for if file is a valid file type. - Can currently load other files as 
            string FileName = FileNameText.Text;
            if (FileName == "File Name")
                MessageBox.Show("Please select a file and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                Process.Start(FileName);
        }

        private void ZeroPosition_Click(object sender, RoutedEventArgs e)
        //Handles Zeroing of X, Y & Z axes in position controls.
        {
            FrameworkElement s = sender as FrameworkElement;
            string name = s.Name;
            string ZeroAxis = null;
            if (name == "ZeroXPosition")
            {
                ZeroAxis = "X";
            }
            else if (name == "ZeroYPosition")
            {
                ZeroAxis = "Y";
            }
            else if (name == "ZeroZPosition")
            {
                ZeroAxis = "Z";
            }
            else if (name == "ZeroRPosition")
            {
                //ZeroAxis = "R";
                //Special case for Rotational Axis. G-Code Requirements?
            }
            sp.WriteLine("G92 " + ZeroAxis + "0" + "\n");
            OutputText.AppendText("Zeroed " + ZeroAxis + "-Axis." + "\r");
            OutputText.ScrollToEnd();
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        //Handles Homing of X,Y & Z axes in Nudge Controls.
        {
            FrameworkElement s = sender as FrameworkElement;
            string name = s.Name;
            string HomeCode = null;
            string HomeOutput = null;
            if (name == "HomeXY")
            {
                HomeCode = "G0 X0 Y0";
                HomeOutput = "Homed X&Y Axes.";
            }
            else if (name == "HomeZ")
            {
                HomeCode = "G0 Z0";
                HomeOutput = "Homed Z Axis.";
            }
            sp.WriteLine(HomeCode);
            OutputText.AppendText(HomeOutput + "\r");
            OutputText.ScrollToEnd();
        }

        private void Nudge_Click(object sender, RoutedEventArgs e)
        //Controls Nudging operation for all servos. Sends relevant GCode for nudges in each direction.
        {
            FrameworkElement s = sender as FrameworkElement;
            string name = s.Name;
            string NudgeCode = "";
            //TestingTextbox.Text = name;
            if (name == "NudgeXDown")
            {
                NudgeCode = "G0 X-" + Globals.NudgeDist;
            }
            else if (name == "NudgeXUp")
            {
                NudgeCode = "G0 X" + Globals.NudgeDist;
            }
            else if (name == "NudgeYDown")
            {
                NudgeCode = "G0 Y-" + Globals.NudgeDist;
            }
            else if (name == "NudgeYUp")
            {
                NudgeCode = "G0 Y" + Globals.NudgeDist;
            }
            else if (name == "NudgeZDown")
            {
                NudgeCode = "G0 Z-" + Globals.NudgeDist;
            }
            else if (name == "NudgeZUp")
            {
                NudgeCode = "G0 Z" + Globals.NudgeDist;
            }
            sp.WriteLine(NudgeCode);
            TestingTextbox.Text = TestingTextbox.Text + "\r" + NudgeCode;
        }

        private void TestingButton_Click(object sender, RoutedEventArgs e)
        {
            sp.WriteLine("M155 S4" + "\n");
            //PortData.Text = sp.ReadLine() + "\r";
            PortData.AppendText(sp.ReadLine() + "\r");
            //port_DataReceived(0);
        }

        public void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int intBuffer;
            intBuffer = sp.BytesToRead;
            byte[] byteBuffer = new byte[intBuffer];
            int output = sp.Read(byteBuffer, 0, intBuffer);
            PortData.AppendText(output + "\r");
        }

        
    }
    
    }

