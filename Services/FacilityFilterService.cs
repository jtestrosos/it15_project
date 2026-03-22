namespace production_system.Services
{
    /// <summary>
    /// Scoped service that holds the Superadmin's currently selected facility filter.
    /// When null, it means "All Facilities" (no filtering).
    /// </summary>
    public class FacilityFilterService
    {
        public int? SelectedFacilityId { get; set; }

        public event Action? OnChanged;

        public void SetFacility(int? facilityId)
        {
            SelectedFacilityId = facilityId;
            OnChanged?.Invoke();
        }
    }
}
