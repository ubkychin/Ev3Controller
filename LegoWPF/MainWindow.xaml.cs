using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
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
using Lego.Ev3;
using Lego.Ev3.Core;
using Lego.Ev3.Desktop;

namespace LegoWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public BrickController Brick { get; set; }
        private Brick Brick_BT { get; set; }
        private string radioCheckedVal = "A";
        private string comRadioCheckVal = "COM1";
        public MainWindow()
        {
            InitializeComponent();
            Brick = new BrickController();

            Lbl1.Content = "Hi";
            //Debug.WriteLine("Constructor " + Brick.Ev3Brick.Ports[InputPort.One].RawValue);

            //Lbl1.Content = Brick.Ev3Brick.Ports[InputPort.One].RawValue == 0 ? "No Brick" : "Brick Connected";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //Ev3Brick.Disconnect();
            //Debug.WriteLine("Before "  + Brick.Ev3Brick.Ports.Keys.ToString());

            Brick.Ev3Brick.BrickChanged += OnBrickChanged;
            Lbl1.Content = Brick.Connect();

            Debug.WriteLine(Brick.Ev3Brick.Ports[InputPort.One].RawValue);
            //Debug.WriteLine("After "  + Brick.Ev3Brick.Ports.Keys.ToString());

        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //Brick.Ev3Brick.BrickChanged += OnBrickChanged;
            //Lbl1.Content = Brick.Connect_BT();



            Brick_BT = Brick.Connect(comRadioCheckVal);
            Brick_BT.BrickChanged += OnBrickChanged;
            Debug.WriteLine(Brick_BT.Ports[InputPort.One].RawValue);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Brick.Move(radioCheckedVal, 100);

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Brick.Move(radioCheckedVal, -100);

        }

        void OnBrickChanged(object sender, BrickChangedEventArgs e)
        {
            Debug.WriteLine("MainWindowcs:  change");
            try
            {
                LblSensor1.Content = e.Ports[InputPort.One].Type;
                LblValue1.Content = e.Ports[InputPort.One].SIValue;

                LblSensor2.Content = e.Ports[InputPort.Two].Type;
                LblValue2.Content = e.Ports[InputPort.Two].SIValue;

                LblSensor3.Content = e.Ports[InputPort.Three].Type;
                LblValue3.Content = e.Ports[InputPort.Three].SIValue;

                LblSensor4.Content = e.Ports[InputPort.Four].Type;
                LblValue4.Content = e.Ports[InputPort.Four].SIValue;
            }
            catch (System.Exception ex)
            {
                Lbl1.Content = "Brick Disconnected";
            }

            

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton button = sender as RadioButton;
            Debug.WriteLine(((RadioButton)button).GroupName);

            if (button.GroupName == "ComPortName")
            {
                comRadioCheckVal = button.Content.ToString();
            } else if (button.GroupName == "PortName")
            {
                radioCheckedVal = button.Content.ToString();
            }
        }


    }


}
