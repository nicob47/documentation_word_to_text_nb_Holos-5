using H.Core.Emissions.Results;
using H.Core.Enumerations;

namespace H.Core.Models
{
    /// <summary>
    /// One "tank" of manure — a running balance of stored volume, N, C, and TAN for a single
    /// (<see cref="AnimalType"/>, year, <see cref="ManureStateType"/>) bucket. Created by
    /// <see cref="H.Core.Services.Animals.ManureService.Initialize"/> from per-day animal-
    /// emission rows; drawn down by field applications, grazing deposits, and exports during
    /// the analysis pass.
    ///
    /// <para><b>Daily-emission cache:</b></para>
    /// <see cref="_dailyResults"/> keeps the per-year list of <c>GroupEmissionsByDay</c> rows
    /// that fed this tank's totals, so downstream code that needs day-level detail (e.g.
    /// monthly spreading patterns) can read it without recomputing.
    /// </summary>
    public class ManureTank : StorageTankBase
    {
        #region Fields

        private Dictionary<int, List<GroupEmissionsByDay>> _dailyResults;

        private ManureStateType _manureStateType;
        private AnimalType _animalType;

        #endregion

        #region Constructors

        public ManureTank()
        {
            _dailyResults = new Dictionary<int, List<GroupEmissionsByDay>>();
        }

        #endregion

        #region Properties

        public AnimalType AnimalType
        {
            get => _animalType;
            set => SetProperty(ref _animalType, value);
        }

        public ManureStateType ManureStateType
        {
            get => _manureStateType;
            set => SetProperty(ref _manureStateType, value);
        }

        #endregion

        #region Public Methods

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(AnimalType)}: {AnimalType}";
        }

        public void AddDailyResultToTank(GroupEmissionsByDay groupEmissionsByDay)
        {
            var dayOfYear = groupEmissionsByDay.DateTime.DayOfYear;

            if (_dailyResults.ContainsKey(dayOfYear))
            {
                var resultsForDay = _dailyResults[dayOfYear];
                resultsForDay.Add(groupEmissionsByDay);
            }
            else
            {
                _dailyResults[dayOfYear] = new List<GroupEmissionsByDay>() {groupEmissionsByDay};
            }
        }

        public double GetVolumeCreatedOnDate(DateTime dateTime)
        {
            var result = 0d;

            var dayOfYear = dateTime.DayOfYear;
            if (_dailyResults.ContainsKey(dayOfYear))
            {
                var allDailyResults = _dailyResults[dayOfYear];
                return allDailyResults.Sum(x => x.TotalVolumeOfManureAvailableForLandApplicationInKilograms);
            }

            return result;
        }

        #endregion

        #region Event Handlers

        #endregion
    }
}