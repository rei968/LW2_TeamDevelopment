using Shop.Shared.DTOs.Categories;

namespace Shop.WinForms.Forms;

public class CategoryEditForm : Form
{
    public CreateCategoryDto? ResultCategory { get; private set; }

    private TextBox _txtName = null!;
    private TextBox _txtDescription = null!;
    private Button _btnSave = null!;
    private Button _btnCancel = null!;

    public CategoryEditForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = "Додати категорію";
        Size = new Size(420, 280);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Color.FromArgb(248, 249, 250);
        Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

        var lblName = new Label { Text = "Назва категорії *:", Location = new Point(25, 20), AutoSize = true };
        _txtName = new TextBox { Location = new Point(25, 42), Width = 350 };

        var lblDesc = new Label { Text = "Опис категорії:", Location = new Point(25, 75), AutoSize = true };
        _txtDescription = new TextBox { Location = new Point(25, 97), Width = 350, Height = 65, Multiline = true };

        _btnSave = new Button
        {
            Text = "Зберегти",
            Location = new Point(165, 180),
            Width = 100,
            Height = 34,
            BackColor = Color.FromArgb(46, 204, 113),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Cursor = Cursors.Hand
        };
        _btnSave.FlatAppearance.BorderSize = 0;
        _btnSave.Click += (s, e) =>
        {
            if (string.IsNullOrWhiteSpace(_txtName.Text))
            {
                MessageBox.Show("Введіть назву категорії.", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ResultCategory = new CreateCategoryDto
            {
                Name = _txtName.Text.Trim(),
                Description = _txtDescription.Text.Trim()
            };
            DialogResult = DialogResult.OK;
        };

        _btnCancel = new Button
        {
            Text = "Скасувати",
            Location = new Point(275, 180),
            Width = 100,
            Height = 34,
            BackColor = Color.FromArgb(189, 195, 199),
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };
        _btnCancel.FlatAppearance.BorderSize = 0;
        _btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;

        Controls.Add(lblName);
        Controls.Add(_txtName);
        Controls.Add(lblDesc);
        Controls.Add(_txtDescription);
        Controls.Add(_btnSave);
        Controls.Add(_btnCancel);
    }
}
