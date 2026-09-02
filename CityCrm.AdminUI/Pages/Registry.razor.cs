using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using CityCrm.AdminUI.Models;
using System.Net.Http.Json;

namespace CityCrm.AdminUI.Pages
{
    public partial class Registry : ComponentBase
    {
        [Inject] private HttpClient Http { get; set; } = default!;
        [Inject] private IJSRuntime JS { get; set; } = default!;

        private List<Building> buildings = new();
        private HashSet<int> expandedBuildings = new();
        private Dictionary<int, int> selectedEntrances = new();

        private bool showBuildingModal = false;
        private bool showPremiseModal = false;
        private Building newBuilding = new();
        private Premise newPremise = new();

        [SupplyParameterFromQuery(Name = "highlight")]
        public int? HighlightBuildingId { get; set; }
        private int? currentlyHighlightedId = null;

        private bool showAdvancedFilters = false;
        private string filterSearch = "";
        private string filterBType = "";
        private string filterCondition = "";
        private string filterPType = "";
        private string filterPStatus = "";
        private string filterPOwnership = "";
        private double? filterMinArea = null;
        private double? filterMaxArea = null;

        protected override async Task OnInitializedAsync() => await LoadBuildings();

        private async Task HandleSearchEnter(KeyboardEventArgs e)
        {
            if (e.Key == "Enter") await ApplyFilters();
        }

        private async Task ApplyFilters() => await LoadBuildings();

        private async Task ResetFilters()
        {
            filterSearch = ""; filterBType = ""; filterCondition = "";
            filterPType = ""; filterPStatus = ""; filterPOwnership = "";
            filterMinArea = null; filterMaxArea = null;
            await LoadBuildings();
        }

        private async Task ApplyQuickFilter(string preset)
        {
            filterSearch = ""; filterBType = ""; filterCondition = "";
            filterPType = ""; filterPStatus = ""; filterPOwnership = "";
            filterMinArea = null; filterMaxArea = null;

            if (preset == "free_communal")
            {
                filterPOwnership = "Комунальна";
                filterPStatus = "Вільне";
            }
            else if (preset == "commerce")
            {
                filterPType = "Комерційна";
            }
            else if (preset == "problems")
            {
                filterPStatus = "Аварійне";
            }

            await LoadBuildings();
        }

        private async Task LoadBuildings()
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(filterSearch)) queryParams.Add($"search={Uri.EscapeDataString(filterSearch)}");
            if (!string.IsNullOrWhiteSpace(filterBType)) queryParams.Add($"bType={Uri.EscapeDataString(filterBType)}");
            if (!string.IsNullOrWhiteSpace(filterCondition)) queryParams.Add($"condition={Uri.EscapeDataString(filterCondition)}");
            if (!string.IsNullOrWhiteSpace(filterPType)) queryParams.Add($"pType={Uri.EscapeDataString(filterPType)}");
            if (!string.IsNullOrWhiteSpace(filterPStatus)) queryParams.Add($"pStatus={Uri.EscapeDataString(filterPStatus)}");
            if (!string.IsNullOrWhiteSpace(filterPOwnership)) queryParams.Add($"pOwnership={Uri.EscapeDataString(filterPOwnership)}");
            if (filterMinArea.HasValue) queryParams.Add($"minArea={filterMinArea.Value}");
            if (filterMaxArea.HasValue) queryParams.Add($"maxArea={filterMaxArea.Value}");

            var url = "api/buildings";
            if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

