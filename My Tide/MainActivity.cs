using Android.App;
using Android.Widget;
using Android.OS;
using System;
using System.Threading.Tasks;
using System.Linq;
using Xamarin.Forms;
using Android.Views;

namespace My_Tide
{
    [Activity(Label = "My Tides", MainLauncher = true, Icon = "@drawable/app", Theme = "@style/MyTides")]
    public class MainActivity : Activity
    {
        //int count = 1;

        private Android.Widget.DatePicker datePicker;
        private Android.Widget.Button refreshButton;
        private TextView highTideOne;
        private TextView highTideTwo;
        private TextView lowTideOne;
        private TextView lowTideTwo;
        private TextView sunRiseView;
        private TextView sunSetView;
        private TextView highTideLabel;
        private Spinner spinner;
        private Core.TideData.Location location;
        private float textSizeForTides = 35;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            RequestWindowFeature(Android.Views.WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.Main);            
            WireObjects();
            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);
            ActionBar.Title = "My Tides";            
        }

        private void WireObjects()
        {
               
            this.refreshButton = FindViewById<Android.Widget.Button>(Resource.Id.RefreshButton);
            refreshButton.Click += (object sender, EventArgs e) => this.DoUpdate();
            this.datePicker = FindViewById<Android.Widget.DatePicker>(Resource.Id.datePicker1);
            this.highTideOne = FindViewById<TextView>(Resource.Id.highTide1); ;
            this.highTideTwo = FindViewById<TextView>(Resource.Id.highTide2);
            this.lowTideOne = FindViewById<TextView>(Resource.Id.lowTide1);
            this.lowTideTwo = FindViewById<TextView>(Resource.Id.lowTide2);
            this.sunSetView = FindViewById<TextView>(Resource.Id.sunSet);
            this.sunRiseView = FindViewById<TextView>(Resource.Id.sunRise);
            this.highTideLabel = FindViewById<TextView>(Resource.Id.highTideLabel);
            spinner = FindViewById<Spinner>(Resource.Id.spinner1);
            
            spinner.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
            var adapter = ArrayAdapter.CreateFromResource(
                    this, Resource.Array.location_array, Android.Resource.Layout.SimpleSpinnerItem);

            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spinner.Adapter = adapter;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menus, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.menu_clearcache)
            {
                var now = datePicker.DateTime;
                Task<Core.DailyData> loadTask = Core.TideData.LoadTideData(now, location);
            }

            //Toast.MakeText(this, "Action selected: " + item.ItemId,
            //    ToastLength.Short).Show();
            return base.OnOptionsItemSelected(item);
        }

        private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var v = spinner.GetItemAtPosition(e.Position).ToString();
            location = Core.TideData.GetLocationId(v);
            string toast = string.Format("The location chaged to {0}", v);
            Toast.MakeText(this, toast, ToastLength.Short).Show();
            this.DoUpdate();
        }

        private async void DoUpdate()
        {
            var now = datePicker.DateTime;
            Task<Core.DailyData> loadTask = Core.TideData.LoadTideData(now, location);
            var results = await loadTask;
            highTideLabel.Text = $"High Tides:    {results.Date.ToShortDateString()}";
            highTideOne.Text = GetTideText(results.FirstHighTide, results);
            highTideTwo.Text = GetTideText(results.SecondHighTide, results);
            lowTideOne.Text = GetTideText(results.FirstLowTide, results);
            lowTideTwo.Text = GetTideText(results.SecondLowTide, results);
            sunRiseView.Text = results.SunRise.Value.ToShortTimeString();
            sunSetView.Text = results.SunSet.Value.ToShortTimeString();            
        }

        private static string GetTideText(DateTime? time, Core.DailyData data)
        {
            if (!time.HasValue)
                return string.Empty;

            var str = time.Value.ToShortTimeString();

            if (data.Date.Date == DateTime.Now.Date && data.Times.Any())
            {
                var nextTime = data.Times.Where(x => x.HasValue && x.Value > DateTime.Now).OrderBy(x => x.Value).FirstOrDefault();
                if (nextTime == time)
                {
                    str = $"* {str} *";
                }
            }

            return str;
        }

        
    }
}

