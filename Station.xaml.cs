using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    /// Interaction logic for Station.xaml
    /// </summary>
    public partial class Station : UserControl
    {
        private static string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=ProjectLau;Trusted_Connection=True;";
        //private static SqlConnection conn;
        private static string sql;

        public int stationId;
        public int lineNumber;
        public string stationName;
        public string stationType;
        public int stationNumber;

        private bool isWorking = false;

        public string productId;
        public static List<Station> listOfStations = new List<Station>();
        private Queue<Product> waitingProductsQueue = new Queue<Product>();

        public static ObservableCollection<Product> currentBatch = new ObservableCollection<Product>();

        public Station()
        {
            InitializeComponent();

            //conn = new SqlConnection(connectionString);
        }

        public async void doSomeTask(Product product)
        {
            productId = product.ProductId;

            Dispatcher.Invoke(() =>
            {
                txtId.Text = productId;
                Background = Brushes.GreenYellow;
            });
            

            if (stationId == 19)
            {
                product.Description = "Product finished OK";
                product.ProductState = "OK";

                product.ProductsCompleted="";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    sql = "UPDATE Product SET ProductState = @productState WHERE ProductId = @productId";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("productId", productId);
                        cmd.Parameters.AddWithValue("@productState", "OK");

                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }
                
            }
            else
            {
                product.BreakingTheProduct();
                product.Description = "Station " + stationId;
                //currentBatch[int.Parse(productId.Split(' ')[1])-1].Description = "Station " + stationId;

                if (stationName.StartsWith("Test"))
                {
                    await Task.Delay(3000);

                    if (product.ProductState == "Faulty")
                    {
                        product.ProductsToRepair="";
                        using (SqlConnection conn = new SqlConnection(connectionString))
                        {
                            conn.Open();

                            sql = "UPDATE Product SET ProductState = @productState, Description = @description WHERE ProductId = @productId";

                            using (SqlCommand cmd = new SqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@productId", productId);
                                cmd.Parameters.AddWithValue("@productState", "Faulty");
                                cmd.Parameters.AddWithValue("@description", "Fault found on test in " + stationName);

                                cmd.ExecuteNonQuery();
                            }

                            conn.Close();
                        }

                        Dispatcher.Invoke(() =>
                        {
                            txtId.Text = productId;
                            Background = Brushes.Red;
                        });
                    }
                    else
                    {
                        new Thread(() => listOfStations[stationNumber + 1].WaitingLine(product)).Start();
                        Dispatcher.Invoke(() => {
                            txtId.Text = "Idle";
                            Background = Brushes.HotPink;
                        });
                    }
                    
                }
                else if (stationType == "Manual")
                {
                    await Task.Delay(3000);
                    new Thread(() => listOfStations[stationNumber + 1].WaitingLine(product)).Start();
                    Dispatcher.Invoke(() => {
                        txtId.Text = "Idle";
                        Background = Brushes.HotPink;
                    });
                }
                else
                {
                    await Task.Delay(2000);
                    new Thread(() => listOfStations[stationNumber + 1].WaitingLine(product)).Start();
                    Dispatcher.Invoke(() => {
                        txtId.Text = "Idle";
                        Background = Brushes.HotPink;
                    });
                }
            }

            isWorking = false;
            CheckQueue();
        }

        public void doSomeTask(string batch)
        {
            listOfStations[stationNumber + 1].WaitingLine(GenerateProduct(batch));
        }

        public void WaitingLine(Product product)
        {
            waitingProductsQueue.Enqueue(product);
            CheckQueue();
        }

        private void CheckQueue()
        {
            lock (waitingProductsQueue)
            {
                if (isWorking == false && waitingProductsQueue.Any())
                {
                    isWorking = true;
                    doSomeTask(waitingProductsQueue.Dequeue());
                }
            }
            
        }

        private Product GenerateProduct(string batch)
        {
            Product.BatchProducts++;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                sql = "INSERT INTO Product (BatchId, ProductId, ProductState) VALUES (@batchId, @productId, @productState)";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@batchId", batch);
                    cmd.Parameters.AddWithValue("@productId", batch + " " + Product.BatchProducts.ToString("0000"));
                    cmd.Parameters.AddWithValue("@productState", "Active");

                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
            
            Product product = new Product 
            {
                ProductId = batch + " " + Product.BatchProducts.ToString("0000"),
                ProductState = "Active",
                Description = "Station " + stationId 
            };

            currentBatch.Add(product);

            return product;
        }
    }
}
