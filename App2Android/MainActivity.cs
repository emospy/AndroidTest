using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Javax.Crypto;
using Javax.Crypto.Spec;
using System.Text;
using System.Security.Cryptography;
using Android.Graphics;

namespace App2Android
{

    /// <summary>
    /// To do
    ///  When dead return to the beginning with start over
    ////- да направя системата за крайните калкулации (Ето това не съм го мислил още, а е готино и за мен е било причина да преиграя такава игра).
    ////- да направя новата игра;
    ////- да я тествам
    ////- да мога да поставя фоново изображение
    /// </summary>
    [Activity(Label = "App2Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private static readonly byte[] salt = Encoding.ASCII.GetBytes("Xamarin.iOS Version: 7.0.6.168");

        int count = 1;

        #region XML Constants

        const string Health = "Здраве";
        const string Strength = "Сила";

        const int MinDice = 2;
        const int MaxDice = 12;
        #endregion

        bool isLoadedGame = false;

        List<int> lstEpizodesQueue = new List<int>();

        SaveGameData Game;

        Game GameSource;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            XmlSerializer serializer = new XmlSerializer(typeof(Game));

            var sr = new StreamReader(Assets.Open("game.xml"));
            this.GameSource = (Game)serializer.Deserialize(sr);
            sr.Close();

            this.Game = new SaveGameData();
            this.isLoadedGame = this.LoadGame("Autosave.xml");
            if (isLoadedGame == false)
            {
                this.InitializeGame();
            }
            this.ExecuteEpizode(this.Game.CurrentEpizode);
        }

        public Bitmap bytesToUIImage(byte[] bytes)
        {
            if (bytes == null)
                return null;

            Bitmap bitmap;

            var documentsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            //Create a folder for the images if not exists
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(documentsFolder, "images"));

            string imatge = System.IO.Path.Combine(documentsFolder, "images", "image.jpg");

            System.IO.File.WriteAllBytes(imatge, bytes.Concat(new Byte[] { (byte)0xD9 }).ToArray());

            bitmap = BitmapFactory.DecodeFile(imatge);

            System.IO.File.Delete(imatge);

            return bitmap;
        }

        private void ExecuteEpizode(int EpizodeNumber)
        {
            var epizode = GetEpizode(EpizodeNumber);
            if (epizode == null)
            {
                return;
            }

            

            if (this.isLoadedGame == false)
            {
                this.AddRemoveItems(epizode);
                this.AddRemoveStats(epizode);
            }

            this.Game.CurrentEpizode = EpizodeNumber;

            var layout = new LinearLayout(this);
            layout.Orientation = Orientation.Vertical;

            this.PrepareImage(epizode, layout);

            this.PrepareText(epizode, layout);

            if (CheckIfDead())
            {
                var choice = new Decision();
                choice.Text = "Продължи";
                choice.GoTo = 1001;
                this.CreateButton(choice, layout);
            }
            else
            {
                this.PrepareChoices(epizode, layout);
            }

            this.isLoadedGame = false;

            this.RefreshStats();

            ScrollView scroll = FindViewById<ScrollView>(Resource.Id.scrollView);
            scroll.RemoveAllViews();
            scroll.AddView(layout);

            this.SaveGame("Autosave.xml");

            this.lstEpizodesQueue.Add(EpizodeNumber);
        }

        private void PrepareImage(EpizodeXML epizode, LinearLayout lay)
        {
            if (epizode.image == null)
            {
                return;
            }
            var bmp = this.bytesToUIImage(epizode.image);

            ImageView ivue = new ImageView(this);
            ivue.SetImageBitmap(bmp);
            lay.AddView(ivue);
        }

        private bool CheckIfDead()
        {
            var item = this.Game.lstStats.Find(n => n.Name == Health);
            if (item != null)
            {
                if (item.Value <= 0)
                {
                    return true;
                }
            }
            return false;
        }

