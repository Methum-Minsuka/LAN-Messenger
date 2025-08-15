using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LAN_Messenger
{
    public partial class Home : Form
    {
        private const int SnapDistance = 10; // Pixels within which it will snap
        public Home()
        {
            InitializeComponent();
        }

        private void Home_Load(object sender, EventArgs e)
        {
            // First open at Left-Bottom corner
            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;
            int x = 0;
            int y = screenHeight - this.Height;

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(x, y);
        }
        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);

            // Screen working area
            Rectangle screen = Screen.FromControl(this).WorkingArea;
            Point newLocation = this.Location;

            // Snap Left
            if (Math.Abs(this.Left - screen.Left) <= SnapDistance)
            {
                newLocation.X = screen.Left;
            }

            // Snap Right
            if (Math.Abs((screen.Right) - (this.Right)) <= SnapDistance)
            {
                newLocation.X = screen.Right - this.Width;
            }

            // Snap Top
            if (Math.Abs(this.Top - screen.Top) <= SnapDistance)
            {
                newLocation.Y = screen.Top;
            }

            // Snap Bottom
            if (Math.Abs((screen.Bottom) - (this.Bottom)) <= SnapDistance)
            {
                newLocation.Y = screen.Bottom - this.Height;
            }

            // Apply snapped location
            if (newLocation != this.Location)
            {
                this.Location = newLocation;
            }
        }
    }
}