            var result = await Http.GetFromJsonAsync<List<Building>>(url);
            if (result != null)
            {
                buildings = result;

                if (HighlightBuildingId.HasValue)
                {
                    expandedBuildings.Clear();
                    expandedBuildings.Add(HighlightBuildingId.Value);
                    currentlyHighlightedId = HighlightBuildingId.Value;

                    StateHasChanged();
                    await Task.Delay(100);
                    await JS.InvokeVoidAsync("scrollToElement", $"building-{currentlyHighlightedId}");

                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(3500);
                        currentlyHighlightedId = null;
                        await InvokeAsync(StateHasChanged);
                    });
                }
            }
        }

        private void ToggleBuilding(int id)
        {
            if (expandedBuildings.Contains(id)) expandedBuildings.Remove(id);
            else { expandedBuildings.Clear(); expandedBuildings.Add(id); }
        }

        private void SelectEntrance(int buildingId, int entrance)
        {
            selectedEntrances[buildingId] = entrance;
        }

        private void ShowAddBuildingModal()
        {
            newBuilding = new Building 
            { 
                BuildingType = "Багатоповерхівка", 
                Condition = "В експлуатації" 
            };
            showBuildingModal = true;
        }

        private void ShowEditBuildingModal(Building b)
        {
            newBuilding = new Building
            {
                Id = b.Id, 
                BuildingType = b.BuildingType, 
                Condition = b.Condition,
                Lat = b.Lat, 
                Lng = b.Lng, 
                StreetId = b.StreetId,
                Street = b.Street,
                BuildingNumber = b.BuildingNumber, 
                BuildingLetter = b.BuildingLetter,
                BuildingBlock = b.BuildingBlock, 
                CoopNumber = b.CoopNumber,
                GeoJson = b.GeoJson, 
                Notes = b.Notes
            };
            showBuildingModal = true;
        }

        private async Task DeleteBuilding(int id)
        {
            if ((await Http.DeleteAsync($"api/buildings/{id}")).IsSuccessStatusCode)
                await LoadBuildings();
        }

        private void ShowAddPremiseModal(int buildingId)
        {
            newPremise = new Premise { BuildingId = buildingId, Type = "Житлова", Status = "Вільне", Ownership = "Комунальна" };
            showPremiseModal = true;
        }

        private void ShowEditPremiseModal(Premise p)
        {
            newPremise = new Premise
            {
                Id = p.Id, BuildingId = p.BuildingId, PremiseNumber = p.PremiseNumber,
                Entrance = p.Entrance, Floor = p.Floor, Area = p.Area, Type = p.Type,
                Status = p.Status, Ownership = p.Ownership, OwnerName = p.OwnerName,
                RegistrationDate = p.RegistrationDate, RentEndDate = p.RentEndDate, Notes = p.Notes,
                BusinessCategory = p.BusinessCategory, BusinessName = p.BusinessName,
                WorkingHours = p.WorkingHours, BusinessDescription = p.BusinessDescription,
                IsPublicVisible = p.IsPublicVisible
            };
            showPremiseModal = true;
        }

        private async Task DeletePremise(int id)
        {
            if ((await Http.DeleteAsync($"api/premises/{id}")).IsSuccessStatusCode)
            {
                await LoadBuildings();
                showPremiseModal = false;
            }
        }

        private async Task HandleBuildingSaved()
        {
            showBuildingModal = false;
            await LoadBuildings();
        }

        private async Task HandlePremiseSaved()
        {
            showPremiseModal = false;
            await LoadBuildings();
        }

        private void CloseBuildingModal() => showBuildingModal = false;
        private void ClosePremiseModal() => showPremiseModal = false;

        private string GetPremiseColorClass(string status) => status switch
        {
            "Вільне" => "bg-success border-success",
            "Аварійне" => "bg-danger border-danger",
            "Орендоване (Комерція)" => "bg-primary border-primary",
            "Орендоване (Соціальне)" => "bg-info border-info text-dark",
            "Службове" => "bg-warning border-warning text-dark",
            _ => "bg-secondary border-secondary"
        };

        private string GetBuildingConditionBadge(string condition) => condition switch
        {
            "В експлуатації" => "bg-success",
            "В процесі будівництва" => "bg-warning text-dark",
            "Зруйновано (бойові дії)" => "bg-danger",
            "Занедбана будівля" => "bg-secondary",
            _ => "bg-light text-dark"
        };
    }
}