        private void InitializeGame()
        {
            this.Game.CurrentEpizode = 1;
            this.Game.lstStats = new List<PersonStats>();
            this.Game.lstInventory = new List<Inventory>();

            Random rand = new Random(DateTime.Now.Second);

            var stats = this.GameSource.lstStats;
            foreach (var stat in stats)
            {
                var name = stat.Name;
                var min = stat.Min;
                var max = stat.Max;
                var val = rand.Next(min, max);
                this.Game.lstStats.Add(new PersonStats { Name = name, Value = val });
                this.Game.lstStats.Add(new PersonStats { Name = "Initial" + name, Value = val });
            }
        }

        private void RefreshStats()
        {
            LinearLayout txtStats = FindViewById<LinearLayout>(Resource.Id.layoutStats);

            txtStats.RemoveAllViews();

            foreach (var s in this.Game.lstStats)
            {
                if (s.Name.ToLower().StartsWith("initial"))
                {

                }
                else
                {
                    string txt = "";
                    var label = new TextView(this);
                    txt += s.Name;
                    txt += ": ";
                    txt += s.Value;
                    label.Text = txt;
                    if(s.IsRed)
                    {
                        s.IsRed = false;
                        label.SetTextColor(Color.Red);
                    }
                    if (s.IsGreen)
                    {
                        s.IsGreen = false;
                        label.SetTextColor(Color.Green);
                    }
                    label.SetPadding(10, 0, 50, 0);
                    label.SetTextSize(Android.Util.ComplexUnitType.Dip, 14);
                    label.SetTypeface(Typeface.Default, TypefaceStyle.Bold);
                    txtStats.AddView(label);
                }
            }

            if (lstEpizodesQueue.Count > 0)
            {
                Button btn = new Button(this);
                btn.Click += this.ClickBack;
                btn.Text = "Back";
                btn.Tag = lstEpizodesQueue.Last();
                btn.SetTextSize(Android.Util.ComplexUnitType.Dip, 14);
                txtStats.AddView(btn);
            }
        }

        private void ResetStat(string name, int qty)
        {
            var item = this.Game.lstStats.Find(n => n.Name == name);
            if (item != null)
            {
                if(item.Value < qty)
                {
                    item.IsGreen = true;
                }
                else if(item.Value > qty)
                {
                    item.IsRed = true;
                }
                item.Value = qty;
            }
        }

        private void RemoveStat(string name, int qty)
        {
            var item = this.Game.lstStats.Find(n => n.Name == name);
            if (item != null)
            {
                item.Value -= qty;
                if (item.Value < 0)
                {
                    item.Value = 0;
                }
                item.IsRed = true;
            }
        }

        private void AddStat(string name, int qty)
        {
            var item = this.Game.lstStats.Find(n => n.Name == name);
            if (item != null)
            {
                item.Value += qty;
                item.IsGreen = true;
            }
            else
            {
                this.Game.lstStats.Add(new PersonStats { Name = name, Value = qty });
            }
        }

        private void RemoveInventoryItem(string name, int qty)
        {
            var item = this.Game.lstInventory.Find(n => n.Name == name);
            if (item != null)
            {
                item.Quantity -= qty;
                if (item.Quantity < 0)
                {
                    //MessageBox.Show("Грешка в играта. Позволено ви беше да използвате повече предмети от колкото имате налични. Въпреки това можете да продължите да играете нормално. Моля, уведомете авторите за отстраняване на грешката.");
                }
            }
            else
            {
                // MessageBox.Show("Грешка в играта. Позволено ви беше да използвате повече предмети от колкото имате налични. Въпреки това можете да продължите да играете нормално. Моля, уведомете авторите за отстраняване на грешката.");
            }
        }

        private void AddInventoryItem(string name, int qty)
        {
            var item = this.Game.lstInventory.Find(n => n.Name == name);
            if (item != null)
            {
                item.Quantity += qty;
            }
            else
            {
                this.Game.lstInventory.Add(new Inventory { Name = name, Quantity = qty });
            }
        }

