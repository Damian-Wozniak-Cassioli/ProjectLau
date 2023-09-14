using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProjectLau
{
    /// <summary>
    /// Interaction logic for AddEditBatch.xaml
    /// </summary>
    public partial class AddEditBatch : Window
    {
        private string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ProjectLau;Trusted_Connection=True;";
        private SqlConnection conn;
        private string sql;

        private bool mode;
        private MainWindow mainWindow;
        public AddEditBatch(string lastBatchId, MainWindow mainWindow)
        {
            InitializeComponent();
            conn = new SqlConnection(connectionString);
            bool flag = int.TryParse(lastBatchId, out int result);
            if (flag)
            {
                txtBatchId.Text = (result + 1).ToString("00000");
            }

            Title = "Add Batch";
            btnAddEdit.Content = "Add";
            txtCompleted.Text = "0";
            txtToRepair.Text = "0";

            mode = true;
            this.mainWindow = mainWindow;
            btnDelete.IsEnabled = false;
            btnDelete.Visibility = Visibility.Collapsed;
        }

        public AddEditBatch(DataRowView dataRowView, MainWindow mainWindow)
        {
            InitializeComponent();
            conn = new SqlConnection(connectionString);
            txtBatchId.Text = dataRowView[0].ToString();
            txtBatchSize.Text = dataRowView[1].ToString();
            txtProductName.Text = dataRowView[2].ToString();
            txtCompleted.Text = dataRowView[3].ToString();
            txtToRepair.Text = dataRowView[4].ToString();

            txtBatchId.IsEnabled = false;

            Title = "Edit Batch";
            btnAddEdit.Content = "Edit";

            mode = false;
            this.mainWindow = mainWindow;
        }

        private void btnAddEdit_Click(object sender, RoutedEventArgs e)
        {
            if (txtBatchId.Text==""||txtBatchSize.Text==""||txtProductName.Text=="")
            {
                MessageBox.Show("You need to specify id, batch size, and product name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            

            conn.Open();

            //ADD BATCH
            if (mode == true)
            {
                sql = "INSERT INTO Batch (BatchId, BatchSize, ProductName, Completed, ToRepair) " +
                    "VALUES (@batchId, @batchSize, @productName, @completed, @toRepair)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@batchId", txtBatchId.Text);
                    cmd.Parameters.AddWithValue("@batchSize", txtBatchSize.Text);
                    cmd.Parameters.AddWithValue("@productName", txtProductName.Text);

                    if (txtCompleted.Text=="")
                    {
                        cmd.Parameters.AddWithValue("@completed", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@completed", txtCompleted.Text);
                    }

                    if (txtToRepair.Text=="")
                    {
                        cmd.Parameters.AddWithValue("@toRepair", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@toRepair", txtToRepair.Text);
                    }
                    
                    cmd.ExecuteNonQuery();
                }
            }
            //EDIT BATCH
            else
            {
                sql = "UPDATE Batch SET BatchSize = @batchSize, ProductName = @productName, " +
                    "Completed = @completed, ToRepair = @toRepair WHERE BatchId = @batchId";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@batchId", txtBatchId.Text);
                    cmd.Parameters.AddWithValue("@batchSize", txtBatchSize.Text);
                    cmd.Parameters.AddWithValue("@productName", txtProductName.Text);

                    if (txtCompleted.Text == "")
                    {
                        cmd.Parameters.AddWithValue("@completed", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@completed", txtCompleted.Text);
                    }

                    if (txtToRepair.Text == "")
                    {
                        cmd.Parameters.AddWithValue("@toRepair", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@toRepair", txtToRepair.Text);
                    }

                    cmd.ExecuteNonQuery();
                }
            }

            conn.Close();
            mainWindow.RefreshBatches();
            this.Close();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            conn.Open();

            sql = "DELETE FROM Product WHERE BatchId = @batchId";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@batchId", txtBatchId.Text);

                cmd.ExecuteNonQuery();
            }

            sql = "DELETE FROM Batch WHERE BatchId = @batchId";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@batchId", txtBatchId.Text);

                cmd.ExecuteNonQuery();
            }

            conn.Close();
            mainWindow.RefreshBatches();
            this.Close();
        }
    }
}
