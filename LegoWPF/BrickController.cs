using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using Lego.Ev3.Core;
using Lego.Ev3.Desktop;
using System.Windows.Controls;

namespace LegoWPF
{
    public class BrickController
    {
        public Brick Ev3Brick { get; set; }
        public Dictionary<string, int> PortNames { get; set; }

        public BrickController()
        {
            Ev3Brick = new Brick(new UsbCommunication());
            Ev3Brick.BrickChanged += OnBrickChanged;

            createPortNames();

        }

        private void createPortNames()
        {
            int count = 0;
            PortNames = new Dictionary<string, int>();

            foreach (Lego.Ev3.Core.InputPort port in Ev3Brick.Ports.Keys)
            {
                PortNames.Add(port.ToString(), count);
                count++;
            }
        }

        public Brick Connect()
        {
           
            try
            {
                string x = Task.Run(ConnectBrick).Result;
                return Ev3Brick;
//
//                var x = ConnectBrick();
//
//                return "Brick Connected";
            }
            catch (Exception ex)
            {
                
                Debug.WriteLine("Exception thrown-: " + ex);
                return null;
            }
            
        }

        public Brick Connect(string port)
        {

            try
            {
                Ev3Brick = new Brick(new BluetoothCommunication(port));
                
                string x = Task.Run(ConnectBrick).Result;
                return Ev3Brick;
               
            }
            catch (Exception ex)
            {

                Debug.WriteLine("Exception thrown-: " + ex);
                return null;
            }

        }


        public async Task<string> ConnectBrick()
        {
            try
            {
                await Ev3Brick.ConnectAsync();
                return "Brick Connected";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
            
        }

        void OnBrickChanged(object sender, BrickChangedEventArgs e)
        {

            // print out the value of the sensor on Port 1 (more on this later...)
            Debug.WriteLine("Controller changed: " + e.Ports[InputPort.One].SIValue);
            
        }

        public void Move(string port, int power)
        {
            MoveMotor(port, power);
            Debug.WriteLine(port);
        }

        public async void MoveMotor(string port, int power)
        {
            
            switch (port)
            {
                case "A":
                    await Ev3Brick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.A, power, 1000, false);
                    break;

                case "B":
                    await Ev3Brick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.B, power, 1000, false);
                    break;

                case "C":
                    await Ev3Brick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.C, power, 1000, false);
                    break;

                case "D":
                    await Ev3Brick.DirectCommand.TurnMotorAtPowerForTimeAsync(OutputPort.D, power, 1000, false);
                    break;
            }               
        }

        
    }
}