        private void AddRemoveStats(EpizodeXML epizode)
        {
            var stats = epizode.Stats;
            foreach (var stat in stats)
            {
                var name = stat.Name;
                int qty = stat.Quantity;

                if (stat.Reset == true)
                {
                    int resetqty = Game.lstStats.Find(s => s.Name == "Initial" + stat.Name).Value;
                   
                    this.ResetStat(stat.Name, resetqty);
                }
                else
                {
                    if (stat.Action == true)
                    {
                        this.AddStat(name, qty);
                    }
                    else
                    {
                        this.RemoveStat(name, qty);
                    }
                }
            }
        }

        private void AddRemoveItems(EpizodeXML epizode)
        {
            var invs = epizode.Inventories;
            foreach (var inv in invs)
            {
                var name = inv.Name;
                int qty = inv.Quantity;
                if (inv.Action == true)
                {
                    AddInventoryItem(name, qty);
                }
                else
                {
                    RemoveInventoryItem(name, qty);
                }
            }
        }

        private void PrepareChoices(EpizodeXML epizode, LinearLayout lay)
        {
            this.PrepareDecisions(epizode, lay);
            this.PrepareChances(epizode, lay);
            this.PrepareBattle(epizode, lay);
            this.PrepareConditions(epizode, lay);
        }

        private void PrepareConditions(EpizodeXML epizode, LinearLayout lay)
        {
            var conditions = epizode.Choices.Conditions;
            foreach (var cond in conditions)
            {
                var predicates = cond.Predicates;
                var pass = true;
                foreach (var pred in predicates)
                {
                    var type = pred.Type;
                    var name = pred.Name;
                    bool aval = pred.IsAvailable;
                    switch (pred.Type)
                    {
                        case PredicateTypes.eInventory:
                            {
                                var inv = this.Game.lstInventory.Find(i => i.Name == name);

                                if (aval == true)
                                {
                                    int qty = pred.Quantity;

                                    if (inv == null || inv.Quantity < qty)
                                    {
                                        pass = false;
                                    }
                                }
                                else
                                {
                                    if (inv != null && inv.Quantity != 0)
                                    {
                                        pass = false;
                                    }
                                }
                                break;
                            }
                        case PredicateTypes.eStat:
                            {
                                var inv = this.Game.lstStats.Find(i => i.Name == name);
                                int qty = pred.Quantity;
                                if (aval == true)
                                {
                                    if (inv == null || inv.Value < qty)
                                    {
                                        pass = false;
                                    }
                                }
                                else
                                {
                                    if (inv != null && inv.Value > qty)
                                    {
                                        pass = false;
                                    }
                                }
                                break;
                            }
                    }
                }
                if (pass)
                {
                    this.CreateButton(cond, lay);
                    break;
                }
            }
        }

        private void PrepareChances(EpizodeXML epizode, LinearLayout lay)
        {
            if (epizode.Choices != null)
            {
                Random rand = new Random(DateTime.Now.Second);
                var r = rand.NextDouble();
                var chances = epizode.Choices.Chances;

                var cnt = chances.Count;
                double tillNow = 0;

                foreach (var chance in chances)
                {
                    tillNow += chance.Probability;//double.Parse(chance.Attribute(Probability).Value, System.Globalization.NumberStyles.AllowDecimalPoint);
                    if (r < tillNow)
                    {
                        Button btn = new Button(this);
                        btn.Click += this.Click;
                        btn.Text = chance.Text;
                        btn.Tag = chance.GoTo;
                        btn.SetTextSize(Android.Util.ComplexUnitType.Dip, 14);
                        lay.AddView(btn);
                        break;
                    }
                }
            }
        }

