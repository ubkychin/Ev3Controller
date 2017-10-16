using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Lego.Ev3.Core;
using SharpDX.XInput;

namespace Game
{
    public class BrickManager
    {
        public static BrickControl[] Bricks { get; set; }
        private static int _currentBrickIndex = 0;

        public static void Init()
        {
            Bricks = new BrickControl[4];
        }


        public static int AddBrickAndControler(BrickControl brickControl)
        {
            if (_currentBrickIndex > 3) throw new OverflowException("To Many Bricks, Only 4 Allowed");
            if (brickControl == null) throw new ArgumentException(nameof(brickControl));
            brickControl.Index = _currentBrickIndex;
            Bricks[_currentBrickIndex] = brickControl;
            _currentBrickIndex++;

            return _currentBrickIndex - 1;
        }

        public static async void ConnectToAllAsync()
        {
            foreach (var control in Bricks.Where(control => control != null))
            {
                await control.Brick.ConnectAsync();
            }
        }
    }

    public class BrickControl
    {
        public Brick Brick { get; set; }
        public Controller Controller { get; set; }
        public ColorSensorColor[] AimedColor { get; set; }
        public int Index { get; set; }
        public BrickControl(Brick brick, Controller controller)
        {
            Brick = brick;
            Controller = controller;
        }

    }
}