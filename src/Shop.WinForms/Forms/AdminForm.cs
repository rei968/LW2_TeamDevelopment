using Shop.Shared.Models;
using Shop.Shared.Services;

namespace Shop.WinForms.Forms;

public class AdminForm : Form
{
    private DataGridView _gridStock = null!;
    private DataGridView _gridOrders = null!;
    private Button _btnIncreaseStock = null!;
    private Button _btnDecreaseStock = null!;
    private Button _btnRefresh = null!;

    public AdminForm()
    {
        InitializeComponent();
        RefreshData();
    }

    private void InitializeComponent()
    {
        Text = "Панель адміністратора (Розробник: Ярік)";
        Size = new Size(800, 500);
        StartPosition = FormStartPosition.CenterScreen;

        var lblStock = new Label
        {
            Text = "Склад товарів:",
            Location = new Point(20, 15),
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold)
        };

        _gridStock = new DataGridView
        {
            Location = new Point(20, 45),
            Size = new Size(740, 180),
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        _btnIncreaseStock = new Button
        {
            Text = "➕ Додати +5 шт.",
            Location = new Point(20, 235),
            Size = new Size(140, 32)
        };
        _btnIncreaseStock.Click += (s, e) =>
        {
            // TODO (Ярік): збільшити залишок обраного товару
            if (_gridStock.CurrentRow?.DataBoundItem is Product p)
            {
                p.StockQuantity += 5;
                RefreshData();
            }
        };

        _btnDecreaseStock = new Button
        {
            Text = "➖ Списати -1 шт.",
            Location = new Point(170, 235),
            Size = new Size(140, 32)
        };
        _btnDecreaseStock.Click += (s, e) =>
        {
            // TODO (Ярік): списати залишок обраного товару
            if (_gridStock.CurrentRow?.DataBoundItem is Product p && p.StockQuantity > 0)
            {
                p.StockQuantity -= 1;
                RefreshData();
            }
        };

        var lblOrders = new Label
        {
            Text = "Замовлення клієнтів:",
            Location = new Point(20, 280),
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold)
        };

        _gridOrders = new DataGridView
        {
            Location = new Point(20, 310),
            Size = new Size(740, 130),
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        Controls.Add(lblStock);
        Controls.Add(_gridStock);
        Controls.Add(_btnIncreaseStock);
        Controls.Add(_btnDecreaseStock);
        Controls.Add(lblOrders);
        Controls.Add(_gridOrders);
    }

    private void RefreshData()
    {
        _gridStock.DataSource = null;
        _gridStock.DataSource = ShopService.Instance.Products.ToList();

        _gridOrders.DataSource = null;
        _gridOrders.DataSource = ShopService.Instance.Orders.Select(o => new
        {
            Номер = o.Id,
            Клієнт = o.CustomerName,
            Телефон = o.CustomerPhone,
            Сума = $"{o.TotalAmount:N2} грн",
            Статус = o.StatusDisplay
        }).ToList();
    }
}
