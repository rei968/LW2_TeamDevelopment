using Shop.WinForms.Services;

namespace Shop.WinForms.Forms;

public class LoginForm : Form
{
    private TextBox _txtServerUrl = null!;
    private TextBox _txtEmail = null!;
    private TextBox _txtPassword = null!;
    private Button _btnLogin = null!;
    private Button _btnQuickAdmin = null!;
    private Button _btnQuickCustomer = null!;
    private Label _lblStatus = null!;

    public LoginForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Авторизація — E-Commerce Shop";
        Size = new Size(460, 480);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        BackColor = Color.FromArgb(245, 247, 250);
        Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

        // Header Panel
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 70,
            BackColor = Color.FromArgb(41, 128, 185)
        };
        var lblTitle = new Label
        {
            Text = "🛒 E-Commerce Platform",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            AutoSize = false,
            Dock = DockStyle.Top,
            Height = 35,
            TextAlign = ContentAlignment.MiddleCenter
        };
        var lblSubtitle = new Label
        {
            Text = "Вхід до системи (Адміністратор / Клієнт)",
            ForeColor = Color.FromArgb(235, 245, 255),
            Font = new Font("Segoe UI", 9.5f),
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter
        };
        pnlHeader.Controls.Add(lblSubtitle);
        pnlHeader.Controls.Add(lblTitle);

        // Server URL
        var lblServer = new Label
        {
            Text = "Web API URL:",
            Location = new Point(40, 90),
            AutoSize = true
        };
        _txtServerUrl = new TextBox
        {
            Text = ApiClient.Instance.BaseUrl,
            Location = new Point(40, 112),
            Size = new Size(365, 27)
        };

        // Email
        var lblEmail = new Label
        {
            Text = "Email адреса:",
            Location = new Point(40, 150),
            AutoSize = true
        };
        _txtEmail = new TextBox
        {
            Text = "admin@store.com",
            Location = new Point(40, 172),
            Size = new Size(365, 27)
        };

        // Password
        var lblPassword = new Label
        {
            Text = "Пароль:",
            Location = new Point(40, 210),
            AutoSize = true
        };
        _txtPassword = new TextBox
        {
            Text = "Admin123!",
            PasswordChar = '●',
            Location = new Point(40, 232),
            Size = new Size(365, 27)
        };

        // Login Button
        _btnLogin = new Button
        {
            Text = "Увійти в систему",
            Location = new Point(40, 275),
            Size = new Size(365, 38),
            BackColor = Color.FromArgb(41, 128, 185),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnLogin.FlatAppearance.BorderSize = 0;
        _btnLogin.Click += async (s, e) => await PerformLoginAsync();

        // Quick login buttons
        _btnQuickAdmin = new Button
        {
            Text = "⚡ Адмін (admin@shop.com)",
            Location = new Point(40, 325),
            Size = new Size(180, 32),
            BackColor = Color.FromArgb(230, 235, 245),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 8.5f),
            Cursor = Cursors.Hand
        };
        _btnQuickAdmin.Click += (s, e) =>
        {
            _txtEmail.Text = "admin@shop.com";
            _txtPassword.Text = "admin123";
        };

        _btnQuickCustomer = new Button
        {
            Text = "⚡ Клієнт (customer@shop.com)",
            Location = new Point(225, 325),
            Size = new Size(180, 32),
            BackColor = Color.FromArgb(230, 235, 245),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 8.5f),
            Cursor = Cursors.Hand
        };
        _btnQuickCustomer.Click += (s, e) =>
        {
            _txtEmail.Text = "customer@shop.com";
            _txtPassword.Text = "customer123";
        };

        // Status Label
        _lblStatus = new Label
        {
            Text = "",
            ForeColor = Color.DarkRed,
            Location = new Point(40, 370),
            Size = new Size(365, 50),
            TextAlign = ContentAlignment.TopCenter
        };

        Controls.Add(pnlHeader);
        Controls.Add(lblServer);
        Controls.Add(_txtServerUrl);
        Controls.Add(lblEmail);
        Controls.Add(_txtEmail);
        Controls.Add(lblPassword);
        Controls.Add(_txtPassword);
        Controls.Add(_btnLogin);
        Controls.Add(_btnQuickAdmin);
        Controls.Add(_btnQuickCustomer);
        Controls.Add(_lblStatus);
    }

    private async Task PerformLoginAsync()
    {
        _lblStatus.Text = "Авторизація...";
        _lblStatus.ForeColor = Color.DarkSlateBlue;
        _btnLogin.Enabled = false;

        ApiClient.Instance.SetBaseUrl(_txtServerUrl.Text);

        var (success, message) = await ApiClient.Instance.LoginAsync(_txtEmail.Text.Trim(), _txtPassword.Text);

        _btnLogin.Enabled = true;

        if (success)
        {
            _lblStatus.ForeColor = Color.DarkGreen;
            _lblStatus.Text = $"Успішно! Вітаємо, {ApiClient.Instance.CurrentFullName}";

            Hide();
            if (ApiClient.Instance.IsAdmin)
            {
                var adminForm = new AdminForm();
                adminForm.FormClosed += (s, args) => Close();
                adminForm.Show();
            }
            else
            {
                var customerForm = new CustomerForm();
                customerForm.FormClosed += (s, args) => Close();
                customerForm.Show();
            }
        }
        else
        {
            _lblStatus.ForeColor = Color.DarkRed;
            _lblStatus.Text = message;
        }
    }
}
