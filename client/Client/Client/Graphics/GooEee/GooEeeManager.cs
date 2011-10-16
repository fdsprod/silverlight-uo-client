using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Client.Input;

namespace Client.Graphics.GooEee
{
    public sealed class GooEeeManager
    {
        public GooEeeManager(Engine engine)
        {
            IInputService inputService = engine.Services.GetService<IInputService>();

            inputService.KeyDown += OnKeyDown;
            inputService.MouseLeftDown += OnMouseLeftDown;
            inputService.MouseLeftUp += OnMouseLeftUp;
            inputService.MouseMove += OnMouseMove;
        }

        private void OnMouseLeftUp(object sender, MouseStateEventArgs e)
        {

        }

        private void OnMouseLeftDown(object sender, MouseStateEventArgs e)
        {

        }

        private void OnMouseMove(object sender, MouseStateEventArgs e)
        {

        }

        private void OnKeyDown(object sender, KeyStateEventArgs e)
        {

        }        
    }
}
