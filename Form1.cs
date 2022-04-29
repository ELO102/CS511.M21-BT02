using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApp1
{
    public partial class Shop : Form
    {
        private void debug()
        {
            this.Controls.Add(this.panel_itemsShow); // 0
            this.Controls.Add(this.panel_search);
            this.Controls.Add(this.panel_itemDetail);
            this.Controls.Add(this.panel_history);
            this.Controls.Add(this.panel_cart);
            this.Controls.Add(this.panel_payment); 
            this.Controls.Add(this.panel_Nav);
            this.Controls.Add(this.panel_Info); // 7
        }
        
        // Init
        private string strFolder = @"E:\Study\CS511.M21\CS511.M21-BT02\";
        
        private DataTable dt_all_items = GetAllItems();
        private DataTable dt_cart_items = new DataTable();
        //private DataTable dt_history = new DataTable();
        private DataTable dt_historyView = new DataTable();
        private int curChosen_id = -1;
        private bool isPayAll = false;

        private void Init_DataTable()
        {
            dt_cart_items.Columns.Add("ID", typeof(string));
            dt_cart_items.Columns.Add("name", typeof(string));
            dt_cart_items.Columns.Add("color", typeof(string));
            dt_cart_items.Columns.Add("wire", typeof(string));
            dt_cart_items.Columns.Add("quantity", typeof(int));
            dt_cart_items.Columns.Add("price", typeof(int));
            dt_cart_items.Columns.Add("total", typeof(int));

            //dt_history.Columns.Add("order_date", typeof(string));
            //dt_history.Columns.Add("receiver", typeof(string));
            //dt_history.Columns.Add("phone", typeof(string));
            //dt_history.Columns.Add("destination", typeof(string));
            //dt_history.Columns.Add("time", typeof(string));
            //dt_history.Columns.Add("total", typeof(int));
            //dt_history.Columns.Add("status", typeof(string));

            dt_historyView.Columns.Add("ID", typeof(string));

        }
        private void hide_all_mainPanel()
        {
            panel_itemsShow.Visible = false;
            panel_search.Visible = false;
            panel_itemDetail.Visible = false;
            panel_history.Visible = false;
            panel_cart.Visible = false;
            panel_payment.Visible = false;
        }
        private void show_panel(Panel pan)
        {
            pan.Visible = true;
            pan.BringToFront();
        }
        private void Raise_ERROR()
        {
            string message = "Error";
            string title = "Error";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            MessageBox.Show(message, title, buttons);
        }

        public Shop()
        {
            // Init
            InitializeComponent();
            Init_DataTable();
            Show_Items();
            hide_all_mainPanel();
            show_panel(panel_itemsShow);
        }

        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine().Split(new string[] { ", " }, StringSplitOptions.None);
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(new string[] { ", " }, StringSplitOptions.None);
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        dr[i] = rows[i];
                    }
                    dt.Rows.Add(dr);
                }

            }

            return dt;
        }

        // Get functions
        private static DataTable GetAllItems()
        {
            return ConvertCSVtoDataTable(@"E:\Study\CS511.M21\CS511.M21-BT02\Data\All_Items.csv");
        }
        private string GetSearchCondition()
        {
            string conditions = "";
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                conditions += "name LIKE '%" + textBox1.Text + "%'";
            }

            if (!string.IsNullOrEmpty(textBox2.Text))
            {
                if (conditions != "") conditions += " AND ";
                conditions += "ID = '" + textBox2.Text + "'";
            }
            
            if (!string.IsNullOrEmpty(textBox6.Text) && !string.IsNullOrEmpty(textBox7.Text))
            {
                if (conditions != "") conditions += " AND ";
                conditions += "(";
                conditions += "price >= " + textBox6.Text;
                conditions += " AND ";
                conditions += "price <= " + textBox7.Text;
                conditions += ")";
            }
            else if (!string.IsNullOrEmpty(textBox7.Text))
            {
                if (conditions != "") conditions += " AND ";
                conditions += "price <= " + textBox7.Text;
            }
            else if (!string.IsNullOrEmpty(textBox6.Text))
            {
                if (conditions != "") conditions += " AND ";
                conditions += "price >= " + textBox6.Text;
            }
            if (checkedListBox1.CheckedItems.Count > 0)
            {
                if (conditions != "") conditions += " AND ";
                if (checkedListBox1.CheckedItems.Count == 1)
                {
                    string temp = checkedListBox1.CheckedItems[0].ToString();
                    conditions += "type = '" + temp + "'";
                }
                else
                {
                    conditions += "(";
                    conditions += "type = '" + checkedListBox1.CheckedItems[0].ToString() + "'";
                    conditions += " OR ";
                    conditions += "type = '" + checkedListBox1.CheckedItems[1].ToString() + "'";
                    conditions += ")";
                }
            }
            return conditions;
        }
        private DataRow GetItemFromID()
        {
            DataRow item = dt_all_items.Select("ID = '" + curChosen_id + "'").First();
            return item;
        }
        private int GetTotalItemsInCart()
        {
            int count = 0;
            foreach (DataRow row in dt_cart_items.Rows)
            {
                count += Convert.ToInt32(row["quantity"]);
            }
            return count;
        }
        private int GetTotalMoneyInCart()
        {
            int count = 0;
            foreach (DataRow row in dt_cart_items.Rows)
            {
                count += Convert.ToInt32(row["total"]);
            }
            return count;
        }
        private int GetTotalMoneyInItemsChecked()
        {
            int count = 0;
            for (int i = 0; i < listView2.Items.Count; i++)
            {
                if (listView2.Items[i].Checked)
                {
                    count += Convert.ToInt32(dt_cart_items.Rows[i]["total"]);
                }
            }
            return count;
        }

        // Show functions
        private void Show_Items()
        {
            int index = 0;
            int page_idx = Convert.ToInt32(numericUpDown_page.Value);
            index = index + page_idx * 10;
            numericUpDown_page.Maximum = dt_all_items.Select().Length/10;
            foreach (var pan in tableLayoutPanel_itemsShow.Controls.OfType<Panel>()) // iter all panel by Add order
            {
                pan.Visible = false; // hide panel

                if (index + 1 > dt_all_items.Select().Length) // checking if current_index > num_movie 
                {
                    continue; // if true then pass this panel and don't show it
                }

                pan.Visible = true; // show panel
                DataRow dr = dt_all_items.Rows[index]; // get row[idx] in datatable

                pan.Name = dr["ID"].ToString();
                
                string path_img = dr["img_path"].ToString();
                PictureBox pb = pan.Controls.OfType<PictureBox>().First();
                pb.ImageLocation = Path.Combine(strFolder, path_img);

                Label[] label = pan.Controls.OfType<Label>().ToArray();
                label[1].Text = dr["name"].ToString();
                label[0].Text = dr["price"].ToString();

                index++;
            }
        }
        private void Show_Items_viewed()
        {
            int index = 0;
            int page_idx = Convert.ToInt32(numericUpDown_page.Value);
            index = index + page_idx * 10;
            DataView dt_view = new DataView(dt_historyView);
            DataRow[] dt = dt_view.ToTable(true, "id").Select();
            numericUpDown_page.Maximum = dt.Length / 10;
            foreach (var pan in tableLayoutPanel_itemsShow.Controls.OfType<Panel>()) // iter all panel by Add order
            {
                pan.Visible = false; // hide panel

                if (index + 1 > dt.Length) // checking if current_index > num_movie 
                {
                    continue; // if true then pass this panel and don't show it
                }

                pan.Visible = true; // show panel
                DataRow dr = dt[index + page_idx * 10]; // get row[idx] in datatable

                pan.Name = dr["ID"].ToString();

                string path_img = dr["img_path"].ToString();
                PictureBox pb = pan.Controls.OfType<PictureBox>().First();
                pb.ImageLocation = Path.Combine(strFolder, path_img);

                Label[] label = pan.Controls.OfType<Label>().ToArray();
                label[1].Text = dr["name"].ToString();
                label[0].Text = dr["price"].ToString();

                index++;
            }
        }
        private void Show_Items_Chuot()
        {
            int index = 0;
            int page_idx = Convert.ToInt32(numericUpDown_page.Value);
            index = index + page_idx * 10;
            DataRow[] dt = dt_all_items.Select("type = 'chuot'");
            numericUpDown_page.Maximum = dt.Length/10;
            foreach (var pan in tableLayoutPanel_itemsShow.Controls.OfType<Panel>()) // iter all panel by Add order
            {
                pan.Visible = false; // hide panel

                if (index + 1 > dt.Length) // checking if current_index > num_movie 
                {
                    continue; // if true then pass this panel and don't show it
                }

                pan.Visible = true; // show panel
                DataRow dr = dt[index + page_idx * 10]; // get row[idx] in datatable

                pan.Name = dr["ID"].ToString();

                string path_img = dr["img_path"].ToString();
                PictureBox pb = pan.Controls.OfType<PictureBox>().First();
                pb.ImageLocation = Path.Combine(strFolder, path_img);

                Label[] label = pan.Controls.OfType<Label>().ToArray();
                label[1].Text = dr["name"].ToString();
                label[0].Text = dr["price"].ToString();

                index++;
            }
        }
        private void Show_Items_BanPhim()
        {
            int index = 0;
            int page_idx = Convert.ToInt32(numericUpDown_page.Value);
            index = index + page_idx * 10;
            DataRow[] dt = dt_all_items.Select("type = 'ban phim'");
            numericUpDown_page.Maximum = dt.Length/10;
            foreach (var pan in tableLayoutPanel_itemsShow.Controls.OfType<Panel>()) // iter all panel by Add order
            {
                pan.Visible = false; // hide panel

                if (index + 1 > dt.Length) // checking if current_index > num_movie 
                {
                    continue; // if true then pass this panel and don't show it
                }

                pan.Visible = true; // show panel
                DataRow dr = dt[index + page_idx * 10]; // get row[idx] in datatable

                pan.Name = dr["ID"].ToString();

                string path_img = dr["img_path"].ToString();
                PictureBox pb = pan.Controls.OfType<PictureBox>().First();
                pb.ImageLocation = Path.Combine(strFolder, path_img);

                Label[] label = pan.Controls.OfType<Label>().ToArray();
                label[1].Text = dr["name"].ToString();
                label[0].Text = dr["price"].ToString();

                index++;
            }
        }
        private void Show_Items_Condition()
        {
            int index = 0;
            int page_idx = Convert.ToInt32(numericUpDown_page.Value);
            index = index + page_idx * 10;
            DataRow[] dt = dt_all_items.Select(GetSearchCondition());
            
            numericUpDown_page.Maximum = dt.Length/10;
            foreach (var pan in tableLayoutPanel_itemsShow.Controls.OfType<Panel>()) // iter all panel by Add order
            {
                pan.Visible = false; // hide panel

                if (index + 1 > dt.Length) // checking if current_index > num_movie 
                {
                    continue; // if true then pass this panel and don't show it
                }

                pan.Visible = true; // show panel
                DataRow dr = dt[index + page_idx * 10]; // get row[idx] in datatable

                pan.Name = dr["ID"].ToString();

                string path_img = dr["img_path"].ToString();
                PictureBox pb = pan.Controls.OfType<PictureBox>().First();
                pb.ImageLocation = Path.Combine(strFolder, path_img);

                Label[] label = pan.Controls.OfType<Label>().ToArray();
                label[1].Text = dr["name"].ToString();
                label[0].Text = dr["price"].ToString();

                index++;
            }
        }
        private void Show_Items_Single(Panel pan)
        {
            //curChosen_id = Int32.Parse(pan.Name);
            DataRow dr = dt_all_items.Select("ID = '" + curChosen_id.ToString() + "'").First();

            string path_img = dr["img_path"].ToString();
            pictureBox4.ImageLocation = Path.Combine(strFolder, path_img);

            label_itemName.Text = dr["name"].ToString();
            label_itemPrice.Text = dr["price"].ToString() + "đ";

            string path_txt = dr["txt_path"].ToString();
            richTextBox1.Text = File.ReadAllText(Path.Combine(strFolder, path_txt));

            if (dr["wire"].ToString() == "false")
            {
                label15.Visible = false;
                comboBox2.Visible = false;
            }
        }
        private void Show_payment(bool isAll)
        {
            if (isAll)
                textBox14.Text = GetTotalMoneyInCart().ToString();
            else
                textBox14.Text = GetTotalMoneyInItemsChecked().ToString();
            textBox10.Text = (Convert.ToInt32(textBox14.Text) + 15000).ToString();
        }
        // Click
        private void Add_to_Cart_click(object sender, EventArgs e)
        {
            DataRow item = GetItemFromID();
            DataRow new_item = dt_cart_items.NewRow();
            new_item["ID"] = item["ID"];
            new_item["name"] = item["name"];
            new_item["price"] = item["price"];

            if (comboBox1.SelectedIndex != -1)
                new_item["color"] = comboBox1.SelectedItem.ToString();
            else
            {
                Raise_ERROR();
                return;
            }
            if (Convert.ToBoolean(item["wire"]) == true)
            {
                if (comboBox2.SelectedIndex != -1)
                    new_item["wire"] = comboBox2.SelectedItem.ToString();
                else
                {
                    Raise_ERROR();
                    return;
                }
            }
            else new_item["wire"] = "Không";

            if (numericUpDown1.Value > 0)
                new_item["quantity"] = numericUpDown1.Value;
            else
            {
                Raise_ERROR();
                return;
            }

            new_item["total"] = Convert.ToInt32(new_item["price"]) * Convert.ToInt32(new_item["quantity"]);
            dt_cart_items.Rows.Add(new_item);
            Update_add1_listView2(new_item);
            Update_Cart_status();

            string message = "Thêm vào giỏ thành công";
            string title = "Thành công";
            MessageBoxButtons buttons = MessageBoxButtons.OK;
            MessageBox.Show(message, title, buttons);
        }
        private void Home_click(object sender, EventArgs e)
        {
            hide_all_mainPanel();
            show_panel(panel_itemsShow);
            numericUpDown_page.Value = 0;
            curChosen_id = -1;
            Show_Items();
        }
        private void Show_panelSearch_click(object sender, EventArgs e)
        {
            hide_all_mainPanel();
            show_panel(panel_search);
        }
        private void Show_panelHistory_click(object sender, EventArgs e)
        {
            hide_all_mainPanel();
            show_panel(panel_history);
        }
        private void Search_click(object sender, EventArgs e)
        {
            hide_all_mainPanel();
            show_panel(panel_itemsShow);
            Show_Items_Condition();
        }
        private void Viewed_click(object sender, EventArgs e)
        {
            hide_all_mainPanel();
            show_panel(panel_itemsShow);
            Show_Items_viewed();
        }
        private void Cart_click(object sender, EventArgs e)
        {
            hide_all_mainPanel();
            show_panel(panel_cart);
        }
        private void Cart_itemRemove_click(object sender, EventArgs e)
        {
            string message = "Bạn muốn xóa các sản phẩm này khỏi giỏ hàng?";
            string title = "Xóa sản phẩm";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result == DialogResult.Yes)
            {
                Update_remove1_listView2_and_dtCart();
                Update_Cart_status();
            }
            
        }
        private void button_exit_Click(object sender, EventArgs e)
        {
            string message = "Do you want to close this window?";
            string title = "Close Window";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
        private void button_back_Click(object sender, EventArgs e)
        {
            hide_all_mainPanel();
            show_panel(panel_itemsShow);
            curChosen_id = -1;
        }
        private void payAll_click(object sender, EventArgs e)
        {
            string message = "Thanh toán toàn bộ giỏ hàng?";
            string title = "Thanh toán";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result == DialogResult.Yes)
            {
                hide_all_mainPanel();
                show_panel(panel_payment);
                isPayAll = true;
                Show_payment(isPayAll);
            }
        }
        private void payChecked_click(object sender, EventArgs e)
        {
            string message = "Thanh toán toàn bộ sản phẩm được chọn?";
            string title = "Thanh toán";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result = MessageBox.Show(message, title, buttons);
            if (result == DialogResult.Yes)
            {
                hide_all_mainPanel();
                show_panel(panel_payment);
                isPayAll = false;
                Show_payment(isPayAll);
            }
        }
        private void pay_button_click(object sender, EventArgs e)
        {
            bool isSucceed = Update_add1_listView1();

            if (isSucceed)
            {
                if (isPayAll)
                    Update_removeAll_listView2_and_dtCart();
                else
                    Update_remove1_listView2_and_dtCart();

                Update_Cart_status();
                string message = "Thanh toán thành công!";
                string title = "Thanh toán";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = MessageBox.Show(message, title, buttons);

                hide_all_mainPanel();
                show_panel(panel_history);
            }
            else
            {
                string message = "Thanh toán thất bại!";
                string title = "Thanh toán";
                MessageBoxButtons buttons = MessageBoxButtons.OK;
                DialogResult result = MessageBox.Show(message, title, buttons);
            }
        }
        private void received_click(object sender, EventArgs e)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                if (listView1.Items[i].Checked)
                {
                    listView1.Items[i].SubItems[6].Text = "Đã nhận hàng";
                }
            }
        }
        private void panel_inList_click(object sender, EventArgs e)
        {
            Panel pan = (Panel)sender;
            curChosen_id = Int32.Parse(pan.Name);
            Show_Items_Single(pan);
            dt_historyView.Rows.Add(pan.Name);
            hide_all_mainPanel();
            show_panel(panel_itemDetail);
        }
        
        private void panelChild_inList_click(object sender, EventArgs e)
        {
            Panel pan = (Panel)((Control)sender).Parent;
            curChosen_id = Int32.Parse(pan.Name);
            Show_Items_Single(pan);
            dt_historyView.Rows.Add(pan.Name);
            hide_all_mainPanel();
            show_panel(panel_itemDetail);
        }
        // Value Change
        private void Change_page(object sender, EventArgs e)
        {
            Show_Items();
        }
        private void Update_Cart_status()
        {
            label_itemsCountNum.Text = GetTotalItemsInCart().ToString();

            DataView view = new DataView(dt_cart_items);
            label_typesCountNum.Text = view.ToTable(true, "ID").Select().Length.ToString();

            textBox3.Text = GetTotalMoneyInCart().ToString();
            textBox8.Text = textBox3.Text;
            textBox5.Text = (Convert.ToInt32(textBox3.Text) + 15000).ToString();

        }
        private void Update_add1_listView2 (DataRow new_item)
        {
            ListViewItem item = new ListViewItem(new_item[0].ToString());
            for (int i = 1; i < dt_cart_items.Columns.Count; i++)
            {
                item.SubItems.Add(new_item[i].ToString());
            }
            listView2.Items.Add(item);
        }
        private void Update_remove1_listView2_and_dtCart()
        {
            for (int i = 0; i < listView2.Items.Count; i++)
            {
                if (listView2.Items[i].Checked)
                {
                    listView2.Items[i].Remove();
                    dt_cart_items.Rows.RemoveAt(i);
                    i--;
                }
            }
        }
        private void Update_removeAll_listView2_and_dtCart()
        {
            for (int i = 0; i < listView2.Items.Count; i++)
            {
                listView2.Items[i].Remove();
                dt_cart_items.Rows.RemoveAt(i);
                i--;
            }
        }
        private bool Update_add1_listView1()
        {
            ListViewItem item = new ListViewItem(DateTime.Now.ToString("yyyy-MM-dd h:mm:ss tt"));
            if (!string.IsNullOrEmpty(textBox11.Text))
                item.SubItems.Add(textBox11.Text);
            else
            {
                Raise_ERROR();
                return false;
            }
            if (!string.IsNullOrEmpty(textBox9.Text))
                item.SubItems.Add(textBox9.Text);
            else
            {
                Raise_ERROR();
                return false;
            }
            if (!string.IsNullOrEmpty(textBox9.Text))
                item.SubItems.Add(textBox12.Text);
            else
            {
                Raise_ERROR();
                return false;
            }
            if (radioButton1.Checked)
                item.SubItems.Add("Sáng");
            else if (radioButton1.Checked)
                item.SubItems.Add("Chiều");
            else if (!radioButton1.Checked && !radioButton2.Checked)
            {
                Raise_ERROR();
                return false;
            }
            if (textBox14.Text != "0")
                item.SubItems.Add(textBox10.Text);
            else
            {
                Raise_ERROR();
                return false;
            }
            item.SubItems.Add("Đang giao hàng");

            listView1.Items.Add(item);
            return true;
        }
    }
}
