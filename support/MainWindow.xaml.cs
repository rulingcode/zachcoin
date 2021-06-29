using stellar_dotnet_sdk;
using stellar_dotnet_sdk.responses;
using stellar_dotnet_sdk.responses.effects;
using stellar_dotnet_sdk.responses.operations;
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

namespace support
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Server server;
        private const string asset_owner = "GA5ZSEJYB37JRC5AVCIA5MOP4RHTM335X2KGX3IHOJAPP5RE34K4KZVN";
        private const string asset = "USDC";
        public MainWindow()
        {
            InitializeComponent();
            Network.UsePublicNetwork();
            server = new Server("https://horizon.stellar.org");
        }
        double send(IEnumerable<AccountDebitedEffectResponse> list, string asset, string asset_owner)
        {
            return list.OfType<AccountDebitedEffectResponse>().Where(i => i.AssetCode == asset && i.AssetIssuer == asset_owner).Sum(i => double.Parse(i.Amount));
        }
        double receive(IEnumerable<EffectResponse> list, string asset, string asset_owner)
        {
            return list.OfType<AccountCreditedEffectResponse>().Where(i => i.AssetCode == asset && i.AssetIssuer == asset_owner).Sum(i => double.Parse(i.Amount));
        }
        double get_bought(IEnumerable<EffectResponse> list, string asset, string asset_owner)
        {
            var enumerable = list.OfType<TradeEffectResponse>().Where(i => i.BoughtAssetCode == asset && i.BoughtAssetIssuer == asset_owner && int.Parse(i.PagingToken.Split('-')[1]) <= 2).ToArray();
            return enumerable.Sum(i => double.Parse(i.BoughtAmount));
        }
        double get_sold(IEnumerable<EffectResponse> list, string asset, string asset_owner)
        {
            return list.OfType<TradeEffectResponse>().Where(i => i.SoldAssetCode == asset && i.SoldAssetIssuer == asset_owner && int.Parse(i.PagingToken.Split('-')[1]) <= 2).Sum(i => double.Parse(i.SoldAmount));
        }
        async Task<List<EffectResponse>> get_effect(string userid, string index)
        {

            List<EffectResponse> l = new();
        retry:
            var dv = await server.Effects.ForAccount(userid).Cursor(index).Limit(200).Execute();
            l.AddRange(dv.Records);
            if (dv.Records.Count < 200)
                return l;
            index = dv.Embedded.Records.Last().PagingToken;
            goto retry;
        }
        double get_balance(List<EffectResponse> list, string asset, string asset_owner)
        {
            var debited_list = list.OfType<AccountDebitedEffectResponse>().Where(i => i.AssetCode == asset && i.AssetIssuer == asset_owner).ToArray();
            var credited_list = list.OfType<AccountCreditedEffectResponse>().Where(i => i.AssetCode == asset && i.AssetIssuer == asset_owner).ToArray();
            var dublicate = debited_list.Select(i => i.PagingToken.Split('-')[0]).Distinct().ToList();
            dublicate.AddRange(credited_list.Select(i => i.PagingToken.Split('-')[0]).Distinct());
            dublicate = dublicate.Distinct().ToList();
            var trade = list.OfType<TradeEffectResponse>().ToList();
            trade.RemoveAll(i => dublicate.Contains(i.PagingToken.Split('-')[0]));

            var credited = credited_list.Sum(i => double.Parse(i.Amount));
            var debited = debited_list.Sum(i => double.Parse(i.Amount));
            var bought = trade.Where(i => i.BoughtAssetCode == asset && i.BoughtAssetIssuer == asset_owner).Sum(i => double.Parse(i.BoughtAmount));
            var sold = trade.Where(i => i.SoldAssetCode == asset && i.SoldAssetIssuer == asset_owner).Sum(i => double.Parse(i.SoldAmount));
            return bought - sold + credited - debited;
        }
        private async void btn_Click(object sender, RoutedEventArgs e)
        {
            //0153266749771751425
            var list = await get_effect(txt.Text, "0");
            var balance = get_balance(list, asset, asset_owner);
            lbl.Content = balance.ToString();
            Console.Beep();
        }
        //async Task<bool> buy_zachco(double amount, double price)
        //{
        //    Network.UsePublicNetwork();
        //    Server server = new Server("https://horizon.stellar.org");
        //    KeyPair sourceKeypair = KeyPair.FromSecretSeed("SCLY44ZFA7RHHVWVQ6QR7MP54VQEJ6T426MH5R3KO2EWLBSCECVS4YFI");
        //    AccountResponse sourceAccountResponse = await server.Accounts.Account(sourceKeypair.AccountId);
        //    Account sourceAccount = new Account(sourceKeypair.AccountId, sourceAccountResponse.SequenceNumber);
        //    Asset zachco = asset.Create(null, "Zachco", "GADCJW2WJ2ITHXQU3GS2K7XD3VDQ3HEZ7JK73DHJ3D7KRLX6XGE2I7QY");
        //    Asset usd = asset.Create(null, asset, asset_owner);

        //    //new ManageSellOfferOperation.Builder()
        //    var operation = new ManageSellOfferOperation.Builder(usd, zachco, amount.ToString(), (1 / price).ToString()).SetSourceAccount(sourceAccount.KeyPair).Build();

        //    //Create transaction and add the payment operation we created
        //    Transaction transaction = new TransactionBuilder(sourceAccount).AddOperation(operation).Build();

        //    //Sign Transaction
        //    transaction.Sign(sourceKeypair);
        //    try
        //    {
        //        var dv = await server.SubmitTransaction(transaction);
        //        return dv.IsSuccess();
        //    }
        //    catch (Exception exception)
        //    {
        //        log.error(exception.Message);
        //        return false;
        //    }
        //}
        //async Task<bool> sell_zachco(double amount, double price)
        //{
        //    Network.UsePublicNetwork();
        //    Server server = new Server("https://horizon.stellar.org");
        //    KeyPair sourceKeypair = KeyPair.FromSecretSeed("SCLY44ZFA7RHHVWVQ6QR7MP54VQEJ6T426MH5R3KO2EWLBSCECVS4YFI");
        //    AccountResponse sourceAccountResponse = await server.Accounts.Account(sourceKeypair.AccountId);
        //    Account sourceAccount = new Account(sourceKeypair.AccountId, sourceAccountResponse.SequenceNumber);
        //    Asset zachco = asset.Create(null, "Zachco", "GADCJW2WJ2ITHXQU3GS2K7XD3VDQ3HEZ7JK73DHJ3D7KRLX6XGE2I7QY");
        //    Asset usd = asset.Create(null, asset, asset_owner);

        //    //new ManageSellOfferOperation.Builder()
        //    var operation = new ManageSellOfferOperation.Builder(zachco, usd, amount.ToString(), price.ToString()).SetSourceAccount(sourceAccount.KeyPair).Build();

        //    //Create transaction and add the payment operation we created
        //    Transaction transaction = new TransactionBuilder(sourceAccount).AddOperation(operation).Build();

        //    //Sign Transaction
        //    transaction.Sign(sourceKeypair);
        //    try
        //    {
        //        var dv = await server.SubmitTransaction(transaction);
        //        return dv.IsSuccess();
        //    }
        //    catch (Exception exception)
        //    {
        //        log.error(exception.Message);
        //        return false;
        //    }
        //}
        //async void send()
        //{
        //    Network.UsePublicNetwork();
        //    //Set network and server
        //    Server server = new Server("https://horizon.stellar.org");



        //    KeyPair sourceKeypair = KeyPair.FromSecretSeed("SCLY44ZFA7RHHVWVQ6QR7MP54VQEJ6T426MH5R3KO2EWLBSCECVS4YFI");
        //    AccountResponse sourceAccountResponse = await server.Accounts.Account(sourceKeypair.AccountId);
        //    Account sourceAccount = new Account(sourceKeypair.AccountId, sourceAccountResponse.SequenceNumber);
        //    Asset asset = asset.Create(null, "Zachco", "GADCJW2WJ2ITHXQU3GS2K7XD3VDQ3HEZ7JK73DHJ3D7KRLX6XGE2I7QY");

        //    KeyPair destinationKeyPair = KeyPair.FromAccountId("GAB5NLGL3GE5PMLCQNMAEVSAKPIC7IAONJX7FMUGJFQDZY6LSZETLRCU");



        //    //Create payment operation
        //    PaymentOperation operation = new PaymentOperation.Builder(destinationKeyPair, asset, "1").SetSourceAccount(sourceAccount.KeyPair).Build();

        //    //Create transaction and add the payment operation we created
        //    Transaction transaction = new TransactionBuilder(sourceAccount).AddOperation(operation).Build();

        //    //Sign Transaction
        //    transaction.Sign(sourceKeypair);

        //    //Try to send the transaction
        //    try
        //    {
        //        await server.SubmitTransaction(transaction);
        //    }
        //    catch (Exception exception)
        //    {
        //        Console.WriteLine("Send Transaction Failed");
        //        Console.WriteLine("Exception: " + exception.Message);
        //    }
        //}
    }
}