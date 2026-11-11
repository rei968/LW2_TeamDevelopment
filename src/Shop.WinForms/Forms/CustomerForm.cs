using Shop.Shared.DTOs;
using Shop.Shared.DTOs.Categories;
using Shop.Shared.DTOs.Products;
using Shop.WinForms.Services;

namespace Shop.WinForms.Forms;

public class CustomerForm : Form
{
    private readonly ApiClient _api = ApiClient.Instance;

    // Filters
    private TextBox _txtSearch = null!;
    private ComboBox _cmbCategory = null!;
    private NumericUpDown _numMaxPrice = null!;
    private Button _btnFilter = null!;
    private Button _btnResetFilter = null!;

    // Catalog DataGridView
    private DataGridView _gridProducts = null!;
    private NumericUpDown _numQuantity = null!;
    private Button _btnAddToCart = null!;

    // Cart
    private DataGridView _gridCart = null!;
    private Button _btnRemoveFromCart = null!;
    private Button _btnClearCart = null!;
    private Label _lblTotal = null!;
    private Button _btnCheckout = null!;
    private Button _btnMyOrders = null!;

    // State
    private List<ProductDto> _products = new();
    private List<CategoryDto> _categories = new();
    private readonly List<OrderItemDto> _cart = new();

    public CustomerForm()
    {
        InitializeComponent();
        _ = LoadInitialDataAsync();
    }

