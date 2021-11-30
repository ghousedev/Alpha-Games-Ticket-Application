using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Mongo DB
using MongoDB.Driver;
using MongoDB.Bson;
using System.ComponentModel;
using Windows.UI.ViewManagement;

namespace Alpha_Games
{
    /// <summary>
    /// Main page of application
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // List to bind to the view
        List<Entity> Events = new List<Entity>();
        // Get the event collection for UI data and to modify document fields when needed
        private IMongoCollection<Entity> GetDbCollection()
        {
            IMongoCollection<Entity> collection = App.dbClient.GetDatabase("content").GetCollection<Entity>("events");
            return collection;
        }
        // Get the event id from the row that was clicked
        private string GetCurrentItemId(RoutedEventArgs e)
        {
            Button button = (Button)e.OriginalSource;
            object dataContext = button.DataContext;
            Entity thisEvent = dataContext as Entity;
            string id = thisEvent.Id;
            return id;
        }
        // Filter to select the document with the matching id field
        private FilterDefinition<Entity> FilterById(string id)
        {
            FilterDefinition<Entity> filter = Builders<Entity>.Filter.Eq("id", id);
            return filter;
        }
        // Update the soldtickets field
        private UpdateDefinition<Entity> UpdateSoldTickets(int incrementValue)
        {
            UpdateDefinition<Entity> update = Builders<Entity>.Update.Inc("soldtickets", incrementValue);
            return update;
        }
        // + clicked, sold a ticket manually so increment the database to update available tickets
        private void PlusTicket_Click(object sender, RoutedEventArgs e)
        {
            string id = GetCurrentItemId(e);
            // Update the database
            GetDbCollection().UpdateOne(FilterById(id), UpdateSoldTickets(1));
            // Update the value for the view
            foreach (Entity doc in Events)
            {
                if (doc.Id == id)
                {
                    doc.soldtickets++;
                }
            }
        }
        // - clicked, used if need to reduce number of tickets sold, cancellation etc
        private void MinusTicket_Click(object sender, RoutedEventArgs e)
        {
            string id = GetCurrentItemId(e);
            // Update the database
            GetDbCollection().UpdateOne(FilterById(id), UpdateSoldTickets(-1));
            // Update the value for the view
            foreach (Entity doc in Events)
            {
                if (doc.Id == id)
                {
                    doc.soldtickets--;
                }
            }
        }
        // Refresh the page
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateUi();
        }
        // Used on page load and value change
        private void PopulateUi()
        {
            // Get the event collection and make a list from it
            Events = GetDbCollection().Find(new BsonDocument()).ToList();
            // Modify the date string for display
            foreach (Entity doc in Events)
            {
                var formattedDate = doc.start_time.Split("T");
                doc.start_time = formattedDate[0];
            }
            // Give the data to the view to display
            eventsListView.ItemsSource = Events;
        }
        public MainPage()
        {
            this.InitializeComponent();
            // Make the window fit the layout better
            ApplicationView.PreferredLaunchViewSize = new Size(1000, 800);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            PopulateUi();
        }
    }
    // Data class to hold data from mongodb
    [MongoDB.Bson.Serialization.Attributes.BsonIgnoreExtraElements]
    public class Entity : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Id { get; set; }
        public string description { set; get; }
        public string end_time { set; get; }
        public string price { set; get; }
        public string start_time { set; get; }
        public string name { set; get; }
        public string imageurl { set; get; }
        public string availabletickets { set; get; }
        private int _soldtickets;
        public int soldtickets
        {
            get => _soldtickets;
            set
            {
                _soldtickets = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("soldtickets"));
            }
        }
    }
}