using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ProjectLau
{
    public class Product : ObservableObject
    {
        private string _productId;
        private string _productState;
        private string _description;
        private static int _productsCompleted = 0;
        private static int _productsToRepair = 0;

        public string ProductsCompleted
        {
            get { return _productsCompleted.ToString(); }
            set 
            {
                _productsCompleted++;
                OnPropertyChanged();
            }
        }

        public string ProductsToRepair
        {
            get 
            {
                return _productsToRepair.ToString(); 
            }
            set 
            {
                _productsToRepair++; 
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get { return _description; }
            set 
            {
                _description = value; 
                OnPropertyChanged();
            }
        }


        public string ProductState
        {
            get { return _productState; }
            set 
            {
                _productState = value;
                OnPropertyChanged();
            }
        }


        public string ProductId
        {
            get { return _productId; }
            set 
            { 
                _productId = value; 
                OnPropertyChanged();
            }
        }

        public static int BatchProducts = 0;

        private int chanceToBreak = 0;

        public void BreakingTheProduct()
        {
            if (ProductState != "Faulty")
            {
                chanceToBreak++;

                Random random = new Random();
                int num = random.Next(chanceToBreak, 100);

                if (num > 95)
                {
                    ProductState = "Faulty";
                }
            }
            
        }
    }
}
