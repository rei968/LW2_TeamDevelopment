namespace Shop.WinForms.Forms;

public class MainForm : Form
{
    public MainForm()
    {
        Text = "Інтернет-магазин (Головне меню)";
        Size = new Size(420, 260);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var lblTitle = new Label
        {
            Text = "🛒 Платформа E-Commerce",
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 50,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var btnCustomer = new Button
        {
            Text = "🛍️ Вітрина покупця (Льоша)",
            Location = new Point(50, 70),
            Size = new Size(300, 45),
            Font = new Font("Segoe UI", 10.5f)
        };
        btnCustomer.Click += (s, e) => new CustomerForm().Show();

        var btnAdmin = new Button
        {
            Text = "⚙️ Панель адміністратора (Ярік)",
            Location = new Point(50, 130),
            Size = new Size(300, 45),
            Font = new Font("Segoe UI", 10.5f)
        };
        btnAdmin.Click += (s, e) => new AdminForm().Show();

        Controls.Add(btnCustomer);
        Controls.Add(btnAdmin);
        Controls.Add(lblTitle);
    }
}
