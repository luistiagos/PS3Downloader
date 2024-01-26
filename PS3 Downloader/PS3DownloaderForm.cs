using PS3_Downloader;

public partial class PS3DownloaderForm : Form
{
    private TextBox searchEntry;
    private ListView listView;
    private TableLayoutPanel tableLayoutPanel;
    private TableLayoutPanel tableLayoutPanelSearch;

    public PS3DownloaderForm()
    {
        InitializeComponents();
    }

    private async void InitializeComponents()
    {
        // Create the main form
        Text = "Playstation 3 Downloader";

        // Create a search box
        Label searchLabel = new Label
        {
            Text = "Nome:",
            AutoSize = true
        };

        searchEntry = new TextBox
        {
            Width = 350
        };

        Button searchButton = new Button
        {
            Text = "Procurar",
            Width = 80
        };
        searchButton.Click += OnSearchButtonClick;

        searchEntry.KeyDown += (sender, e) =>
        {
            if (e.KeyCode == Keys.Return)
            {
                OnSearchButtonClick(sender, e);
            }
        };

        // Create a ListView control in Details mode
        listView = new ListView
        {
            View = View.Details,
            Dock = DockStyle.None, // Set DockStyle to None
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom // Use Anchor property
        };

        // Add columns to the ListView
        listView.Columns.Add("Nome", 120);
        listView.Columns.Add("Localidade", 80);
        listView.Columns.Add("Tamanho", 80);
        listView.Columns.Add("ID", 120);

        // Bind the selection event
        listView.SelectedIndexChanged += OnListViewSelect;

        // Create a frame for the footer
        Label footerLabel = new Label
        {
            Text = "Digital Store Games - Playstation 3 Downloader",
            AutoSize = false,
            Width = 300
        };

        // Add controls to the form
        Controls.Add(searchLabel);
        Controls.Add(searchEntry);
        Controls.Add(searchButton);
        Controls.Add(listView);
        Controls.Add(footerLabel);

        // Set layout for the form
        tableLayoutPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill
        };

        tableLayoutPanelSearch = new TableLayoutPanel();
        tableLayoutPanelSearch.Dock = DockStyle.Fill;
        tableLayoutPanelSearch.ColumnCount = 3;
        tableLayoutPanelSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
        tableLayoutPanelSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
        tableLayoutPanelSearch.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));

        tableLayoutPanelSearch.Controls.Add(searchLabel, 0, 0);
        tableLayoutPanelSearch.Controls.Add(searchEntry, 1, 0);
        tableLayoutPanelSearch.Controls.Add(searchButton, 2, 0);


        //tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,20));
        //tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,60));
        //tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,20));

        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

        //tableLayoutPanel.Controls.Add(searchLabel, 0, 0);
        //tableLayoutPanel.Controls.Add(searchEntry, 1, 0);
        //tableLayoutPanel.Controls.Add(searchButton, 2, 0);
        tableLayoutPanel.Controls.Add(tableLayoutPanelSearch, 0, 0);
        tableLayoutPanel.Controls.Add(listView, 0, 1);
        //tableLayoutPanel.SetColumnSpan(listView, 3); // Set column span to 3
        tableLayoutPanel.Controls.Add(footerLabel, 0, 2);
        //tableLayoutPanel.SetColumnSpan(footerLabel, 3); // Set column span to 3

        Controls.Add(tableLayoutPanel);
        this.Size = new System.Drawing.Size(800, 600);

        // Set the Anchor property for the controls in the first row
        //searchLabel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        //searchEntry.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top; // AnchorStyles.Top is added to keep the control from stretching vertically
        //searchButton.Anchor = AnchorStyles.Left | AnchorStyles.Right;

        FillItems("");
    }

    private async void OnListViewSelect(object sender, EventArgs e)
    {
        // Handle ListView item selection
        if (listView.SelectedItems.Count > 0)
        {
            ListViewItem selectedItem = listView.SelectedItems[0];
            string[] values = new string[4];

            for (int i = 0; i < 4; i++)
            {
                values[i] = selectedItem.SubItems[i].Text;
            }

            string confirmationMessage = $"Gostaria de fazer o Download: {values[0]}";

            // Show confirmation message with OK button
            DialogResult result = MessageBox.Show(confirmationMessage, "Confirmation", MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                // Download the selected item
                await DownloadSelectedItem(values[3]);
                Console.WriteLine("\nDownload Completo");
            }
        }
    }

    private void OnSearchButtonClick(object sender, EventArgs e)
    {
        string searchQuery = searchEntry.Text.Trim().ToLower();
        FillItems(searchQuery);
    }

    private void FillItems(string searchQuery)
    {
        listView.Items.Clear();
        List<Dictionary<string, string>> items = NPS.LoadItems("ps3");

        foreach (var item in items)
        {
            if (searchQuery == null || searchQuery == "" || item["name"].ToLower().Contains(searchQuery))
            {
                ListViewItem listViewItem = new ListViewItem(new string[] { item["name"], item["region"], NPS.FormatSize(item["file_size"]), item["title_id"] });
                listView.Items.Add(listViewItem);
            }
        }

       
    }

    private async Task DownloadSelectedItem(string titleId)
    {
        await NPS.DownloadSelectedItem(titleId);
    }
    
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new PS3DownloaderForm());
    }
    
}
