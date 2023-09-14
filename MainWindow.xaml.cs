using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProjectLau
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ProjectLau;Trusted_Connection=True;";
        private SqlConnection conn;
        private string sql;
        private List<Station> stations = new List<Station>();
        private DataTable dt;
        private DataTable productTable;

        public MainWindow()
        {
            InitializeComponent();

            conn = new SqlConnection(connectionString);

            GetBatches();

            btnStartLine.IsEnabled = false;
            btnStopLine.IsEnabled = false;
            btnEditBatch.IsEnabled = false;
        }

        private void LoadStations()
        {
            conn.Open();

            sql = "SELECT * FROM Stations WHERE LineNumber = @lineNumber ORDER BY StationId";

            using(var command = new SqlCommand(sql, conn))
            {
                command.Parameters.AddWithValue("@lineNumber", ConfigurationManager.AppSettings["Line"]);

                using(var reader = command.ExecuteReader())
                {
                    int number = 0;

                    while (reader.Read())
                    {
                        stations.Add(new Station()
                        {
                            stationId = (int)reader["StationId"],
                            lineNumber = (int)reader["LineNumber"],
                            stationName = reader["StationName"].ToString(),
                            stationType = reader["StationType"].ToString(),
                            stationNumber = number
                        });
                        number++;
                    }
                }
            }

            conn.Close();

            foreach (Station station in stations)
            {
                station.txtName.Text = station.stationName;
                spStations.Children.Add(station);
            }

            Station.listOfStations = stations;
        }

        private void GetBatches()
        {
            conn.Open();

            sql = "SELECT * FROM BATCH";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                dt = new DataTable("Batches");
                adapter.Fill(dt);
                dgBatches.ItemsSource = dt.DefaultView;
            }

            conn.Close();
        }

        public void RefreshBatches()
        {
            dt.Clear();
            conn.Open();

            sql = "SELECT * FROM BATCH";

            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dt);
                dgBatches.ItemsSource = dt.DefaultView;
            }

            conn.Close();
        }

        private void dgBatches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnStartLine.IsEnabled = true;
            btnEditBatch.IsEnabled = true;
        }

        private void btnAddBatch_Click(object sender, RoutedEventArgs e)
        {
            var lastRow = (DataRowView)dgBatches.Items[dgBatches.Items.Count - 1];
            AddEditBatch addEditBatch = new AddEditBatch(lastRow[0].ToString(), this);
            addEditBatch.Show();
        }

        private void btnEditBatch_Click(object sender, RoutedEventArgs e)
        {
            var selectedRow = (DataRowView)dgBatches.SelectedItem;
            AddEditBatch addEditBatch = new AddEditBatch(selectedRow, this);
            addEditBatch.Show();
        }

        private void btnStartLine_Click(object sender, RoutedEventArgs e)
        {
            LoadStations();

            tabStations.IsEnabled = true;
            tabLog.IsEnabled = true;
            tabCurrent.IsEnabled = true;
            btnStopLine.IsEnabled = true;
            btnStartLine.IsEnabled = false;

            var selectedRow = (DataRowView)dgBatches.SelectedItem;

            LoadCurrentBatch(selectedRow);
        }

        private void btnStopLine_Click(object sender, RoutedEventArgs e)
        {
            btnStartLine.IsEnabled = true;
            btnStopLine.IsEnabled = false;
        }

        private async void ActiveLine()
        {
            //var lastRow = (DataRowView)dgProducts.Items[dgProducts.Items.Count - 1];
            //var lastRow = Product.currentBatch[Product.currentBatch.Count - 1];

            //int i = 0;
            //if (int.TryParse(lastRow.ProductId, out i));

            Product.BatchProducts = Station.currentBatch.Count;

            while (Product.BatchProducts < int.Parse(txtSize.Text))
            {
                await Task.Delay(3000);
                stations[0].doSomeTask(txtBatch.Text);
            }
        }

        private void LoadCurrentBatch(DataRowView currentBatch)
        {
            txtBatch.Text = currentBatch[0].ToString();
            txtSize.Text = currentBatch[1].ToString();
            txtCompleted.Content = currentBatch[3].ToString();
            txtToRepair.Content = currentBatch[4].ToString();

            if (txtCompleted.Content == "")
            {
                txtCompleted.Content = "0";
            }
            if (txtToRepair.Content == "")
            {
                txtToRepair.Content = "0";
            }

            conn.Open();

            sql = "SELECT ProductId, ProductState, Description FROM Product WHERE BatchId = @batchId";

            using(SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@batchId", currentBatch[0].ToString());

                using(SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Station.currentBatch.Add(new Product
                        {
                            ProductId = reader[0].ToString(),
                            ProductState = reader[1].ToString(),
                            Description = reader[2].ToString()
                        });
                    }
                }
                //productTable = new DataTable("Products");
                //SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                //adapter.Fill(productTable);
                //dgProducts.ItemsSource = dt.DefaultView;
            }

            dgProducts.ItemsSource = Station.currentBatch;
            conn.Close();

            ActiveLine();
        }
    }
}
