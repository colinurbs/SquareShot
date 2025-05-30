namespace SquareShot;

using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

public partial class Form1 : Form
{
    private bool isCapturing;
    private Form overlayForm;
    private Bitmap screenBitmap;
    private string saveDirectory;
    private int screenshotCounter = 0;
    private string selectedDirectory;
    private Bitmap lastScreenshot;
    private int height = 512;
    private int width = 512;
    private string[] filetypes = ["jpg, png"];


    public Form1()
    {
        InitializeComponent();
        numericUpDown1.Minimum = 256;
        numericUpDown1.Maximum = 2048;
        numericUpDown1.Increment = 1;
        numericUpDown1.Value = height;
        numericUpDown2.Minimum = 256;
        numericUpDown2.Maximum = 2048;
        numericUpDown2.Increment = 1;
        numericUpDown2.Value = width;
    }

    private void button1_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(selectedDirectory))
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    selectedDirectory = fbd.SelectedPath;
                }
                else
                {
                    return; // Exit if user cancels dialog
                }
            }
        }

        isCapturing = true;
        CreateOverlay();
    }

    private void CreateOverlay()
    {
        overlayForm = new Form
        {
            FormBorderStyle = FormBorderStyle.None,
            Bounds = Screen.PrimaryScreen.Bounds,
            TopMost = true,
            BackColor = Color.Gray,
            Opacity = 0.02,
            Cursor = Cursors.Cross,
            ShowInTaskbar = false
        };

        overlayForm.Paint += (s, e) =>
        {
            Point cursorPos = overlayForm.PointToClient(Cursor.Position);
            using (Pen pen = new Pen(Color.White, 3))
            {
                e.Graphics.DrawRectangle(pen,
                    cursorPos.X - (width / 2),
                    cursorPos.Y - (height / 2),
                    width, height);
            }
        };

        overlayForm.MouseMove += (s, e) => overlayForm.Invalidate();

        overlayForm.MouseClick += (s, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                Point screenPos = Cursor.Position;
                Rectangle captureArea = new Rectangle(
                    screenPos.X - (width / 2),
                    screenPos.Y - (height / 2),
                    width, height);

                using (Bitmap bmp = new Bitmap(width, height))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.CopyFromScreen(captureArea.Location, Point.Empty, captureArea.Size);

                    // Store copy and update picture box
                    lastScreenshot?.Dispose();
                    lastScreenshot = new Bitmap(bmp);

                    // Save to file
                    if (selectedDirectory != null)
                    {
                        string fileName = Path.Combine(selectedDirectory, $"{Guid.NewGuid()}.png");

                        bmp.Save(fileName, ImageFormat.Png);
                    }
                    // Update UI thread safely
                    if (pictureBox1.InvokeRequired)
                    {
                        pictureBox1.Invoke((MethodInvoker)(() =>
                        {
                            pictureBox1.Image = lastScreenshot;
                            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                        }));
                    }
                    else
                    {
                        pictureBox1.Image = lastScreenshot;
                        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    }
                }

                overlayForm.Close();
                isCapturing = false;
            }
        };

        overlayForm.Show();
    }

    private void numericUpDown1_ValueChanged(object sender, EventArgs e)
    {
        width = (int)numericUpDown1.Value;
    }

    private void numericUpDown2_ValueChanged(object sender, EventArgs e)
    {
        height = (int)numericUpDown2.Value;
    }

}