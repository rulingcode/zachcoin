using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace calculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const double fix = 10000 * 10000;
        double caaa = 100 * 1000000;
        public MainWindow()
        {
            InitializeComponent();
            txt_strat.TextChanged += Txt_strat_TextChanged;
            txt_end.TextChanged += Txt_end_TextChanged;
            btn.Click += Btn_Click;
            max_pric.MouseDown += Max_pric_KeyDown;
            suport25.MouseDown += Suport25_KeyDown;
            lbl_number.MouseDown += Lbl_number_MouseDown;
        }
        private void Lbl_number_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText(lbl_number.Content.ToString().Replace(",", ""));
            Console.Beep();
        }
        private void Suport25_KeyDown(object sender, MouseEventArgs e)
        {
            Clipboard.SetText(suport25.Content.ToString().Replace("$", ""));
            Console.Beep();
        }

        private void Max_pric_KeyDown(object sender, MouseEventArgs e)
        {
            Clipboard.SetText(max_pric.Content.ToString().Replace("$", ""));
            Console.Beep();
        }

        private void Txt_end_TextChanged(object sender, TextChangedEventArgs e)
        {
            lbl_number.Content = "---";
        }
        private void Btn_Click(object sender, RoutedEventArgs e)
        {
            reset();
        }
        private void Txt_strat_TextChanged(object sender, TextChangedEventArgs e)
        {
            txt_end.Text = txt_strat.Text;
            lbl_number.Content = "---";
        }

        private void reset()
        {
            int n_start = 0;
            int n_end = 0;
            double n = 0;
            try
            {
                n_start = int.Parse(txt_strat.Text);
            }
            catch
            {
                error();
                return;
            }
            try
            {
                n_end = int.Parse(txt_end.Text);
            }
            catch
            {
                error();
                return;
            }
            if (n_start == 0 || n_end < n_start)
            {
                error();
                return;
            }
            n = get_tokens(n_start, n_end);
            lbl_number.Content = n.ToString("n0");
            Clipboard.SetText(n.ToString());
            double price = get_price(n_end);
            max_pric.Content = price.ToString("c6");
            pey_all.Content = (caaa * price).ToString("c0");
            suport25.Content = (price / 4).ToString("c6");
        }
        private static double get_price(int n_end)
        {
            return 500 / (fix / n_end);
        }
        double get_tokens(int start, int end)
        {
            double n = 0;
            for (int i = start; i <= end; i++)
            {
                var dv = fix / i;
                n += dv;
            }
            return n;
        }
        private void error()
        {
            lbl_number.Content = "---";
        }
    }
}