        private void CreateBattleButton(Battle battle, LinearLayout lay)
        {
            Button btn = new Button(this);
            btn.Click += this.Click;
            btn.Text = battle.Text;

            var es = battle.EnemyStrength;
            var eh = battle.EnemyHealth;

            var MyHealth = this.Game.lstStats.Find(f => f.Name == Health);
            var health = MyHealth.Value;
            var strength = this.Game.lstStats.Find(f => f.Name == Strength).Value;

            Random rand = new Random(DateTime.Now.Second);

            while (health > 0 && eh > 0)
            {
                var myHit = rand.Next(MinDice, MaxDice);
                var enemyHit = rand.Next(MinDice, MaxDice);

                myHit += strength + myHit;
                enemyHit += es + enemyHit;
                if (myHit > enemyHit)
                {
                    eh -= 2;
                }
                else
                {
                    health -= 2;
                }
            }

            if (health > eh)
            {
                MyHealth.Value = health;
                btn.Tag = battle.GoTo;
            }
            else
            {
                MyHealth.Value = health;
                btn.Tag = battle.Lose;
            }
            lay.AddView(btn);
        }

        private void CreateButton(Decision chance, LinearLayout lay)
        {

            Button btn = new Button(this);
            btn.Click += this.Click;
            btn.Text = chance.Text;
            btn.Tag = chance.GoTo;
            btn.SetTextSize(Android.Util.ComplexUnitType.Dip, 14);
            lay.AddView(btn);
        }

        private void CreateRollBackButton(Decision chance, LinearLayout lay)
        {

            Button btn = new Button(this);
            btn.Click += this.ClickRollBack;
            btn.Text = chance.Text;
            btn.Tag = chance.GoTo;
            btn.SetTextSize(Android.Util.ComplexUnitType.Dip, 14);
            lay.AddView(btn);
        }

        private void PrepareDecisions(EpizodeXML epizode, LinearLayout lay)
        {
            if (epizode.Choices != null)
            {
                var choices = epizode.Choices.Decisions;
                foreach (var choice in choices)
                {
                    this.CreateButton(choice, lay);
                }
            }
        }

        private void PrepareText(EpizodeXML epizode, LinearLayout lay)
        {
            TextView ivue = new TextView(this);
            ivue.Text = Crypto.Decrypt(epizode.Text, "pass");
            ivue.SetPadding(10, 0, 10, 0);
            ivue.SetTextSize(Android.Util.ComplexUnitType.Dip, 14);
            lay.AddView(ivue);
        }

        private void PrepareBattle(EpizodeXML epizode, LinearLayout lay)
        {
            var choices = epizode.Choices.Battles;
            foreach (var choice in choices)
            {
                this.CreateBattleButton(choice, lay);
            }
        }

        private EpizodeXML GetEpizode(int EpizodeNumber)
        {
            var epizode = this.GameSource.lstEpizodes.Where(e => e.ID == EpizodeNumber).FirstOrDefault();
            return epizode;
        }

