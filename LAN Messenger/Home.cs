using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LAN_Messenger
{
    public partial class Home : Form
    {
        private const int SnapDistance = 10; // Pixels within which it will snap

        private ListBox listBoxUsers;
        private Button btnRefresh;
        private Label lblInfo;
        private UdpListener udpListener;
        private string myName = $"User-{Environment.MachineName}"; // change if you want custom


        public Home()
        {
            this.Text = "LAN Messenger - Online Users";
            this.Size = new Size(400, 500);
            InitializeComponents();

            this.Load += Home_Load;
            this.FormClosing += Home_FormClosing;
        }
        private void InitializeComponents()
        {
            listBoxUsers = new ListBox()
            {
                Dock = DockStyle.Fill
            };
            listBoxUsers.DoubleClick += listBox1_DoubleClick;

            btnRefresh = new Button()
            {
                Text = "Refresh (WHO_IS_ONLINE)",
                Dock = DockStyle.Top,
                Height = 35
            };
            btnRefresh.Click += BtnRefresh_Click;

            lblInfo = new Label()
            {
                Text = "Double-click a user to open chat window.",
                Dock = DockStyle.Top,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft
            };

            this.Controls.Add(listBoxUsers);
            this.Controls.Add(btnRefresh);
            this.Controls.Add(lblInfo);
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



            // create listener and subscribe
            udpListener = new UdpListener(8888);
            udpListener.OnUserDetected += (userInfo) =>
            {
                // UI update must be on UI thread
                if (this.IsHandleCreated)
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        if (!listBoxUsers.Items.Contains(userInfo))
                            listBoxUsers.Items.Add(userInfo);
                    });
                }
            };

            // Start listening (binds to port 8888). Pass my name.
            udpListener.StartListening(myName);

            // Announce ourselves, then request current online users
            udpListener.AnnounceOnline(); // tell others we're online
            System.Threading.Thread.Sleep(100); // tiny delay so announcer packet goes out
            udpListener.RequestOnline();     // ask who is online (they will reply)
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

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBoxUsers.SelectedItem == null) return;
            string selected = listBoxUsers.SelectedItem.ToString();
            ChatWindow chat = new ChatWindow(selected);
            chat.Show();
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            // Ask again for current online users
            udpListener.RequestOnline();
        }

        private void Home_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Optionally broadcast offline - not implemented here
            udpListener?.Dispose();
        }
    }
}