    private void InitializeComponent()
    {
        Text = "Інтернет-магазин — Вітрина покупця";
        Size = new Size(1100, 720);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(245, 247, 250);
        Font = new Font("Segoe UI", 9.5f);

        // Header Panel
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 65,
            BackColor = Color.FromArgb(41, 128, 185)
        };
        var lblTitle = new Label
        {
            Text = "🛍️ Каталог товарів та кошик покупця",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 14f, FontStyle.Bold),
            Location = new Point(20, 15),
            AutoSize = true
        };
        var lblUser = new Label
        {
            Text = $"Користувач: {_api.CurrentFullName ?? "Покупець"} ({_api.CurrentEmail})",
            ForeColor = Color.FromArgb(230, 240, 255),
            Location = new Point(600, 20),
            AutoSize = true
        };
        var btnLogout = new Button
        {
            Text = "Вийти",
            Location = new Point(980, 15),
            Size = new Size(85, 32),
            BackColor = Color.FromArgb(192, 57, 43),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnLogout.Click += (s, e) =>
        {
            _api.Logout();
            Hide();
            var loginForm = new LoginForm();
            loginForm.FormClosed += (sender, args) => Close();
            loginForm.Show();
        };

        pnlHeader.Controls.Add(lblTitle);
        pnlHeader.Controls.Add(lblUser);
        pnlHeader.Controls.Add(btnLogout);

        // Filter Panel
        var pnlFilter = new Panel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = Color.White,
            Padding = new Padding(15, 10, 15, 10)
        };

        var lblSearch = new Label { Text = "Пошук:", Location = new Point(15, 16), AutoSize = true };
        _txtSearch = new TextBox { Location = new Point(70, 13), Width = 150 };

        var lblCat = new Label { Text = "Категорія:", Location = new Point(235, 16), AutoSize = true };
        _cmbCategory = new ComboBox { Location = new Point(310, 13), Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };

        var lblPrice = new Label { Text = "До ціни:", Location = new Point(465, 16), AutoSize = true };
        _numMaxPrice = new NumericUpDown { Location = new Point(530, 13), Width = 100, Maximum = 1000000, Increment = 1000 };

        _btnFilter = new Button
        {
            Text = "🔍 Фільтрувати",
            Location = new Point(645, 11),
            Size = new Size(115, 30),
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnFilter.Click += async (s, e) => await ApplyFilterAsync();

        _btnResetFilter = new Button
        {
            Text = "Скинути",
            Location = new Point(770, 11),
            Size = new Size(80, 30),
            BackColor = Color.FromArgb(236, 240, 241),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnResetFilter.Click += async (s, e) =>
        {
            _txtSearch.Clear();
            _cmbCategory.SelectedIndex = 0;
            _numMaxPrice.Value = 0;
            await ApplyFilterAsync();
        };

        _btnMyOrders = new Button
        {
            Text = "📦 Мої замовлення",
            Location = new Point(880, 11),
            Size = new Size(180, 30),
            BackColor = Color.FromArgb(39, 174, 96),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnMyOrders.Click += async (s, e) => await ShowMyOrdersAsync();

        pnlFilter.Controls.Add(lblSearch);
        pnlFilter.Controls.Add(_txtSearch);
        pnlFilter.Controls.Add(lblCat);
        pnlFilter.Controls.Add(_cmbCategory);
        pnlFilter.Controls.Add(lblPrice);
        pnlFilter.Controls.Add(_numMaxPrice);
        pnlFilter.Controls.Add(_btnFilter);
        pnlFilter.Controls.Add(_btnResetFilter);
        pnlFilter.Controls.Add(_btnMyOrders);

        // Split Main Panel: Catalog (Left) + Cart (Right)
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 650,
            BackColor = Color.FromArgb(220, 225, 230)
        };

        // Left Panel (Catalog)
        var pnlCatalog = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10) };
        var lblCatTitle = new Label { Text = "📋 Каталог товарів", Font = new Font("Segoe UI", 11f, FontStyle.Bold), Dock = DockStyle.Top, Height = 30 };

        _gridProducts = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White
        };

        var pnlCatalogBottom = new Panel { Dock = DockStyle.Bottom, Height = 45, Padding = new Padding(5) };
        var lblQty = new Label { Text = "К-сть:", Location = new Point(10, 12), AutoSize = true };
        _numQuantity = new NumericUpDown { Location = new Point(60, 10), Width = 60, Minimum = 1, Maximum = 100, Value = 1 };
        _btnAddToCart = new Button
        {
            Text = "🛒 Додати в кошик",
            Location = new Point(135, 7),
            Size = new Size(160, 32),
            BackColor = Color.FromArgb(41, 128, 185),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnAddToCart.Click += (s, e) => AddSelectedToCart();

        pnlCatalogBottom.Controls.Add(lblQty);
        pnlCatalogBottom.Controls.Add(_numQuantity);
        pnlCatalogBottom.Controls.Add(_btnAddToCart);

        pnlCatalog.Controls.Add(_gridProducts);
        pnlCatalog.Controls.Add(pnlCatalogBottom);
        pnlCatalog.Controls.Add(lblCatTitle);

        // Right Panel (Cart)
        var pnlCart = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(10) };
        var lblCartTitle = new Label { Text = "🧺 Мій кошик", Font = new Font("Segoe UI", 11f, FontStyle.Bold), Dock = DockStyle.Top, Height = 30 };

        _gridCart = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = Color.White
        };

        var pnlCartBottom = new Panel { Dock = DockStyle.Bottom, Height = 90, Padding = new Padding(5) };
        _lblTotal = new Label
        {
            Text = "Разом до сплати: 0.00 грн",
            Font = new Font("Segoe UI", 12f, FontStyle.Bold),
            ForeColor = Color.FromArgb(41, 128, 185),
            Location = new Point(10, 10),
            AutoSize = true
        };

        _btnRemoveFromCart = new Button
        {
            Text = "Видалити",
            Location = new Point(10, 45),
            Size = new Size(95, 32),
            BackColor = Color.FromArgb(231, 76, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnRemoveFromCart.Click += (s, e) => RemoveSelectedFromCart();

        _btnClearCart = new Button
        {
            Text = "Очистити",
            Location = new Point(115, 45),
            Size = new Size(90, 32),
            BackColor = Color.FromArgb(236, 240, 241),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnClearCart.Click += (s, e) => ClearCart();

        _btnCheckout = new Button
        {
            Text = "✅ Оформити замовлення",
            Location = new Point(215, 45),
            Size = new Size(185, 32),
            BackColor = Color.FromArgb(39, 174, 96),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnCheckout.Click += async (s, e) => await CheckoutAsync();

        pnlCartBottom.Controls.Add(_lblTotal);
        pnlCartBottom.Controls.Add(_btnRemoveFromCart);
        pnlCartBottom.Controls.Add(_btnClearCart);
        pnlCartBottom.Controls.Add(_btnCheckout);

        pnlCart.Controls.Add(_gridCart);
        pnlCart.Controls.Add(pnlCartBottom);
        pnlCart.Controls.Add(lblCartTitle);

        split.Panel1.Controls.Add(pnlCatalog);
        split.Panel2.Controls.Add(pnlCart);

        Controls.Add(split);
        Controls.Add(pnlFilter);
        Controls.Add(pnlHeader);
    }

    private async Task LoadInitialDataAsync()
    {
        _categories = await _api.GetCategoriesAsync();
        _cmbCategory.Items.Clear();
        _cmbCategory.Items.Add("Всі категорії");
        foreach (var c in _categories)
        {
            _cmbCategory.Items.Add(c.Name);
        }
        _cmbCategory.SelectedIndex = 0;

        await ApplyFilterAsync();
        UpdateCartGrid();
    }

    private async Task ApplyFilterAsync()
    {
        string? search = string.IsNullOrWhiteSpace(_txtSearch.Text) ? null : _txtSearch.Text.Trim();
        int? catId = null;
        if (_cmbCategory.SelectedIndex > 0)
        {
            var selectedName = _cmbCategory.SelectedItem?.ToString();
            catId = _categories.FirstOrDefault(c => c.Name == selectedName)?.Id;
        }
        decimal? maxPrice = _numMaxPrice.Value > 0 ? _numMaxPrice.Value : null;

        _products = await _api.GetProductsAsync(categoryId: catId, search: search, maxPrice: maxPrice);

        _gridProducts.DataSource = null;
        _gridProducts.DataSource = _products.Select(p => new
        {
            p.Id,
            Назва = p.Name,
            Бренд = p.Brand,
            Категорія = p.CategoryName,
            Ціна_грн = p.Price,
            Залишок = p.StockQuantity
        }).ToList();
    }

    private void AddSelectedToCart()
    {
        if (_gridProducts.CurrentRow == null)
        {
            MessageBox.Show("Оберіть товар із каталогу.", "Інформація", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var id = (int)_gridProducts.CurrentRow.Cells["Id"].Value;
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null) return;

        int qty = (int)_numQuantity.Value;
        if (product.StockQuantity < qty)
        {
            MessageBox.Show($"Недостатньо товару на складі! Доступно: {product.StockQuantity}", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var existing = _cart.FirstOrDefault(i => i.ProductId == id);
        if (existing != null)
        {
            if (product.StockQuantity < existing.Quantity + qty)
            {
                MessageBox.Show($"Не можна додати більше ніж залишок ({product.StockQuantity})!", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            existing.Quantity += qty;
        }
        else
        {
            _cart.Add(new OrderItemDto
            {
                Id = _cart.Count + 1,
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = qty
            });
        }

        UpdateCartGrid();
    }

    private void RemoveSelectedFromCart()
    {
        if (_gridCart.CurrentRow == null) return;
        int pId = (int)_gridCart.CurrentRow.Cells["ProductId"].Value;
        _cart.RemoveAll(i => i.ProductId == pId);
        UpdateCartGrid();
    }

    private void ClearCart()
    {
        _cart.Clear();
        UpdateCartGrid();
    }

    private void UpdateCartGrid()
    {
        _gridCart.DataSource = null;
        _gridCart.DataSource = _cart.Select(i => new
        {
            i.ProductId,
            Товар = i.ProductName,
            Ціна = i.UnitPrice,
            Кількість = i.Quantity,
            Сума = i.TotalPrice
        }).ToList();

        decimal total = _cart.Sum(i => i.TotalPrice);
        _lblTotal.Text = $"Разом до сплати: {total:N2} грн";
    }

    private async Task CheckoutAsync()
    {
        if (_cart.Count == 0)
        {
            MessageBox.Show("Кошик порожній! Додайте товари перед оформленням.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Simple Checkout Dialog
        using var checkoutForm = new Form
        {
            Text = "Оформлення замовлення",
            Size = new Size(400, 320),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            Font = new Font("Segoe UI", 9.5f)
        };

        var lblName = new Label { Text = "Ваше ПІБ:", Location = new Point(20, 20), AutoSize = true };
        var txtName = new TextBox { Text = _api.CurrentFullName ?? "Іван Покупець", Location = new Point(20, 45), Width = 340 };

        var lblPhone = new Label { Text = "Номер телефону:", Location = new Point(20, 85), AutoSize = true };
        var txtPhone = new TextBox { Text = "+380501234567", Location = new Point(20, 110), Width = 340 };

        var lblAddress = new Label { Text = "Адреса доставки:", Location = new Point(20, 150), AutoSize = true };
        var txtAddress = new TextBox { Text = "м. Київ, відділення №1", Location = new Point(20, 175), Width = 340 };

        var btnSubmit = new Button
        {
            Text = "Підтвердити замовлення",
            Location = new Point(20, 225),
            Size = new Size(340, 35),
            BackColor = Color.FromArgb(39, 174, 96),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };

        btnSubmit.Click += async (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Заповніть контактні дані!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var orderDto = new CreateOrderDto
            {
                CustomerName = txtName.Text.Trim(),
                CustomerPhone = txtPhone.Text.Trim(),
                ShippingAddress = txtAddress.Text.Trim(),
                Items = _cart.Select(i => new CreateOrderItemDto
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };

            var (ok, msg, createdOrder) = await _api.CreateOrderAsync(orderDto);
            if (ok && createdOrder != null)
            {
                MessageBox.Show($"Замовлення #{createdOrder.OrderNumber} на суму {createdOrder.TotalAmount:N2} грн успішно оформлено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearCart();
                checkoutForm.Close();
                await ApplyFilterAsync(); // refresh stock in catalog
            }
            else
            {
                MessageBox.Show($"Помилка оформлення: {msg}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        checkoutForm.Controls.Add(lblName);
        checkoutForm.Controls.Add(txtName);
        checkoutForm.Controls.Add(lblPhone);
        checkoutForm.Controls.Add(txtPhone);
        checkoutForm.Controls.Add(lblAddress);
        checkoutForm.Controls.Add(txtAddress);
        checkoutForm.Controls.Add(btnSubmit);

        checkoutForm.ShowDialog(this);
    }

    private async Task ShowMyOrdersAsync()
    {
        var orders = await _api.GetMyOrdersAsync();
        using var ordersForm = new Form
        {
            Text = "Історія моїх замовлень",
            Size = new Size(750, 450),
            StartPosition = FormStartPosition.CenterParent,
            Font = new Font("Segoe UI", 9.5f)
        };

        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            DataSource = orders.Select(o => new
            {
                Номер = o.OrderNumber,
                Дата = o.CreatedAt.ToString("dd.MM.yyyy HH:mm"),
                Сума = $"{o.TotalAmount:N2} грн",
                Статус = o.Status.ToString(),
                Товарів = o.Items.Count,
                Адреса = o.ShippingAddress
            }).ToList()
        };

        ordersForm.Controls.Add(grid);
        ordersForm.ShowDialog(this);
    }
}
