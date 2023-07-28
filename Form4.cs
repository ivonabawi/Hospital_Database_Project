using ClosedXML.Excel;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.DataFormats;

namespace hospital_1
{
    public partial class Form4 : Form
    {
        private Form1 form1;

        private MySqlConnection con;
        public Form4()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.form1 = new Form1();
            form1.Show();
            this.Close();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = "localhost",
                UserID = "root",
                Password = "",
                Database = "hospital"
            };

            using var con = new MySqlConnection(builder.ConnectionString);

            con.Open();
            using (var dataCommand = new MySqlCommand("SELECT * FROM drugs", con))
            using (var dataReader = dataCommand.ExecuteReader())
            {
                DataTable dataTable = new();
                dataTable.Load(dataReader);
                dataGridView1.DataSource = dataTable;
            }
            con.Close();

        }

        private void search_data_obat(String keyword)
        {
            con.Open();
            MySqlCommand dataCommand = new MySqlCommand("SELECT * FROM data_obat WHERE `Nama Obat` LIKE @keyword", this.con);
            dataCommand.Parameters.AddWithValue("@keyword", "%" + keyword + "%");
            MySqlDataReader dataReader = dataCommand.ExecuteReader();
            DataTable dataTable = new DataTable();
            dataTable.Load(dataReader);
            dataGridView1.DataSource = dataTable;
            con.Close();
        }

        private void load_data_obat()
        {
            con.Open();
            MySqlCommand dataCommand = new MySqlCommand("SELECT * FROM data_obat", this.con);
            MySqlDataReader dataReader = dataCommand.ExecuteReader();
            DataTable dataTable = new DataTable();
            dataTable.Load(dataReader);
            dataGridView1.DataSource = dataTable;
            con.Close();
        }

        private void clear_form()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox6.Text = "";
        }

        private void load_single_obat(int drug_id)
        {
            con.Open();
            MySqlCommand dataCommand = new MySqlCommand("SELECT * FROM drugs WHERE id = @drug_id", con);
            dataCommand.Parameters.AddWithValue("@drug_id", drug_id);
            MySqlDataReader dataReader = dataCommand.ExecuteReader();

            if (dataReader.HasRows == true)
            {
                dataReader.Read();
                textBox1.Text = dataReader.GetString(dataReader.GetOrdinal("name"));
                textBox2.Text = dataReader.GetString(dataReader.GetOrdinal("diagnose"));
                textBox3.Text = dataReader.GetInt32(dataReader.GetOrdinal("quantity")).ToString();
                textBox4.Text = dataReader.GetDecimal(dataReader.GetOrdinal("price")).ToString("0.##");
            }
            else
            {
                MessageBox.Show("Data Tidak Ditemukan", "Perhatian");
            }
            con.Close();
        }

        private void drop_single_obat(int drug_id)
        {
            con.Open();
            MySqlCommand dataCommand = new MySqlCommand("DELETE FROM drugs WHERE id = @drug_id", con);
            try
            {
                dataCommand.Parameters.AddWithValue("@drug_id", drug_id);
                dataCommand.ExecuteNonQuery();
                MessageBox.Show("Berhasil menghapus data", "Berhasil");
            }
            catch (Exception e)
            {
                MessageBox.Show("Gagal menghapus data. Obat pernah digunakan saat pemeriksaan obat (checkup medicine).", "Perhatian");
            }
            con.Close();
        }



        private void button6_Click(object sender, EventArgs e)
        {
            clear_form();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            search_data_obat(textBox6.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var rowIndex = dataGridView1.CurrentCell.RowIndex;
            var drug_id = dataGridView1.Rows[rowIndex].Cells[0].Value.ToString();
            textBox7.Text = drug_id;
            load_single_obat(Int16.Parse(drug_id));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var answer = MessageBox.Show("Anda yakin ingin menghapus data ini?", "Perhatian", MessageBoxButtons.YesNo);
            if (answer == DialogResult.Yes)
            {
                var rowIndex = dataGridView1.CurrentCell.RowIndex;
                var drug_id = dataGridView1.Rows[rowIndex].Cells[0].Value.ToString();
                drop_single_obat(Int16.Parse(drug_id));
                load_data_obat();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var nama_obat = textBox1.Text;
            var diagnosa = textBox2.Text;
            var jumlah = textBox3.Text;
            var harga = textBox4.Text;

            con.Open();
            MySqlCommand dataCommand;
            if (textBox6.Text == "")
            {
                dataCommand = new MySqlCommand("INSERT INTO drugs (name, diagnose, quantity, price) VALUE (@name, @diagnose, @quantity, @price)", con);
            }
            else
            {
                dataCommand = new MySqlCommand("UPDATE drugs SET name = @name, diagnose = @diagnose, quantity = @quantity, price = @price WHERE id = @drug_id", con);
                dataCommand.Parameters.AddWithValue("@drug_id", textBox6.Text);
            }
            dataCommand.Parameters.AddWithValue("@name", nama_obat);
            dataCommand.Parameters.AddWithValue("@diagnose", diagnosa);
            dataCommand.Parameters.AddWithValue("@quantity", jumlah);
            dataCommand.Parameters.AddWithValue("@price", harga);
            int affected_rows = dataCommand.ExecuteNonQuery();
            if (affected_rows > 0)
            {
                MessageBox.Show("Berhasil menyimpan data", "Informasi");
            }
            con.Close();
            clear_form();
            load_data_obat();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            {
                {
                    DataTable dt = new DataTable();
                    foreach (DataGridViewColumn col in dataGridView1.Columns)
                    {
                        dt.Columns.Add(col.Name);
                    }

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        DataRow dRow = dt.NewRow();
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            dRow[cell.ColumnIndex] = cell.Value;
                        }
                        dt.Rows.Add(dRow);
                    }

                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        wb.Worksheets.Add(dt, "Sheet1");
                        using (MemoryStream stream = new MemoryStream())
                        {
                            wb.SaveAs(stream);
                            SaveFileDialog saveFileDialog = new SaveFileDialog();
                            saveFileDialog.Filter = "Excel Files|*.xlsx";
                            saveFileDialog.FileName = "Rekap Data Pasien.xlsx";

                            if (saveFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                File.WriteAllBytes(saveFileDialog.FileName, stream.ToArray());
                                MessageBox.Show("File saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                        }
                    }
                }
            }
        }
    }
}