        private void Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            ExecuteEpizode((int)b.Tag);
        }

        private void ClickBack(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            this.lstEpizodesQueue.Remove(lstEpizodesQueue.Last());
            this.lstEpizodesQueue.Remove(lstEpizodesQueue.Last());
            ExecuteEpizode((int)b.Tag);
        }

        private void ClickRollBack(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            ExecuteEpizode((int)b.Tag);
        }

        //private void Save_Click_1(object sender, RoutedEventArgs e)
        //{
        //    var ofd = new SaveFileDialog();
        //    Nullable<bool> result = ofd.ShowDialog();

        //    if (result == true)
        //    {
        //        string file = ofd.FileName;
        //        SaveGame(file);
        //    }
        //}

        private void SaveGame(string file)
        {
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string filename = System.IO.Path.Combine(path, file);
            
            TextWriter writer = new StreamWriter(filename, false);
            
            writer.WriteLine("CurrentEpizode=" + Game.CurrentEpizode);
            foreach(var inv in Game.lstInventory)
            {
                writer.WriteLine("Inventory=" + inv.Name + "=" + inv.Quantity);
            }
            foreach(var stat in Game.lstStats)
            {
                writer.WriteLine("Stat=" + stat.Name + "=" + stat.Value);
            }

            writer.Close();
        }

        //private void Load_Click_1(object sender, RoutedEventArgs e)
        //{
        //    var ofd = new OpenFileDialog();
        //    Nullable<bool> result = ofd.ShowDialog();
        //    string file = ofd.FileName;
        //    if (result == true)
        //    {
        //        LoadGame(file);
        //    }
        //    this.ExecuteEpizode(this.Game.CurrentEpizode);
        //}

        private bool LoadGame(string file)
        {
            string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            string filename = System.IO.Path.Combine(path, file);

            if (System.IO.File.Exists(filename) == true)
            {
                MemoryStream ms = new MemoryStream();
                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                try
                {
                    fs.CopyTo(ms);
                    ms.Flush();
                    var sr = new StreamReader(fs);
                    string line;
                    this.Game = new SaveGameData();
                    this.Game.lstInventory = new List<Inventory>();
                    this.Game.lstStats = new List<PersonStats>();

                    string Arr = System.Text.Encoding.UTF8.GetString(ms.ToArray());
                    var Lines = Arr.Split(new char[] { '\n'});
                    int i = Lines.Length;
                    
                    for (int j = 0; j < i; j++ )
                    {
                        var linarr = Lines[j].Split(new char[] { '=' });
                        switch (linarr[0])
                        {
                            case "CurrentEpizode":
                                this.Game.CurrentEpizode = int.Parse(linarr[1]);
                                break;
                            case "Inventory":
                                var inv = new Inventory();
                                inv.Name = linarr[1];
                                inv.Quantity = int.Parse(linarr[2]);
                                this.Game.lstInventory.Add(inv);
                                break;
                            case "Stat":
                                var stat = new PersonStats();
                                stat.Name = linarr[1];
                                stat.Value = int.Parse(linarr[2]);
                                this.Game.lstStats.Add(stat);
                                break;
                        }
                    }
                    fs.Close();
                    if(this.Game.CurrentEpizode == 0)
                    {
                        this.Game.CurrentEpizode = 1;
                    }
                    
                    this.isLoadedGame = true;
                    return true;
                }
                catch (Exception ex)
                {
                    fs.Close();
                    //StringReader tr = new StringReader();
                }
            }
            return false;
        }

        //private void Exit_Click_1(object sender, RoutedEventArgs e)
        //{
        //    this.Close();
        //}

        //private void GameInfo_Click_1(object sender, RoutedEventArgs e)
        //{
        //    Window win = new Window();
        //    var Grid = new Grid();
        //    Grid.ColumnDefinitions.Add(new ColumnDefinition());
        //    Grid.ColumnDefinitions.Add(new ColumnDefinition());
        //    int row = 0;
        //    foreach (var stat in this.Game.lstStats)
        //    {
        //        Grid.RowDefinitions.Add(new RowDefinition());
        //        var label = new Label { Content = stat.Name, HorizontalAlignment = System.Windows.HorizontalAlignment.Right };
        //        Grid.Children.Add(label);
        //        Grid.SetRow(label, row);
        //        Grid.SetColumn(label, 0);

        //        var val = new Label { Content = stat.Value, HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
        //        Grid.Children.Add(val);
        //        Grid.SetRow(val, row);
        //        Grid.SetColumn(val, 1);
        //        row++;
        //    }

        //    foreach (var stat in this.Game.lstInventory)
        //    {
        //        Grid.RowDefinitions.Add(new RowDefinition());
        //        var label = new Label { Content = stat.Name, HorizontalAlignment = System.Windows.HorizontalAlignment.Right };
        //        Grid.Children.Add(label);
        //        Grid.SetRow(label, row);
        //        Grid.SetColumn(label, 0);

        //        var val = new Label { Content = stat.Quantity, HorizontalAlignment = System.Windows.HorizontalAlignment.Left };
        //        Grid.Children.Add(val);
        //        Grid.SetRow(val, row);
        //        Grid.SetColumn(val, 1);
        //        row++;
        //    }
        //    win.Content = Grid;
        //    win.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;
        //    win.ShowDialog();
        //}

        //private void NewGame_Click_1(object sender, RoutedEventArgs e)
        //{
        //    this.InitializeGame();
        //    this.ExecuteEpizode(this.Game.CurrentEpizode);
        //}
    }

    static internal class Crypto
    {
        // Define the secret salt value for encrypting data
        private static readonly byte[] salt = Encoding.ASCII.GetBytes("Xamarin.iOS Version: 7.0.6.168");

        /// <summary>
        /// Takes the given text string and encrypts it using the given password.
        /// </summary>
        /// <param name="textToEncrypt">Text to encrypt.</param>
        /// <param name="encryptionPassword">Encryption password.</param>
        internal static string Encrypt(string textToEncrypt, string encryptionPassword)
        {
            var algorithm = GetAlgorithm(encryptionPassword);

            //Anything to process?
            if (textToEncrypt == null || textToEncrypt == "") return "";

            byte[] encryptedBytes;
            using (ICryptoTransform encryptor = algorithm.CreateEncryptor(algorithm.Key, algorithm.IV))
            {
                byte[] bytesToEncrypt = Encoding.UTF8.GetBytes(textToEncrypt);
                encryptedBytes = InMemoryCrypt(bytesToEncrypt, encryptor);
            }
            return Convert.ToBase64String(encryptedBytes);
        }

        /// <summary>
        /// Takes the given encrypted text string and decrypts it using the given password
        /// </summary>
        /// <param name="encryptedText">Encrypted text.</param>
        /// <param name="encryptionPassword">Encryption password.</param>
        internal static string Decrypt(string encryptedText, string encryptionPassword)
        {
            var algorithm = GetAlgorithm(encryptionPassword);

            //Anything to process?
            if (encryptedText == null || encryptedText == "") return "";

            byte[] descryptedBytes;
            using (ICryptoTransform decryptor = algorithm.CreateDecryptor(algorithm.Key, algorithm.IV))
            {
                byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                descryptedBytes = InMemoryCrypt(encryptedBytes, decryptor);
            }
            return Encoding.UTF8.GetString(descryptedBytes);
        }

        /// <summary>
        /// Performs an in-memory encrypt/decrypt transformation on a byte array.
        /// </summary>
        /// <returns>The memory crypt.</returns>
        /// <param name="data">Data.</param>
        /// <param name="transform">Transform.</param>
        private static byte[] InMemoryCrypt(byte[] data, ICryptoTransform transform)
        {
            MemoryStream memory = new MemoryStream();
            using (Stream stream = new CryptoStream(memory, transform, CryptoStreamMode.Write))
            {
                stream.Write(data, 0, data.Length);
            }
            return memory.ToArray();
        }

        /// <summary>
        /// Defines a RijndaelManaged algorithm and sets its key and Initialization Vector (IV) 
        /// values based on the encryptionPassword received.
        /// </summary>
        /// <returns>The algorithm.</returns>
        /// <param name="encryptionPassword">Encryption password.</param>
        private static RijndaelManaged GetAlgorithm(string encryptionPassword)
        {
            // Create an encryption key from the encryptionPassword and salt.
            var key = new Rfc2898DeriveBytes(encryptionPassword, salt);

            // Declare that we are going to use the Rijndael algorithm with the key that we've just got.
            var algorithm = new RijndaelManaged();
            int bytesForKey = algorithm.KeySize / 8;
            int bytesForIV = algorithm.BlockSize / 8;
            algorithm.Key = key.GetBytes(bytesForKey);
            algorithm.IV = key.GetBytes(bytesForIV);
            return algorithm;
        }

    }
}

