namespace Sevent
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            this.WindowState = FormWindowState.Maximized;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
        }
    }
}
