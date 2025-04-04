using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Jellee.Pages
{
    public partial class RestaurantsPage : ContentPage
    {
        private const string GOOGLE_PLACES_API_KEY = ""; // Of course move this into something more secure.

        private readonly HttpClient _httpClient;

        private ObservableCollection<Restaurant> _restaurants;
        private bool _isLoading;

        public ObservableCollection<Restaurant> Restaurants
        {
            get => _restaurants;
            set
            {
                _restaurants = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public RestaurantsPage()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await FetchNearbyRestaurants();
        }

        private async Task<string> GetPlacePhoto(string photoReference)
        {
            if (string.IsNullOrEmpty(photoReference))
            {
                Debug.WriteLine("photoReference is null or empty");
                return null;
            }

            const int maxWidth = 400;
            var url = $"https://maps.googleapis.com/maps/api/place/photo?maxwidth={maxWidth}&photo_reference={photoReference}&key={GOOGLE_PLACES_API_KEY}";

            Debug.WriteLine($"Generated Photo URL: {url}");

            return url;
        }

        private async Task FetchNearbyRestaurants()
        {
            try
            {

                if (Restaurants == null)
                {
                    Restaurants = new ObservableCollection<Restaurant>();
                }

                IsLoading = true;
                Debug.WriteLine("Fetching restaurants...");

                var location = await GetCurrentLocation();
                if (location == null)
                {
                    await DisplayAlert("Permission Denied", "Location access is required to find restaurants", "OK");
                    return;
                }

                double latitude = location.Latitude;
                double longitude = location.Longitude;

                var placesResponse = await _httpClient.GetAsync(
                    $"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={latitude},{longitude}&radius=5000&type=restaurant&key={GOOGLE_PLACES_API_KEY}");

                Debug.WriteLine($"API response status: {placesResponse.StatusCode}");

                if (!placesResponse.IsSuccessStatusCode)
                {
                    await DisplayAlert("Error", "Failed to fetch places data", "OK");
                    return;
                }

                var placesData = await placesResponse.Content.ReadFromJsonAsync<GooglePlacesResponse>();
                Debug.WriteLine($"API data fetched: {placesData?.Results?.Count} restaurants found.");

                var responseString = await placesResponse.Content.ReadAsStringAsync();
                Debug.WriteLine(responseString);

                if (placesData?.Results != null && placesData.Results.Any())
                {
                    var restaurantTasks = placesData.Results.Take(15).Select(async place =>
                    {
                        var photoUrl = place.Photos?.FirstOrDefault()?.PhotoReference != null
                            ? await GetPlacePhoto(place.Photos.FirstOrDefault()?.PhotoReference)
                            : null;

                        return new Restaurant
                        {
                            Id = place.PlaceId ?? "Unknown",
                            Name = place.Name ?? "Unnamed Restaurant",
                            Address = place.Vicinity ?? "No Address",
                            CuisineType = place.Types?.FirstOrDefault() ?? "Restaurant",
                            Latitude = place.Geometry?.Location?.Lat ?? 0,
                            Longitude = place.Geometry?.Location?.Lng ?? 0,
                            OverallRating = new Rating
                            {
                                Average = place.Rating ?? 0
                            },
                            PhotoUrl = photoUrl
                        };
                    }).ToList();

                    var restaurantList = await Task.WhenAll(restaurantTasks);

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        Debug.WriteLine($"Adding {restaurantList.Length} restaurants to the list.");
                        Restaurants.Clear();
                        foreach (var restaurant in restaurantList)
                        {
                            Restaurants.Add(restaurant);
                        }
                        OnPropertyChanged(nameof(Restaurants));
                    });

                    IsLoading = false;
                }
                else
                {
                    Debug.WriteLine("No restaurants found or the API returned an empty list.");
                    await DisplayAlert("No Results", "No nearby restaurants found.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                await DisplayAlert("Error", $"Could not fetch restaurants: {ex.Message}", "OK");
            }
            finally
            {
                Debug.WriteLine("Finished fetching.");
                IsLoading = false;
            }
        }

        private async Task<Microsoft.Maui.Devices.Sensors.Location> GetCurrentLocation()
        {
            try
            {
                var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                {
                    return null;
                }

                var location = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(30)
                });

                return location;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Location Error", ex.Message, "OK");
                return null;
            }
        }

        private async void OnRestaurantTapped(object sender, EventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is Restaurant restaurant)
            {
                await Shell.Current.GoToAsync($"restaurantdetails?id={restaurant.Id}");
            }
        }
    }

    public class GooglePlacesResponse
    {
        public List<PlaceResult> Results { get; set; }
    }

    public class PlaceResult
    {
        public string PlaceId { get; set; }
        public string Name { get; set; }
        public string Vicinity { get; set; }
        public double? Rating { get; set; }
        public Geometry Geometry { get; set; }
        public List<Photo> Photos { get; set; }
        public List<string> Types { get; set; }
    }

    public class Geometry
    {
        public Location Location { get; set; }
    }

    public class Location
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

    public class Photo
    {
        public string PhotoReference { get; set; }
    }

    public class Restaurant
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string CuisineType { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Rating OverallRating { get; set; }
        public string PhotoUrl { get; set; }
    }

    public class Rating
    {
        public double Average { get; set; }
    }
}