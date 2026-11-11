using Shop.Shared.Models;
using Shop.Shared.Services;

namespace Shop.WinForms.Forms;

public class CustomerForm : Form
{
    private DataGridView _gridProducts = null!;
    private DataGridView _gridCart = null!;
    private Label _lblTotal = null!;
    private Button _btnAddToCart = null!;
    private Button _btnCheckout = null!;
    private List<OrderItem> _cart = new();

    public CustomerForm()
    {
        InitializeComponent();
        LoadProducts();
    }

    private void InitializeComponent()
    {
        Text = "Вітрина покупця (Розробник: Льоша)";
        Size = new Size(800, 500);
        StartPosition = FormStartPosition.CenterScreen;

        var lblTitle = new Label
        {
            Text = "Каталог товарів та кошик",
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            Dock = DockStyle.Top,
            Height = 40
        };

        _gridProducts = new DataGridView
        {
            Location = new Point(20, 50),
            Size = new Size(450, 320),
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        _btnAddToCart = new Button
        {
            Text = "🛒 Додати в кошик",
            Location = new Point(20, 385),
            Size = new Size(160, 35)
        };
        _btnAddToCart.Click += (s, e) =>
        {
            // TODO (Льоша): Додати перевірку залишків та додавання товару в кошик
            if (_gridProducts.CurrentRow?.DataBoundItem is Product p)
            {
                _cart.Add(new OrderItem
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    UnitPrice = p.Price,
                    Quantity = 1
                });
                UpdateCart();
            }
        };

        _gridCart = new DataGridView
        {
            Location = new Point(490, 50),
            Size = new Size(270, 320),
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        _lblTotal = new Label
        {
            Text = "Сума: 0.00 грн",
            Location = new Point(490, 385),
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold)
        };

        _btnCheckout = new Button
        {
            Text = "✅ Купити",
            Location = new Point(650, 385),
            Size = new Size(110, 35)
        };
        _btnCheckout.Click += (s, e) =>
        {
            // TODO (Льоша): Оформити замовлення через ShopService.Instance.CreateOrder(...)
            if (_cart.Count > 0)
            {
                ShopService.Instance.CreateOrder("Клієнт", "+380000000000", new List<OrderItem>(_cart));
                _cart.Clear();
                UpdateCart();
                LoadProducts();
                MessageBox.Show("Замовлення успішно оформлено!");
            }
        };

        Controls.Add(_gridProducts);
        Controls.Add(_btnAddToCart);
        Controls.Add(_gridCart);
        Controls.Add(_lblTotal);
        Controls.Add(_btnCheckout);
        Controls.Add(lblTitle);
    }

    private void LoadProducts()
    {
        _gridProducts.DataSource = null;
        _gridProducts.DataSource = ShopService.Instance.Products.ToList();
    }

    private void UpdateCart()
    {
        _gridCart.DataSource = null;
        _gridCart.DataSource = _cart.Select(x => new { Товар = x.ProductName, Кількість = x.Quantity, Сума = x.TotalPrice }).ToList();
        _lblTotal.Text = $"Сума: {_cart.Sum(x => x.TotalPrice):N2} грн";
    }
}
