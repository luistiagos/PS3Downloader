using System;
using System.ComponentModel;
using System.Net;
using System.Windows.Forms;

public partial class ProgressForm : Form
{
    private ProgressBar progressBar;
    private Label progressLabel;
    private static DateTime startTime;

    public ProgressForm()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        // Create the main form
        Text = "Download Progress";
        Width = 300;
        Height = 100;

        // Create a ProgressBar
        progressBar = new ProgressBar
        {
            Dock = DockStyle.Top,
            Style = ProgressBarStyle.Continuous
        };

        // Create a Label for progress information
        progressLabel = new Label
        {
            Dock = DockStyle.Bottom,
            TextAlign = ContentAlignment.MiddleCenter
        };

        // Add controls to the form
        Controls.Add(progressBar);
        Controls.Add(progressLabel);
    }

    public void UpdateProgress(DownloadProgressChangedEventArgs e)
    {
        double mbytesRecived = CalculateDownloadMbytesRecived(e.BytesReceived);
        progressBar.Value = e.ProgressPercentage;
        progressLabel.Text = $"Downloading: {e.ProgressPercentage}% ({mbytesRecived} mbs)";
        Refresh();
    }

    private static double CalculateDownloadMbytesRecived(long bytesReceived)
    {
        // Calculate download speed in megabytes per second (MBps)
        const int bytesPerMegabyte = 1024 * 1024;
        double downloadSpeedMBps = ((bytesReceived) / bytesPerMegabyte);
        return downloadSpeedMBps;
    }
}
