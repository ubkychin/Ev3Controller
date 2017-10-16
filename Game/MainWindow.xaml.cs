using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Lego.Ev3.Core;
using Lego.Ev3.Desktop;
using SharpDX.XInput;
using Color = Lego.Ev3.Core.Color;

namespace Game
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer _timer;
        public MainWindow()
        {
            InitializeComponent();
            BrickManager.Init();

            var ports = SerialPort.GetPortNames();
            var portsList = new List<string>();
            portsList.Add("None");
            portsList.AddRange(ports);
            ports = portsList.ToArray();
            Brick1Comport.ItemsSource = ports;
            Brick2Comport.ItemsSource = ports;
            Brick3Comport.ItemsSource = ports;
            Brick4Comport.ItemsSource = ports;
            foreach (UserIndex value in Enum.GetValues(typeof(UserIndex)))
            {
                if(value == UserIndex.Any) continue;
                var controler = new Controller(value);
                if (!controler.IsConnected)
                {
                    HideRing(value);
                    continue;
                }
                BrickManager.AddBrickAndControler(new BrickControl(null, controler));
            }
            _timer= new Timer(75);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
            
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach (var brickControler in BrickManager.Bricks)
            {
                if(brickControler == null) continue;
                
                var controler = brickControler.Controller;
                var brick = brickControler.Brick;
                if (controler == null || brick == null) continue;
                UpdateBrickMovement(controler.GetState(), brick);

            }
        }

        private async void UpdateBrickMovement(State state, Brick brick)
        {
            int forw_mag = state.Gamepad.RightTrigger - state.Gamepad.LeftTrigger;
            int turn_mag = state.Gamepad.LeftThumbX;
            int trigger_vals = 255;
            int forw_steps = 5;
            int turn_vals = 32767;
            int turn_steps = 10;
            int mot_A_speed = 0, mot_B_speed = 0;
            int speed = forw_mag / (trigger_vals / forw_steps) * 100 / forw_steps;
            int turn_rate = Math.Abs(turn_mag) / (turn_vals / turn_steps) * 100 / turn_steps;

            if (forw_mag < 0)
            {
                turn_rate *= -1;
            }

            if (turn_mag == 0)
            {
                mot_A_speed = speed;
                mot_B_speed = mot_A_speed;
            }
            else if (turn_mag > 0)
            {
                mot_A_speed = speed;
                mot_B_speed = speed - (int)((double)speed / 100 * (double)turn_rate / 100 * speed);

            }
            else if (turn_mag < 0)
            {
                mot_B_speed = speed;
                mot_A_speed = speed - (int)((double)speed / 100 * (double)turn_rate / 100 * speed);

            }

            //Console.WriteLine(move_forward);

            if (Math.Abs(mot_A_speed) <= 100 && Math.Abs(mot_A_speed) >= 0 && Math.Abs(mot_B_speed) <= 100 && Math.Abs(mot_B_speed) >= 0)
            {
                //Console.WriteLine("MotA: " + mot_A_speed);
                //Console.WriteLine("MotB: " + mot_B_speed);
                brick.BatchCommand.TurnMotorAtPowerForTime(OutputPort.A, mot_A_speed, 100, false);
                brick.BatchCommand.TurnMotorAtPowerForTime(OutputPort.B, mot_B_speed, 100, false);
            }

            if (state.Gamepad.RightThumbX > 24000)
            {
                brick.BatchCommand.StepMotorAtSpeed(OutputPort.C, 20, 10, false);
            }   
            else if (state.Gamepad.RightThumbX < -24000)
            {
                brick.BatchCommand.StepMotorAtSpeed(OutputPort.C, -20, 10, false);
            }
            await brick.BatchCommand.SendCommandAsync();
        }

        private void HideRing(UserIndex value)
        {
            switch (value)
            {
                case UserIndex.One:
                    Brick1Ring.Visibility = Visibility.Hidden;
                    break;
                case UserIndex.Two:
                    Brick2Ring.Visibility = Visibility.Hidden;
                    break;
                case UserIndex.Three:
                    Brick3Ring.Visibility = Visibility.Hidden;
                    break;
                case UserIndex.Four:
                    Brick4Ring.Visibility = Visibility.Hidden;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async void BrickComport_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = (ComboBox) sender;
            var brickIndex = Convert.ToInt32(comboBox.Tag);
            if((string) comboBox.SelectedItem == "None") return;
            comboBox.IsEnabled = false;
            var connection = new BluetoothCommunication((string)comboBox.SelectedItem);
            var brick = new Brick(connection);
            try
            {
                await brick.ConnectAsync();
                brick.BrickChanged += Brick_BrickChanged;
                BrickManager.Bricks[brickIndex].Brick = brick;
                BrickManager.Bricks[brickIndex].AimedColor = GetColors(brickIndex).ToArray();
                await brick.DirectCommand.CleanUIAsync();
                await brick.DirectCommand.SelectFontAsync(FontType.Large);
                await brick.DirectCommand.DrawTextAsync(Color.Foreground, 0, 20, "Brick " + (brickIndex + 1));
                await brick.DirectCommand.DrawTextAsync(Color.Foreground, 0, 60, GetColors(brickIndex).First().ToString());
                await brick.DirectCommand.UpdateUIAsync();
                brick.Ports[InputPort.One].SetMode(ColorMode.Color);
                var image = GetImage(BrickManager.Bricks[brickIndex]);
                image.Effect = null;
                brick.BrickChanged += Brick_BrickChanged1;
                MessageBox.Show("Connection Successful!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connetion Failed!");
                comboBox.IsEnabled = true;
            }

        }

        private void Brick_BrickChanged1(object sender, BrickChangedEventArgs e)
        {
            if (BrickManager.Bricks.All(x => x.Brick != sender)) return;

            var brickControler = BrickManager.Bricks.First(x => x.Brick == sender);
            var data = (ColorSensorColor) e.Ports[InputPort.One].RawValue;
            Console.WriteLine(data);
            UpdateColor(brickControler, data);
            
        }

        private void UpdateColor(BrickControl brickControler, ColorSensorColor data)
        {
            Ellipse colorEllipse = GetEllipse(brickControler);
            if (brickControler.AimedColor.Any(x => x == data)) 
            {
                colorEllipse.Fill = colorEllipse.Stroke;
            }
            else
            {
                colorEllipse.Fill = Brushes.Transparent;
            }
        }

        private Ellipse GetEllipse(BrickControl brickControler)
        {
            switch (brickControler.Index)
            {
                case 0:
                    return Brick1Color;
                case 1:
                    return Brick2Color;
                case 2:
                    return Brick3Color;
                case 3:
                    return Brick4Color;
            }
            return null;
        }
        private Image GetImage(BrickControl brickControler)
        {
            switch (brickControler.Index)
            {
                case 0:
                    return Brick1Image;
                case 1:
                    return Brick2Image;
                case 2:
                    return Brick3Image;
                case 3:
                    return Brick4Image;
            }
            return null;
        }

        private void Brick_BrickChanged(object sender, BrickChangedEventArgs e)
        {
        }

        private IEnumerable<ColorSensorColor> GetColors(int brickIndex)
        {
            switch (brickIndex)
            {
                case 0:
                    yield return ColorSensorColor.Yellow;
                    break;
                case 1:
                    yield return ColorSensorColor.Red;
                    break;
                case 2:
                    yield return ColorSensorColor.Black;
                    yield return ColorSensorColor.Brown;
                    break;
                case 3:
                    yield return ColorSensorColor.Blue;
                    yield return ColorSensorColor.Green;
                    break;
            }
        }
    }
}
