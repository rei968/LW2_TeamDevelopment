using Shop.Shared.DTOs;
using Shop.Shared.DTOs.Categories;
using Shop.Shared.DTOs.Products;
using Shop.Shared.Enums;
using Shop.WinForms.Services;

namespace Shop.WinForms.Forms;

public class AdminForm : Form
{
    private TabControl _tabControl = null!;

    // Products Tab
    private DataGridView _dgvProducts = null!;
    private TextBox _txtProductSearch = null!;
    private ComboBox _cmbProductCategory = null!;
    private CheckBox _chkLowStockOnly = null!;
    private NumericUpDown _numStockAdjust = null!;
    private List<ProductDto> _allProducts = new();
    private List<CategoryDto> _categories = new();

    // Orders Tab
    private DataGridView _dgvOrders = null!;
    private DataGridView _dgvOrderItems = null!;
    private ComboBox _cmbOrderStatusFilter = null!;
    private Label _lblSelectedOrderInfo = null!;
    private Button _btnStatusProcessing = null!;
    private Button _btnStatusCompleted = null!;
    private Button _btnStatusCancelled = null!;
    private List<OrderDto> _allOrders = new();
    private OrderDto? _selectedOrder;

    // Categories Tab
    private DataGridView _dgvCategories = null!;

    // Header labels
    private Label _lblUserInfo = null!;

    public AdminForm()
    {
        InitializeComponent();
        Load += async (s, e) => await LoadAllDataAsync();
    }

    private void InitializeComponent()
    {
        Text = "Адміністративна панель — E-Commerce Shop Manager";
        Size = new Size(1180, 720);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1000, 600);
        Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
        BackColor = Color.FromArgb(245, 247, 250);

