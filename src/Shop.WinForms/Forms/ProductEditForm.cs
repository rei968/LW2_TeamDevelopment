using Shop.Shared.DTOs.Categories;
using Shop.Shared.DTOs.Products;

namespace Shop.WinForms.Forms;

public class ProductEditForm : Form
{
    public ProductDto? ResultProduct { get; private set; }

    private readonly ProductDto? _existingProduct;
    private readonly List<CategoryDto> _categories;

    private TextBox _txtName = null!;
    private TextBox _txtBrand = null!;
    private ComboBox _cmbCategory = null!;
    private NumericUpDown _numPrice = null!;
    private NumericUpDown _numStock = null!;
    private TextBox _txtDescription = null!;
    private TextBox _txtImageUrl = null!;
    private Button _btnSave = null!;
    private Button _btnCancel = null!;

    public ProductEditForm(List<CategoryDto> categories, ProductDto? product = null)
    {
        _categories = categories;
        _existingProduct = product;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = _existingProduct == null ? "Додати новий товар" : $"Редагувати товар: {_existingProduct.Name}";
        Size = new Size(520, 580);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(248, 249, 250);
        Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

        int lblX = 30, inputX = 30, width = 445;
        int y = 20;

        // Name
        var lblName = new Label { Text = "Назва товару *:", Location = new Point(lblX, y), AutoSize = true };
        y += 22;
        _txtName = new TextBox { Location = new Point(inputX, y), Width = width, Text = _existingProduct?.Name ?? "" };
        y += 38;

        // Brand
        var lblBrand = new Label { Text = "Бренд *:", Location = new Point(lblX, y), AutoSize = true };
        y += 22;
        _txtBrand = new TextBox { Location = new Point(inputX, y), Width = width, Text = _existingProduct?.Brand ?? "" };
        y += 38;

        // Category
        var lblCategory = new Label { Text = "Категорія *:", Location = new Point(lblX, y), AutoSize = true };
        y += 22;
        _cmbCategory = new ComboBox
        {
            Location = new Point(inputX, y),
            Width = width,
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = "Name",
            ValueMember = "Id",
            DataSource = _categories
        };
        if (_existingProduct != null)
        {
            _cmbCategory.SelectedValue = _existingProduct.CategoryId;
        }
        y += 38;

        // Price & Stock Quantity side by side
        var lblPrice = new Label { Text = "Ціна (грн) *:", Location = new Point(lblX, y), AutoSize = true };
        var lblStock = new Label { Text = "Залишок на складі (шт.) *:", Location = new Point(lblX + 230, y), AutoSize = true };
        y += 22;

        _numPrice = new NumericUpDown
        {
            Location = new Point(inputX, y),
            Width = 200,
            DecimalPlaces = 2,
            Maximum = 1000000,
            Value = _existingProduct?.Price ?? 1000
        };

        _numStock = new NumericUpDown
        {
            Location = new Point(inputX + 230, y),
            Width = 215,
            Maximum = 100000,
            Value = _existingProduct?.StockQuantity ?? 10
        };
        y += 38;

        // Description
        var lblDescription = new Label { Text = "Опис товару:", Location = new Point(lblX, y), AutoSize = true };
        y += 22;
        _txtDescription = new TextBox
        {
            Location = new Point(inputX, y),
            Width = width,
            Height = 80,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Text = _existingProduct?.Description ?? ""
        };
        y += 90;

        // Image URL
        var lblImage = new Label { Text = "URL зображення:", Location = new Point(lblX, y), AutoSize = true };
        y += 22;
        _txtImageUrl = new TextBox
        {
            Location = new Point(inputX, y),
            Width = width,
            Text = _existingProduct?.ImageUrl ?? ""
        };
        y += 48;

        // Buttons
        _btnSave = new Button
        {
            Text = "Зберегти товар",
            Location = new Point(inputX + 185, y),
            Width = 140,
            Height = 36,
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnSave.FlatAppearance.BorderSize = 0;
        _btnSave.Click += BtnSave_Click;

        _btnCancel = new Button
        {
            Text = "Скасувати",
            Location = new Point(inputX + 335, y),
            Width = 110,
            Height = 36,
            BackColor = Color.FromArgb(189, 195, 199),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnCancel.FlatAppearance.BorderSize = 0;
        _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

        Controls.Add(lblName);
        Controls.Add(_txtName);
        Controls.Add(lblBrand);
        Controls.Add(_txtBrand);
        Controls.Add(lblCategory);
        Controls.Add(_cmbCategory);
        Controls.Add(lblPrice);
        Controls.Add(_numPrice);
        Controls.Add(lblStock);
        Controls.Add(_numStock);
        Controls.Add(lblDescription);
        Controls.Add(_txtDescription);
        Controls.Add(lblImage);
        Controls.Add(_txtImageUrl);
        Controls.Add(_btnSave);
        Controls.Add(_btnCancel);
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_txtName.Text))
        {
            MessageBox.Show("Будь ласка, введіть назву товару.", "Помилка валідації", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_txtBrand.Text))
        {
            MessageBox.Show("Будь ласка, введіть бренд.", "Помилка валідації", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_cmbCategory.SelectedValue == null)
        {
            MessageBox.Show("Будь ласка, оберіть категорію.", "Помилка валідації", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ResultProduct = new ProductDto
        {
            Id = _existingProduct?.Id ?? 0,
            Name = _txtName.Text.Trim(),
            Brand = _txtBrand.Text.Trim(),
            CategoryId = (int)_cmbCategory.SelectedValue,
            CategoryName = (_cmbCategory.SelectedItem as CategoryDto)?.Name ?? "",
            Price = _numPrice.Value,
            StockQuantity = (int)_numStock.Value,
            Description = _txtDescription.Text.Trim(),
            ImageUrl = _txtImageUrl.Text.Trim()
        };

        DialogResult = DialogResult.OK;
    }
}
