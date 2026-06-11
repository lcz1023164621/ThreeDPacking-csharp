using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    internal partial class OrderMatchDialog : Form
    {
        private readonly ProductCatalog _productCatalog;

        public OrderMatchDialog()
            : this(null)
        {
        }

        public OrderMatchDialog(ProductCatalog productCatalog)
        {
            _productCatalog = productCatalog;
            InitializeComponent();
        }

        public List<OrderMatchItem> Items
        {
            get
            {
                var items = new List<OrderMatchItem>();
                foreach (DataGridViewRow row in dgvOrderItems.Rows)
                {
                    if (row.IsNewRow)
                    {
                        continue;
                    }

                    string barcode = Convert.ToString(row.Cells[colBarcode.Name].Value).Trim();
                    string sku = Convert.ToString(row.Cells[colSku.Name].Value).Trim();
                    string qtyText = Convert.ToString(row.Cells[colOrderQuantity.Name].Value).Trim();
                    int quantity;
                    if (barcode.Length == 0 && sku.Length == 0 && qtyText.Length == 0)
                    {
                        continue;
                    }
                    if (barcode.Length == 0 || !int.TryParse(qtyText, out quantity) || quantity < 0)
                    {
                        throw new InvalidOperationException("订单匹配信息需要填写条码号，并且订单数量必须是非负整数。");
                    }

                    ProductRecord product = _productCatalog == null ? null : _productCatalog.Find(barcode);
                    if (_productCatalog != null && product == null)
                    {
                        throw new InvalidOperationException("条码 " + barcode + " 不在 ProductInfo 中，不能保存到订单。");
                    }

                    items.Add(new OrderMatchItem
                    {
                        Barcode = product == null ? barcode : product.Barcode,
                        Sku = product == null ? sku : FirstNonEmpty(product.GetValue("SKU", "Sku", "sku", "货号", "产品货号"), sku),
                        OrderQuantity = quantity,
                        Length = product == null ? string.Empty : product.GetValue("Length", "L", "长", "长度"),
                        Width = product == null ? string.Empty : product.GetValue("Width", "W", "宽", "宽度"),
                        Height = product == null ? string.Empty : product.GetValue("Height", "H", "高", "高度")
                    });
                }
                return items;
            }
            set
            {
                dgvOrderItems.Rows.Clear();
                if (value == null)
                {
                    return;
                }
                foreach (OrderMatchItem item in value)
                {
                    dgvOrderItems.Rows.Add(item.Barcode, item.Sku, item.OrderQuantity);
                }
            }
        }

        private void btnImportCsv_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "CSV 文件 (*.csv)|*.csv|所有文件 (*.*)|*.*";
                dialog.Title = "导入订单匹配信息";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                dgvOrderItems.Rows.Clear();
                foreach (string line in File.ReadLines(dialog.FileName, Encoding.Default))
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    string[] parts = line.Split(',');
                    if (parts.Length < 3)
                    {
                        continue;
                    }
                    if (parts[0].IndexOf("条码", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        continue;
                    }

                    dgvOrderItems.Rows.Add(parts[0].Trim(), parts[1].Trim(), parts[2].Trim());
                }
            }
        }

        private void btnDeleteSelected_Click(object sender, EventArgs e)
        {
            if (dgvOrderItems.SelectedRows.Count == 0)
            {
                return;
            }

            foreach (DataGridViewRow row in dgvOrderItems.SelectedRows)
            {
                if (!row.IsNewRow)
                {
                    dgvOrderItems.Rows.Remove(row);
                }
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            try
            {
                var unused = Items;
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "匹配信息错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private static string FirstNonEmpty(string primary, string fallback)
        {
            return string.IsNullOrWhiteSpace(primary) ? (fallback ?? string.Empty) : primary;
        }
    }
}