        // 1. TOP HEADER
        var pnlHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = Color.FromArgb(34, 49, 63)
        };

        var lblBrand = new Label
        {
            Text = "🛒 E-Commerce Admin Console",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12.5f, FontStyle.Bold),
            Location = new Point(15, 14),
            AutoSize = true
        };

        _lblUserInfo = new Label
        {
            Text = $"Менеджер: {ApiClient.Instance.CurrentEmail ?? "admin@store.com"} ({ApiClient.Instance.CurrentRole ?? "Admin"})",
            ForeColor = Color.FromArgb(200, 220, 240),
            Font = new Font("Segoe UI", 9.5f),
            Location = new Point(340, 18),
            AutoSize = true
        };

        var btnRefreshAll = new Button
        {
            Text = "🔄 Оновити все",
            Location = new Point(910, 12),
            Size = new Size(125, 32),
            BackColor = Color.FromArgb(41, 128, 185),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnRefreshAll.FlatAppearance.BorderSize = 0;
        btnRefreshAll.Click += async (s, e) => await LoadAllDataAsync();

        var btnLogout = new Button
        {
            Text = "🚪 Вийти",
            Location = new Point(1045, 12),
            Size = new Size(100, 32),
            BackColor = Color.FromArgb(192, 57, 43),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnLogout.FlatAppearance.BorderSize = 0;
        btnLogout.Click += (s, e) =>
        {
            ApiClient.Instance.Logout();
            Hide();
            var login = new LoginForm();
            login.FormClosed += (sender, args) => Close();
            login.Show();
        };

        pnlHeader.Controls.Add(lblBrand);
        pnlHeader.Controls.Add(_lblUserInfo);
        pnlHeader.Controls.Add(btnRefreshAll);
        pnlHeader.Controls.Add(btnLogout);

        // 2. TAB CONTROL
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10f, FontStyle.Regular),
            Padding = new Point(15, 6)
        };

        var tabProducts = new TabPage("📦 Товари та Склад (Stock Management)");
        BuildProductsTab(tabProducts);

        var tabOrders = new TabPage("📋 Обробка замовлень (Orders Management)");
        BuildOrdersTab(tabOrders);

        var tabCategories = new TabPage("🏷️ Категорії товарів");
        BuildCategoriesTab(tabCategories);

        _tabControl.TabPages.Add(tabProducts);
        _tabControl.TabPages.Add(tabOrders);
        _tabControl.TabPages.Add(tabCategories);

        Controls.Add(_tabControl);
        Controls.Add(pnlHeader);
    }

    // =========================================================================
    // TAB 1: PRODUCTS & STOCK
    // =========================================================================
    private void BuildProductsTab(TabPage tab)
    {
        tab.BackColor = Color.White;

        // Top Filter Panel
        var pnlFilters = new Panel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = Color.FromArgb(245, 247, 250),
            Padding = new Padding(10)
        };

        var lblSearch = new Label { Text = "Пошук:", Location = new Point(12, 17), AutoSize = true };
        _txtProductSearch = new TextBox { Location = new Point(65, 14), Width = 190 };
        _txtProductSearch.KeyDown += async (s, e) => { if (e.KeyCode == Keys.Enter) await FilterProductsAsync(); };

        var lblCategory = new Label { Text = "Категорія:", Location = new Point(270, 17), AutoSize = true };
        _cmbProductCategory = new ComboBox
        {
            Location = new Point(345, 14),
            Width = 190,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cmbProductCategory.SelectedIndexChanged += async (s, e) => await FilterProductsAsync();

        _chkLowStockOnly = new CheckBox
        {
            Text = "⚠️ Тільки малий залишок (<= 5 шт.)",
            Location = new Point(555, 15),
            AutoSize = true,
            ForeColor = Color.DarkRed,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };
        _chkLowStockOnly.CheckedChanged += async (s, e) => await FilterProductsAsync();

        var btnFilter = new Button
        {
            Text = "🔍 Знайти",
            Location = new Point(810, 12),
            Size = new Size(95, 30),
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnFilter.FlatAppearance.BorderSize = 0;
        btnFilter.Click += async (s, e) => await FilterProductsAsync();

        var btnReset = new Button
        {
            Text = "Скинути",
            Location = new Point(915, 12),
            Size = new Size(85, 30),
            BackColor = Color.FromArgb(220, 225, 230),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnReset.FlatAppearance.BorderSize = 0;
        btnReset.Click += async (s, e) =>
        {
            _txtProductSearch.Text = "";
            _chkLowStockOnly.Checked = false;
            if (_cmbProductCategory.Items.Count > 0)
                _cmbProductCategory.SelectedIndex = 0;
            await FilterProductsAsync();
        };

        pnlFilters.Controls.Add(lblSearch);
        pnlFilters.Controls.Add(_txtProductSearch);
        pnlFilters.Controls.Add(lblCategory);
        pnlFilters.Controls.Add(_cmbProductCategory);
        pnlFilters.Controls.Add(_chkLowStockOnly);
        pnlFilters.Controls.Add(btnFilter);
        pnlFilters.Controls.Add(btnReset);

        // Bottom Actions Panel (CRUD & Stock)
        var pnlActions = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 65,
            BackColor = Color.FromArgb(245, 247, 250),
            Padding = new Padding(10)
        };

        var btnAdd = new Button
        {
            Text = "➕ Додати товар",
            Location = new Point(12, 16),
            Size = new Size(140, 34),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnAdd.FlatAppearance.BorderSize = 0;
        btnAdd.Click += async (s, e) => await AddProductAsync();

        var btnEdit = new Button
        {
            Text = "✏️ Редагувати",
            Location = new Point(160, 16),
            Size = new Size(125, 34),
            BackColor = Color.FromArgb(243, 156, 18),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnEdit.FlatAppearance.BorderSize = 0;
        btnEdit.Click += async (s, e) => await EditProductAsync();

        var btnDelete = new Button
        {
            Text = "🗑️ Видалити",
            Location = new Point(295, 16),
            Size = new Size(115, 34),
            BackColor = Color.FromArgb(231, 76, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnDelete.FlatAppearance.BorderSize = 0;
        btnDelete.Click += async (s, e) => await DeleteProductAsync();

        // Stock quick management section
        var lblStockManage = new Label
        {
            Text = "Зміна залишку:",
            Location = new Point(445, 23),
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };

        _numStockAdjust = new NumericUpDown
        {
            Location = new Point(560, 20),
            Width = 90,
            Maximum = 100000,
            Value = 10
        };

        var btnSetStock = new Button
        {
            Text = "💾 Зберегти залишок",
            Location = new Point(660, 16),
            Size = new Size(160, 34),
            BackColor = Color.FromArgb(52, 73, 94),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnSetStock.FlatAppearance.BorderSize = 0;
        btnSetStock.Click += async (s, e) => await UpdateStockQuantityAsync();

        var btnPlus5 = new Button
        {
            Text = "+5 шт",
            Location = new Point(830, 16),
            Size = new Size(65, 34),
            BackColor = Color.FromArgb(230, 235, 245),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnPlus5.Click += async (s, e) => await QuickStockDeltaAsync(5);

        var btnPlus10 = new Button
        {
            Text = "+10 шт",
            Location = new Point(900, 16),
            Size = new Size(70, 34),
            BackColor = Color.FromArgb(230, 235, 245),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnPlus10.Click += async (s, e) => await QuickStockDeltaAsync(10);

        pnlActions.Controls.Add(btnAdd);
        pnlActions.Controls.Add(btnEdit);
        pnlActions.Controls.Add(btnDelete);
        pnlActions.Controls.Add(lblStockManage);
        pnlActions.Controls.Add(_numStockAdjust);
        pnlActions.Controls.Add(btnSetStock);
        pnlActions.Controls.Add(btnPlus5);
        pnlActions.Controls.Add(btnPlus10);

        // DataGridView
        _dgvProducts = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            RowTemplate = { Height = 32 }
        };

        _dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 55 });
        _dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "Назва товару", Width = 280 });
        _dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CategoryName", HeaderText = "Категорія", Width = 170 });
        _dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Brand", HeaderText = "Бренд", Width = 110 });

        var colPrice = new DataGridViewTextBoxColumn { DataPropertyName = "Price", HeaderText = "Ціна (грн)", Width = 110 };
        colPrice.DefaultCellStyle.Format = "N2";
        colPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        _dgvProducts.Columns.Add(colPrice);

        var colStock = new DataGridViewTextBoxColumn { DataPropertyName = "StockQuantity", HeaderText = "Залишок (шт.)", Width = 120 };
        colStock.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _dgvProducts.Columns.Add(colStock);

        var colRating = new DataGridViewTextBoxColumn { DataPropertyName = "AverageRating", HeaderText = "Рейтинг ⭐", Width = 95 };
        colRating.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        colRating.DefaultCellStyle.Format = "N1";
        _dgvProducts.Columns.Add(colRating);

        var colCreated = new DataGridViewTextBoxColumn { DataPropertyName = "CreatedAt", HeaderText = "Створено", Width = 120 };
        colCreated.DefaultCellStyle.Format = "yyyy-MM-dd";
        _dgvProducts.Columns.Add(colCreated);

        // Highlight low stock rows in light red / amber
        _dgvProducts.CellFormatting += (s, e) =>
        {
            if (e.RowIndex >= 0 && e.RowIndex < _dgvProducts.Rows.Count)
            {
                if (_dgvProducts.Rows[e.RowIndex].DataBoundItem is ProductDto product)
                {
                    if (product.StockQuantity <= 5)
                    {
                        if (e.ColumnIndex == colStock.Index)
                        {
                            e.CellStyle!.BackColor = Color.FromArgb(255, 230, 230);
                            e.CellStyle.ForeColor = Color.DarkRed;
                            e.CellStyle.Font = new Font(_dgvProducts.Font, FontStyle.Bold);
                        }
                    }
                }
            }
        };

        _dgvProducts.SelectionChanged += (s, e) =>
        {
            if (_dgvProducts.SelectedRows.Count > 0 &&
                _dgvProducts.SelectedRows[0].DataBoundItem is ProductDto p)
            {
                _numStockAdjust.Value = Math.Max(0, p.StockQuantity);
            }
        };

        tab.Controls.Add(_dgvProducts);
        tab.Controls.Add(pnlActions);
        tab.Controls.Add(pnlFilters);
    }

    // =========================================================================
    // TAB 2: ORDERS MANAGEMENT
    // =========================================================================
    private void BuildOrdersTab(TabPage tab)
    {
        tab.BackColor = Color.White;

        // Top Filter Bar
        var pnlFilters = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.FromArgb(245, 247, 250),
            Padding = new Padding(10)
        };

        var lblStatus = new Label { Text = "Статус замовлення:", Location = new Point(12, 15), AutoSize = true };
        _cmbOrderStatusFilter = new ComboBox
        {
            Location = new Point(160, 12),
            Width = 180,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _cmbOrderStatusFilter.Items.Add("Всі статуси");
        _cmbOrderStatusFilter.Items.Add("New (Нові)");
        _cmbOrderStatusFilter.Items.Add("Processing (В обробці)");
        _cmbOrderStatusFilter.Items.Add("Completed (Виконані)");
        _cmbOrderStatusFilter.Items.Add("Cancelled (Скасовані)");
        _cmbOrderStatusFilter.SelectedIndex = 0;
        _cmbOrderStatusFilter.SelectedIndexChanged += async (s, e) => await FilterOrdersAsync();

        var btnRefreshOrders = new Button
        {
            Text = "🔄 Оновити",
            Location = new Point(355, 10),
            Size = new Size(110, 30),
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        btnRefreshOrders.FlatAppearance.BorderSize = 0;
        btnRefreshOrders.Click += async (s, e) => await FilterOrdersAsync();

        pnlFilters.Controls.Add(lblStatus);
        pnlFilters.Controls.Add(_cmbOrderStatusFilter);
        pnlFilters.Controls.Add(btnRefreshOrders);

        // Split container: Top for Orders list, Bottom for selected Order Details & Actions
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 320,
            BackColor = Color.FromArgb(220, 225, 230)
        };

        // Top: Orders DataGridView
        _dgvOrders = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            RowTemplate = { Height = 30 }
        };

        _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 50 });
        _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "OrderNumber", HeaderText = "Номер замовлення", Width = 180 });
        _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CustomerName", HeaderText = "Покупець", Width = 160 });
        _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CustomerPhone", HeaderText = "Телефон", Width = 130 });
        _dgvOrders.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ShippingAddress", HeaderText = "Адреса доставки", Width = 220 });

        var colTotal = new DataGridViewTextBoxColumn { DataPropertyName = "TotalAmount", HeaderText = "Сума (грн)", Width = 110 };
        colTotal.DefaultCellStyle.Format = "N2";
        colTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        _dgvOrders.Columns.Add(colTotal);

        var colStatus = new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Статус", Width = 110 };
        colStatus.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _dgvOrders.Columns.Add(colStatus);

        var colOrderDate = new DataGridViewTextBoxColumn { DataPropertyName = "CreatedAt", HeaderText = "Дата", Width = 140 };
        colOrderDate.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
        _dgvOrders.Columns.Add(colOrderDate);

        _dgvOrders.CellFormatting += (s, e) =>
        {
            if (e.ColumnIndex == colStatus.Index && e.Value is OrderStatus st)
            {
                e.CellStyle!.Font = new Font(_dgvOrders.Font, FontStyle.Bold);
                switch (st)
                {
                    case OrderStatus.New:
                        e.CellStyle.ForeColor = Color.FromArgb(211, 84, 0); // Orange
                        break;
                    case OrderStatus.Processing:
                        e.CellStyle.ForeColor = Color.FromArgb(41, 128, 185); // Blue
                        break;
                    case OrderStatus.Completed:
                        e.CellStyle.ForeColor = Color.FromArgb(39, 174, 96); // Green
                        break;
                    case OrderStatus.Cancelled:
                        e.CellStyle.ForeColor = Color.Gray;
                        break;
                }
            }
        };

        _dgvOrders.SelectionChanged += (s, e) => UpdateSelectedOrderView();
        split.Panel1.Controls.Add(_dgvOrders);

        // Bottom: Order Details Panel
        var pnlBottom = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 250, 252),
            Padding = new Padding(10)
        };

        var pnlDetailsHeader = new Panel
        {
            Dock = DockStyle.Top,
            Height = 45,
            BackColor = Color.FromArgb(235, 240, 245)
        };

        _lblSelectedOrderInfo = new Label
        {
            Text = "Оберіть замовлення зі списку для перегляду деталей",
            Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
            ForeColor = Color.FromArgb(44, 62, 80),
            Location = new Point(10, 12),
            AutoSize = true
        };

        // Action Status Buttons
        _btnStatusProcessing = new Button
        {
            Text = "⚙️ Взяти в обробку",
            Location = new Point(620, 8),
            Size = new Size(160, 30),
            BackColor = Color.FromArgb(52, 152, 219),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnStatusProcessing.FlatAppearance.BorderSize = 0;
        _btnStatusProcessing.Click += async (s, e) => await ChangeOrderStatusAsync(OrderStatus.Processing);

        _btnStatusCompleted = new Button
        {
            Text = "✅ Завершити",
            Location = new Point(790, 8),
            Size = new Size(130, 30),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnStatusCompleted.FlatAppearance.BorderSize = 0;
        _btnStatusCompleted.Click += async (s, e) => await ChangeOrderStatusAsync(OrderStatus.Completed);

        _btnStatusCancelled = new Button
        {
            Text = "❌ Скасувати",
            Location = new Point(930, 8),
            Size = new Size(120, 30),
            BackColor = Color.FromArgb(231, 76, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnStatusCancelled.FlatAppearance.BorderSize = 0;
        _btnStatusCancelled.Click += async (s, e) => await ChangeOrderStatusAsync(OrderStatus.Cancelled);

        pnlDetailsHeader.Controls.Add(_lblSelectedOrderInfo);
        pnlDetailsHeader.Controls.Add(_btnStatusProcessing);
        pnlDetailsHeader.Controls.Add(_btnStatusCompleted);
        pnlDetailsHeader.Controls.Add(_btnStatusCancelled);

        // Order Items Grid
        _dgvOrderItems = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            RowTemplate = { Height = 26 }
        };

        _dgvOrderItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductId", HeaderText = "ID Товару", Width = 90 });
        _dgvOrderItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Назва замовленого товару", Width = 340 });

        var colUnitPrice = new DataGridViewTextBoxColumn { DataPropertyName = "UnitPrice", HeaderText = "Ціна за од.", Width = 110 };
        colUnitPrice.DefaultCellStyle.Format = "N2";
        colUnitPrice.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        _dgvOrderItems.Columns.Add(colUnitPrice);

        var colQty = new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Кількість", Width = 100 };
        colQty.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _dgvOrderItems.Columns.Add(colQty);

        var colItemTotal = new DataGridViewTextBoxColumn { DataPropertyName = "TotalPrice", HeaderText = "Разом (грн)", Width = 120 };
        colItemTotal.DefaultCellStyle.Format = "N2";
        colItemTotal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        _dgvOrderItems.Columns.Add(colItemTotal);

        pnlBottom.Controls.Add(_dgvOrderItems);
        pnlBottom.Controls.Add(pnlDetailsHeader);
        split.Panel2.Controls.Add(pnlBottom);

        tab.Controls.Add(split);
        tab.Controls.Add(pnlFilters);
    }

    // =========================================================================
    // TAB 3: CATEGORIES
    // =========================================================================
    private void BuildCategoriesTab(TabPage tab)
    {
        tab.BackColor = Color.White;

        var pnlActions = new Panel
        {
            Dock = DockStyle.Top,
            Height = 55,
            BackColor = Color.FromArgb(245, 247, 250),
            Padding = new Padding(10)
        };

        var btnAddCategory = new Button
        {
            Text = "➕ Додати категорію",
            Location = new Point(12, 12),
            Size = new Size(180, 32),
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnAddCategory.FlatAppearance.BorderSize = 0;
        btnAddCategory.Click += async (s, e) => await AddCategoryAsync();

        var btnDeleteCategory = new Button
        {
            Text = "🗑️ Видалити категорію",
            Location = new Point(205, 12),
            Size = new Size(180, 32),
            BackColor = Color.FromArgb(231, 76, 60),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        btnDeleteCategory.FlatAppearance.BorderSize = 0;
        btnDeleteCategory.Click += async (s, e) => await DeleteCategoryAsync();

        pnlActions.Controls.Add(btnAddCategory);
        pnlActions.Controls.Add(btnDeleteCategory);

        _dgvCategories = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            RowTemplate = { Height = 30 }
        };

        _dgvCategories.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "ID", Width = 60 });
        _dgvCategories.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Name", HeaderText = "Назва категорії", Width = 260 });
        _dgvCategories.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Description", HeaderText = "Опис", Width = 380 });

        var colCount = new DataGridViewTextBoxColumn { DataPropertyName = "ProductsCount", HeaderText = "Кількість товарів", Width = 150 };
        colCount.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        _dgvCategories.Columns.Add(colCount);

        tab.Controls.Add(_dgvCategories);
        tab.Controls.Add(pnlActions);
    }

    // =========================================================================
    // DATA LOADING & LOGIC
    // =========================================================================
    private async Task LoadAllDataAsync()
    {
        try
        {
            _categories = await ApiClient.Instance.GetCategoriesAsync();

            // Populate category filter in products tab
            _cmbProductCategory.Items.Clear();
            _cmbProductCategory.Items.Add("Всі категорії");
            foreach (var cat in _categories)
            {
                _cmbProductCategory.Items.Add(cat.Name);
            }
            _cmbProductCategory.SelectedIndex = 0;

            _dgvCategories.DataSource = null;
            _dgvCategories.DataSource = _categories;

            await FilterProductsAsync();
            await FilterOrdersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка завантаження даних з Web API: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task FilterProductsAsync()
    {
        try
        {
            int? categoryId = null;
            if (_cmbProductCategory.SelectedIndex > 0)
            {
                var catName = _cmbProductCategory.SelectedItem?.ToString();
                var cat = _categories.FirstOrDefault(c => c.Name == catName);
                if (cat != null) categoryId = cat.Id;
            }

            var search = _txtProductSearch.Text.Trim();
            _allProducts = await ApiClient.Instance.GetProductsAsync(categoryId: categoryId, search: search);

            var displayList = _allProducts;
            if (_chkLowStockOnly.Checked)
            {
                displayList = displayList.Where(p => p.StockQuantity <= 5).ToList();
            }

            _dgvProducts.DataSource = null;
            _dgvProducts.DataSource = displayList;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка фільтрації товарів: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task AddProductAsync()
    {
        var form = new ProductEditForm(_categories);
        if (form.ShowDialog(this) == DialogResult.OK && form.ResultProduct != null)
        {
            var p = form.ResultProduct;
            var (success, msg) = await ApiClient.Instance.CreateProductAsync(new CreateProductDto
            {
                Name = p.Name,
                Brand = p.Brand,
                CategoryId = p.CategoryId,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Description = p.Description,
                ImageUrl = p.ImageUrl
            });

            if (success)
            {
                MessageBox.Show("Товар успішно додано!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await FilterProductsAsync();
            }
            else
            {
                MessageBox.Show($"Помилка: {msg}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async Task EditProductAsync()
    {
        if (_dgvProducts.SelectedRows.Count == 0 ||
            _dgvProducts.SelectedRows[0].DataBoundItem is not ProductDto selectedProduct)
        {
            MessageBox.Show("Будь ласка, оберіть товар зі списку.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var form = new ProductEditForm(_categories, selectedProduct);
        if (form.ShowDialog(this) == DialogResult.OK && form.ResultProduct != null)
        {
            var p = form.ResultProduct;
            var (success, msg) = await ApiClient.Instance.UpdateProductAsync(selectedProduct.Id, new UpdateProductDto
            {
                Name = p.Name,
                Brand = p.Brand,
                CategoryId = p.CategoryId,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                Description = p.Description,
                ImageUrl = p.ImageUrl
            });

            if (success)
            {
                MessageBox.Show("Товар успішно оновлено!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await FilterProductsAsync();
            }
            else
            {
                MessageBox.Show($"Помилка: {msg}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async Task DeleteProductAsync()
    {
        if (_dgvProducts.SelectedRows.Count == 0 ||
            _dgvProducts.SelectedRows[0].DataBoundItem is not ProductDto selectedProduct)
        {
            MessageBox.Show("Будь ласка, оберіть товар для видалення.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"Ви дійсно бажаєте видалити товар '{selectedProduct.Name}'?",
            "Підтвердження видалення",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm == DialogResult.Yes)
        {
            var (success, msg) = await ApiClient.Instance.DeleteProductAsync(selectedProduct.Id);
            if (success)
            {
                MessageBox.Show("Товар успішно видалено.", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await FilterProductsAsync();
            }
            else
            {
                MessageBox.Show($"Помилка видалення: {msg}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private async Task UpdateStockQuantityAsync()
    {
        if (_dgvProducts.SelectedRows.Count == 0 ||
            _dgvProducts.SelectedRows[0].DataBoundItem is not ProductDto selectedProduct)
        {
            MessageBox.Show("Будь ласка, оберіть товар зі списку.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        int newStock = (int)_numStockAdjust.Value;
        var (success, msg) = await ApiClient.Instance.UpdateStockAsync(selectedProduct.Id, newStock);
        if (success)
        {
            selectedProduct.StockQuantity = newStock;
            _dgvProducts.Refresh();
            MessageBox.Show($"Залишок для товару '{selectedProduct.Name}' оновлено на {newStock} шт.", "Склад оновлено", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show($"Помилка: {msg}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task QuickStockDeltaAsync(int delta)
    {
        if (_dgvProducts.SelectedRows.Count == 0 ||
            _dgvProducts.SelectedRows[0].DataBoundItem is not ProductDto selectedProduct)
        {
            MessageBox.Show("Будь ласка, оберіть товар зі списку.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        int target = Math.Max(0, selectedProduct.StockQuantity + delta);
        _numStockAdjust.Value = target;
        await UpdateStockQuantityAsync();
    }

    private async Task FilterOrdersAsync()
    {
        try
        {
            OrderStatus? status = null;
            if (_cmbOrderStatusFilter.SelectedIndex == 1) status = OrderStatus.New;
            else if (_cmbOrderStatusFilter.SelectedIndex == 2) status = OrderStatus.Processing;
            else if (_cmbOrderStatusFilter.SelectedIndex == 3) status = OrderStatus.Completed;
            else if (_cmbOrderStatusFilter.SelectedIndex == 4) status = OrderStatus.Cancelled;

            _allOrders = await ApiClient.Instance.GetOrdersAsync(status);
            _dgvOrders.DataSource = null;
            _dgvOrders.DataSource = _allOrders;

            UpdateSelectedOrderView();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка завантаження замовлень: {ex.Message}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdateSelectedOrderView()
    {
        if (_dgvOrders.SelectedRows.Count > 0 &&
            _dgvOrders.SelectedRows[0].DataBoundItem is OrderDto order)
        {
            _selectedOrder = order;
            _lblSelectedOrderInfo.Text = $"Замовлення: #{order.OrderNumber} | Клієнт: {order.CustomerName} | Сума: {order.TotalAmount:N2} грн | Статус: {order.Status}";

            _dgvOrderItems.DataSource = null;
            _dgvOrderItems.DataSource = order.Items;

            // Enable / disable status transition buttons
            _btnStatusProcessing.Enabled = order.Status == OrderStatus.New;
            _btnStatusCompleted.Enabled = order.Status == OrderStatus.Processing;
            _btnStatusCancelled.Enabled = order.Status != OrderStatus.Cancelled && order.Status != OrderStatus.Completed;
        }
        else
        {
            _selectedOrder = null;
            _lblSelectedOrderInfo.Text = "Оберіть замовлення зі списку для перегляду деталей";
            _dgvOrderItems.DataSource = null;
            _btnStatusProcessing.Enabled = false;
            _btnStatusCompleted.Enabled = false;
            _btnStatusCancelled.Enabled = false;
        }
    }

    private async Task ChangeOrderStatusAsync(OrderStatus newStatus)
    {
        if (_selectedOrder == null) return;

        var confirm = MessageBox.Show(
            $"Перевести замовлення #{_selectedOrder.OrderNumber} у статус '{newStatus}'?",
            "Підтвердження зміни статусу",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes) return;

        var (success, msg) = await ApiClient.Instance.UpdateOrderStatusAsync(_selectedOrder.Id, newStatus);
        if (success)
        {
            MessageBox.Show($"Статус замовлення успішно змінено на {newStatus}!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
            await FilterOrdersAsync();
        }
        else
        {
            MessageBox.Show($"Помилка зміни статусу: {msg}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private async Task AddCategoryAsync()
    {
        var form = new CategoryEditForm();
        if (form.ShowDialog(this) == DialogResult.OK && form.ResultCategory != null)
        {
            var (success, msg) = await ApiClient.Instance.CreateCategoryAsync(form.ResultCategory);
            if (success)
            {
                MessageBox.Show("Категорію успішно додано!", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAllDataAsync();
            }
            else
            {
                MessageBox.Show($"Помилка: {msg}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private async Task DeleteCategoryAsync()
    {
        if (_dgvCategories.SelectedRows.Count == 0 ||
            _dgvCategories.SelectedRows[0].DataBoundItem is not CategoryDto selectedCategory)
        {
            MessageBox.Show("Будь ласка, оберіть категорію для видалення.", "Увага", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"Видалити категорію '{selectedCategory.Name}'?",
            "Підтвердження видалення",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm == DialogResult.Yes)
        {
            var (success, msg) = await ApiClient.Instance.DeleteCategoryAsync(selectedCategory.Id);
            if (success)
            {
                MessageBox.Show("Категорію видалено.", "Успіх", MessageBoxButtons.OK, MessageBoxIcon.Information);
                await LoadAllDataAsync();
            }
            else
            {
                MessageBox.Show($"Помилка: {msg}", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
