using H.Core.Calculators.Infrastructure;
using H.Core.Emissions.Results;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Models.Infrastructure;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Services.Animals
{
    /// <summary>
    /// Default <see cref="IDigestateService"/> implementation. Delegates the actual AD math to
    /// <see cref="ADCalculator"/> (which produces per-day digester outputs from the farm's
    /// substrate inputs) and aggregates the daily outputs into the per-year, per-state
    /// totals callers need.
    ///
    /// <para><b>Drawdown semantics:</b></para>
    /// <see cref="SubtractAmountsFromLandApplications"/> defaults to <c>true</c> — the
    /// "remaining at end of year" queries subtract amounts already applied to fields. Set to
    /// <c>false</c> when calling from contexts that need the gross amount produced (export
    /// view, audit reports). <see cref="SubtractAmountsFromImportedDigestateLandApplications"/>
    /// is the imported-digestate equivalent.
    /// </summary>
    public class DigestateService : IDigestateService
    {
        #region Fields

        /// <summary>
        /// On-farm digester output vs imported digestate. Most farms with AD components use
        /// on-farm; imported is for farms that buy digestate from a regional facility.
        /// </summary>
        private readonly List<ManureLocationSourceType> _validDigestateLocationSourceTypes = new List<ManureLocationSourceType>()
        {
            ManureLocationSourceType.NotSelected,
            ManureLocationSourceType.OnFarmAnaerobicDigestor,
            ManureLocationSourceType.Imported,
        };

        #endregion

        #region Properties

        /// <summary>
        /// Precomputed per-component animal results used as substrate input for the AD daily
        /// output run. Populated via <see cref="Initialize"/>; left at empty if not primed
        /// (lookups then short-circuit to zero).
        /// </summary>
        public List<AnimalComponentEmissionsResults> AnimalResults { get; set; }

        /// <summary>The actual AD-process calculator that consumes substrate inputs and produces daily digester outputs (gas, liquid, solid).</summary>
        public IADCalculator ADCalculator { get; set; }

        /// <summary>
        /// When <c>true</c> (default), "remaining at end of year" totals subtract amounts
        /// already applied to fields. Set <c>false</c> to see the gross stored amount.
        /// </summary>
        public bool SubtractAmountsFromLandApplications { get; set; }

        /// <summary>Same as <see cref="SubtractAmountsFromLandApplications"/> but for imported digestate.</summary>
        public bool SubtractAmountsFromImportedDigestateLandApplications { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default ctor — wires up a fresh <see cref="ADCalculator"/> and an empty
        /// <see cref="AnimalResults"/> list. Callers re-prime the service for each farm via
        /// <see cref="Initialize"/>.
        /// </summary>
        public DigestateService()
        {
            this.ADCalculator = new ADCalculator();
            this.AnimalResults = new List<AnimalComponentEmissionsResults>();

            this.SubtractAmountsFromLandApplications = true;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Stash the precomputed animal-component results so the AD calculator can read them
        /// as substrate inputs. Null-safe — replaces null with an empty list rather than
        /// throwing.
        /// </summary>
        public void Initialize(Farm farm, List<AnimalComponentEmissionsResults> animalComponentEmissionsResults)
        {
            this.AnimalResults = animalComponentEmissionsResults ?? new List<AnimalComponentEmissionsResults>();
        }

        /// <summary>
        /// Run the AD calculator and return the per-day digester output rows. Empty when the
        /// farm has no <see cref="AnaerobicDigestionComponent"/> — short-circuits before
        /// touching the calculator at all.
        /// </summary>
        public List<DigestorDailyOutput> GetDailyResults(Farm farm)
        {
            if (farm.AnaerobicDigestionComponents.Any() == false)
            {
                return new List<DigestorDailyOutput>();
            }

            var dailyResults = ADCalculator.CalculateResults(farm, this.AnimalResults);

            return dailyResults;
        }

        public List<ManureLocationSourceType> GetValidDigestateLocationSourceTypes()
        {
            return _validDigestateLocationSourceTypes;
        }

        /// <summary>
        /// Equation 4.6.1-4
        ///
        /// (kg N)
        /// </summary>
        public double GetTotalManureNitrogenRemainingForFarmAndYear(int year, Farm farm, List<DigestorDailyOutput> digestorDailyOutputs, DigestateState state)
        {
            var tankStates = this.GetDailyTankStates(farm, digestorDailyOutputs, year);
            if (tankStates.Any() == false || tankStates.Any(x => x.DateCreated.Year == year) == false)
            {
                // No management periods selected for input into AD system
                return 0;
            }

            var result = 0d;
            var amounts = new List<double>();
            switch (state)
            {
                case DigestateState.Raw:
                    {
                        amounts = tankStates.Where(x => x.DateCreated.Year == year).OrderBy(x => x.DateCreated).Select(x => x.NitrogenFromRawDigestate).ToList();

                        result = amounts.Last();

                        break;
                    }

                case DigestateState.SolidPhase:
                    {
                        amounts = tankStates.Where(x => x.DateCreated.Year == year).OrderBy(x => x.DateCreated).Select(x => x.NitrogenFromSolidDigestate).ToList();

                        result = amounts.Last();

                        break;
                    }

                case DigestateState.LiquidPhase:
                    {
                        amounts = tankStates.Where(x => x.DateCreated.Year == year).OrderBy(x => x.DateCreated).Select(x => x.NitrogenFromLiquidDigestate).ToList();

                        result = amounts.Last();

                        break;
                    }

                default:
                    {
                        result = 0;

                        break;
                    }
            }

            //var totalAvailableNitrogen = this.GetTotalNitrogenCreated(year);

            //var items = farm.GetCropViewItemsByYear(year, false);
            //var localSourcedNitrogenApplied = 0d;
            //var importedNitrogenApplied = 0d;
            //foreach (var cropViewItem in items)
            //{
            //    foreach (var manureApplicationViewItem in cropViewItem.GetLocalSourcedApplications(year))
            //    {
            //        localSourcedNitrogenApplied += manureApplicationViewItem.AmountOfNitrogenAppliedPerHectare * cropViewItem.Area;
            //    }

            //    foreach (var manureApplicationViewItem in cropViewItem.GetManureImportsByYear(year))
            //    {
            //        importedNitrogenApplied += manureApplicationViewItem.AmountOfNitrogenAppliedPerHectare * cropViewItem.Area;
            //    }
            //}

            //var totalAppliedNitrogen = localSourcedNitrogenApplied;//this.GetTotalNitrogenAppliedToAllFields(year);
            //var totalExportedNitrogen = this.GetTotalNitrogenFromExportedManure(year, farm);

            //// If all manure used was imported and none from local sources were used or created then there is no remaining N since all imports are used
            //if (totalAvailableNitrogen == 0 && totalAppliedNitrogen == 0 && importedNitrogenApplied > 0)
            //{
            //    return 0;
            //}

            //return totalAvailableNitrogen - (totalAppliedNitrogen - importedNitrogenApplied) - totalExportedNitrogen;

            return result;
        }

        /// <summary>
        /// Returns the date when the maximum amount of of digestate is available
        /// </summary>
        /// <param name="farm">The farm to consider</param>
        /// <param name="state">The <see cref="DigestateState"/> to consider</param>
        /// <param name="year">The year to be considered</param>
        /// <param name="digestorDailyOutputs">Daily amounts output from digestor</param>
        /// <param name="subtractFieldAppliedAmounts">Indicates if amounts used during field applications should be considered</param>
        /// <returns></returns>
        public DateTime GetDateOfMaximumAvailableDigestate(Farm farm,
            DigestateState state,
            int year,
            List<DigestorDailyOutput> digestorDailyOutputs,
            bool subtractFieldAppliedAmounts)
        {
            var tankStates = this.GetDailyTankStates(farm, digestorDailyOutputs, year);
            if (tankStates.Any() == false || tankStates.Any(x => x.DateCreated.Year == year) == false)
            {
                // No management periods selected for input into AD system
                return DateTime.Now;
            }

            var result = DateTime.Now;
            switch (state)
            {
                case DigestateState.Raw:
                    {
                        if (subtractFieldAppliedAmounts)
                        {
                            result = tankStates.Where(x => x.DateCreated.Year == year).OrderBy(x => x.TotalRawDigestateAvailable).Last().DateCreated;
                        }
                        else
                        {
                            result = tankStates.Where(x => x.DateCreated.Year == year).OrderBy(x => x.TotalRawDigestateProduced).Last().DateCreated;
                        }
                    }

                    break;

                case DigestateState.LiquidPhase:
                    {
                        if (subtractFieldAppliedAmounts)
                        {
                            result = tankStates.Where(x => x.DateCreated.Year == year).OrderBy(x => x.TotalLiquidDigestateAvailable).Last().DateCreated;
                        }
                        else
                        {
                            result = tankStates.Where(x => x.DateCreated.Year == year).OrderBy(x => x.TotalLiquidDigestateProduced).Last().DateCreated;
                        }
                    }

                    break;

                default:
                    {
                        if (subtractFieldAppliedAmounts)
                        {
                            result = tankStates.Where(x => x.DateCreated.Year == year).OrderBy(x => x.TotalSolidDigestateAvailable).Last().DateCreated;
                        }
                        else
                        {
                            result = tankStates.Where(x => x.DateCreated.Year == year).OrderBy(x => x.TotalSolidDigestateProduced).Last().DateCreated;
                        }
                    }

                    break;
            }

            return result;
        }

        public List<DigestateTank> GetDailyTankStates(Farm farm, List<DigestorDailyOutput> dailyOutputs, int year)
        {
            var component = farm.AnaerobicDigestionComponents.SingleOrDefault();
            if (component == null)
            {
                return new List<DigestateTank>();
            }

            return this.GetDailyTankStates(dailyOutputs, farm, component, year);
        }

        public DigestateTank GetTank(Farm farm, DateTime targetDate, List<DigestorDailyOutput> dailyOutputs)
        {
            var year = targetDate.Year;
            var tanks = this.GetDailyTankStates(farm, dailyOutputs, year);
            var result = tanks.SingleOrDefault(x => x.DateCreated.Date.Equals(targetDate.Date));
            if (result != null)
            {
                return result;
            }
            else
            {
                return new DigestateTank()
                {
                    DateCreated = targetDate,
                    Year = targetDate.Year,
                };
            }
        }

        /// <summary>
        /// Equation 4.9.7-2
        /// </summary>
        public List<DigestateTank> GetDailyTankStates(
            List<DigestorDailyOutput> digestorDailyOutputs,
            Farm farm,
            AnaerobicDigestionComponent component, int year)
        {
            var result = new List<DigestateTank>();

            var outputsForYear = digestorDailyOutputs.Where(x => x.Date.Year == year).OrderBy(y => y.Date).ToList();

            for (int i = 0; i < outputsForYear.Count; i++)
            {
                var outputOnCurrentDay = outputsForYear.ElementAt(i);
                var outputDate = outputOnCurrentDay.Date;

                var tank = new DigestateTank
                {
                    DateCreated = outputDate,
                };

                result.Add(tank);

                /*
                 * Calculate raw amounts available
                 */

                this.CalculateRawAmountsAvailable(
                    outputOnCurrentDay: outputOnCurrentDay,
                    outputDate: outputDate,
                    farm: farm,
                    outputNumber: i,
                    result: result,
                    tank: tank,
                    component: component);

                /*
                 * Calculate liquid amounts available
                 */

                this.CalculateLiquidAmountsAvailable(
                    outputOnCurrentDay: outputOnCurrentDay,
                    outputDate: outputDate,
                    farm: farm,
                    outputNumber: i,
                    result: result,
                    tank: tank,
                    component: component);

                /*
                 * Calculate solid amounts available
                 */

                this.CalculateSolidAmountsAvailable(
                    outputOnCurrentDay: outputOnCurrentDay,
                    outputDate: outputDate,
                    farm: farm,
                    outputNumber: i,
                    result: result,
                    tank: tank,
                    component: component);
            }

            return result;
        }

        /// <summary>
        /// Returns the total amount of N applied (to the entire field) from a digestate field application.
        /// 
        /// (kg N)
        /// </summary>
        public double CalculateTotalNitrogenFromDigestateApplication(
            CropViewItem cropViewItem,
            DigestateApplicationViewItem digestateApplicationViewItem,
            DigestateTank tank)
        {
            var amountApplied = digestateApplicationViewItem.AmountAppliedPerHectare;
            var totalAmountApplied = amountApplied * cropViewItem.Area;

            var totalDigestateCreatedOnDay = 0d;
            var totalNitrogenAvailableOnDay = 0d;
            switch (digestateApplicationViewItem.DigestateState)
            {
                case DigestateState.Raw:
                    totalDigestateCreatedOnDay = tank.TotalRawDigestateProduced;
                    totalNitrogenAvailableOnDay = tank.NitrogenFromRawDigestate;
                    break;

                case DigestateState.SolidPhase:
                    totalDigestateCreatedOnDay = tank.TotalSolidDigestateProduced;
                    totalNitrogenAvailableOnDay = tank.NitrogenFromSolidDigestate;
                    break;

                default:
                    totalDigestateCreatedOnDay = tank.TotalLiquidDigestateProduced;
                    totalNitrogenAvailableOnDay = tank.NitrogenFromLiquidDigestate;
                    break;
            }

            if (totalDigestateCreatedOnDay <= 0)
            {
                return 0;
            }

            var fraction = totalAmountApplied / totalDigestateCreatedOnDay;

            var amountOfNitrogen = fraction * totalNitrogenAvailableOnDay;

            return amountOfNitrogen;
        }

        /// <summary>
        /// (kg C)
        /// </summary>
        public double CalculateTotalCarbonFromDigestateApplication(
            CropViewItem cropViewItem,
            DigestateApplicationViewItem digestateApplicationViewItem,
            DigestateTank tank)
        {
            var amountApplied = digestateApplicationViewItem.AmountAppliedPerHectare;
            var totalAmountApplied = amountApplied * cropViewItem.Area;

            var totalDigestateCreatedOnDay = 0d;
            var totalCarbonAvailableOnDay = 0d;
            switch (digestateApplicationViewItem.DigestateState)
            {
                case DigestateState.Raw:
                    totalDigestateCreatedOnDay = tank.TotalRawDigestateProduced;
                    totalCarbonAvailableOnDay = tank.CarbonFromRawDigestate;
                    break;

                case DigestateState.SolidPhase:
                    totalDigestateCreatedOnDay = tank.TotalSolidDigestateProduced;
                    totalCarbonAvailableOnDay = tank.CarbonFromSolidDigestate;
                    break;

                default:
                    totalDigestateCreatedOnDay = tank.TotalLiquidDigestateProduced;
                    totalCarbonAvailableOnDay = tank.CarbonFromLiquidDigestate;
                    break;
            }

            if (totalDigestateCreatedOnDay <= 0)
            {
                return 0;
            }

            var fraction = totalAmountApplied / totalDigestateCreatedOnDay;

            var amountOfCarbon = fraction * totalCarbonAvailableOnDay;

            return amountOfCarbon;
        }

        public double GetTotalNitrogenExported(int year, Farm farm)
        {
            // Exports not supported yet
            return 0;
        }

        public double GetTotalCarbonRemainingAtEndOfYear(int year, Farm farm, AnaerobicDigestionComponent component)
        {
            var dailyResults = this.GetDailyResults(farm);
            if (dailyResults.Any() == false)
            {
                return 0;
            }

            var dateOfLastOutput = dailyResults.Max(x => x.Date);
            var tank = this.GetTank(farm, dateOfLastOutput, dailyResults);

            var totalCarbon = 0d;
            if (component.IsLiquidSolidSeparated)
            {
                totalCarbon = tank.CarbonFromLiquidDigestate + tank.CarbonFromSolidDigestate;
            }
            else
            {
                totalCarbon = tank.CarbonFromRawDigestate;
            }

            return totalCarbon;
        }

        // HERE - account for imports
        public double GetTotalAmountOfDigestateAppliedOnDay(
            DateTime dateTime, 
            Farm farm, 
            DigestateState state,
            ManureLocationSourceType sourceLocation)
        {
            var result = 0d;

            foreach (var farmFieldSystemComponent in farm.FieldSystemComponents)
            {
                foreach (var cropViewItem in farmFieldSystemComponent.CropViewItems)
                {
                    foreach (var digestateApplicationViewItem in cropViewItem.DigestateApplicationViewItems)
                    {
                        if (digestateApplicationViewItem.DateCreated.Date == dateTime.Date && digestateApplicationViewItem.DigestateState == state && digestateApplicationViewItem.ManureLocationSourceType == sourceLocation)
                        {
                            result += digestateApplicationViewItem.AmountAppliedPerHectare * cropViewItem.Area;
                        }
                    }
                }
            }

            return result;
        }

        public double GetTotalCarbonAppliedToField(CropViewItem cropViewItem, int year)
        {
            var result = 0d;

            foreach (var digestateApplicationViewItem in cropViewItem.DigestateApplicationViewItems.Where(x => x.DateCreated.Year == year))
            {
                result += digestateApplicationViewItem.AmountOfCarbonAppliedPerHectare * cropViewItem.Area;
            }

            return result;
        }

        public double GetTotalNitrogenRemainingAtEndOfYear(int year, Farm farm)
        {
            var dailyResults = this.GetDailyResults(farm);
            if (dailyResults.Any() == false)
            {
                return 0;
            }

            var dateOfLastOutput = dailyResults.Max(x => x.Date);
            var tank = this.GetTank(farm, dateOfLastOutput, dailyResults);

            var component = farm.GetAnaerobicDigestionComponent();

            var totalNitrogen = 0d;
            if (component.IsLiquidSolidSeparated)
            {
                // Total remaining N from liquid and solid fractions
                totalNitrogen = tank.NitrogenFromLiquidDigestate + tank.NitrogenFromSolidDigestate;
            }
            else
            {
                totalNitrogen = tank.NitrogenFromRawDigestate;
            }

            if (totalNitrogen >= 0)
            {
                return totalNitrogen;
            }
            else
            {
                return 0;
            }
        }

        public double GetTotalCarbonRemainingForField(CropViewItem cropViewItem, int year, Farm farm, AnaerobicDigestionComponent component)
        {
            var dailyResults = this.GetDailyResults(farm);
            if (dailyResults.Any() == false)
            {
                return 0;
            }

            var dateOfLastOutput = dailyResults.Max(x => x.Date);
            var tank = this.GetTank(farm, dateOfLastOutput, dailyResults);

            var totalCarbonRemaining = 0d;
            if (component.IsLiquidSolidSeparated)
            {
                // Total remaining C from liquid and solid fractions
                totalCarbonRemaining = tank.CarbonFromLiquidDigestate + tank.CarbonFromSolidDigestate;
            }
            else
            {
                totalCarbonRemaining = tank.CarbonFromRawDigestate;
            }

            var result = totalCarbonRemaining * (cropViewItem.Area / farm.GetTotalAreaOfFarm(false, year));

            return result;
        }

        /// <summary>
        /// Equations 4.9.7-1, 4.9.7-2, 4.9.7-5
        ///
        /// (kg C ha^-1)
        /// </summary>
        public double GetTotalDigestateCarbonInputsForField(Farm farm, int year, CropViewItem viewItem)
        {
            if (viewItem.CropType.IsNativeGrassland())
            {
                return 0;
            }

            var field = farm.GetFieldSystemComponent(viewItem.FieldSystemComponentGuid);
            if (field == null)
            {
                return 0;
            }

            var inputsFromLocalManure = 0d;
            if (field.HasLivestockDigestateApplicationsInYear(year))
            {
                inputsFromLocalManure = viewItem.GetTotalCarbonFromAppliedDigestate(ManureLocationSourceType.Livestock);
            }

            var component = farm.GetAnaerobicDigestionComponent();
            var remaining = this.GetTotalCarbonRemainingForField(viewItem, viewItem.Year, farm, component);

            return (inputsFromLocalManure + remaining) / field.FieldArea;
        }

        public double GetTotalCarbonForField(
            CropViewItem cropViewItem,
            int year,
            Farm farm,
            AnaerobicDigestionComponent component)
        {
            var totalAppliedToField = this.GetTotalCarbonAppliedToField(cropViewItem, year);
            var totalCarbonRemainingForField = this.GetTotalCarbonRemainingForField(cropViewItem, year, farm, component);

            var result = totalAppliedToField + totalCarbonRemainingForField;

            return result;
        }

        public void CalculateRawAmountsAvailable(DigestorDailyOutput outputOnCurrentDay,
            DateTime outputDate,
            Farm farm,
            int outputNumber,
            List<DigestateTank> result,
            DigestateTank tank,
            AnaerobicDigestionComponent component)
        {
            /*
             * Calculate raw amounts available
             */

            var totalAmountFromApplications = 0d;
            if (this.SubtractAmountsFromImportedDigestateLandApplications)
            {
                 totalAmountFromApplications += this.GetTotalAmountOfDigestateAppliedOnDay(outputDate, farm, DigestateState.Raw, ManureLocationSourceType.Imported);
            }

            totalAmountFromApplications += this.GetTotalAmountOfDigestateAppliedOnDay(outputDate, farm, DigestateState.Raw, ManureLocationSourceType.Livestock);

            // Raw digestate
            var totalRawDigestateOnThisDay = outputOnCurrentDay.FlowRateOfAllSubstratesInDigestate;
            var totalRawDigestateUsedForFieldApplications = totalAmountFromApplications;
            var totalRawDigestateFromPreviousDay = outputNumber == 0 ? 0 : result.ElementAt(outputNumber - 1).TotalRawDigestateAvailable;
            var totalRawProduced = totalRawDigestateOnThisDay + totalRawDigestateFromPreviousDay;
            var totalRawDigestateAvailableAfterFieldApplications = totalRawProduced - totalRawDigestateUsedForFieldApplications;

            // Nitrogen from raw digestate
            var totalNitrogenFromRawDigestateOnThisDay = outputOnCurrentDay.TotalAmountOfNitrogenFromRawDigestateAvailableForLandApplication;
            var totalNitrogenFromRawDigestateFromPreviousDay = outputNumber == 0 ? 0 : result.ElementAt(outputNumber - 1).NitrogenFromRawDigestate;
            var totalNitrogenFromRawDigestateAvailable = totalNitrogenFromRawDigestateOnThisDay + totalNitrogenFromRawDigestateFromPreviousDay;

            // Carbon from raw digestate
            var totalCarbonFromRawDigestateOnThisDay = outputOnCurrentDay.TotalAmountOfCarbonInRawDigestateAvailableForLandApplication;
            var totalCarbonFromRawDigestateFromPreviousDay = outputNumber == 0 ? 0 : result.ElementAt(outputNumber - 1).CarbonFromRawDigestate;
            var totalCarbonFromRawDigestateAvailable = totalCarbonFromRawDigestateOnThisDay + totalCarbonFromRawDigestateFromPreviousDay;

            // There should only be raw amounts if there is no separation performed
            if (component.IsLiquidSolidSeparated == false)
            {
                tank.TotalRawDigestateAvailable = totalRawDigestateAvailableAfterFieldApplications;
                tank.TotalRawDigestateProduced = totalRawProduced;
                tank.NitrogenFromRawDigestate = totalNitrogenFromRawDigestateAvailable;
                tank.CarbonFromRawDigestate = totalCarbonFromRawDigestateAvailable;

                var totalFractionOfRawDigestateUsedFromLandApplications = totalRawDigestateUsedForFieldApplications / totalRawProduced;
                var totalCarbonUsed = totalFractionOfRawDigestateUsedFromLandApplications * totalCarbonFromRawDigestateAvailable;
                var totalNitrogenUsed = totalFractionOfRawDigestateUsedFromLandApplications * totalNitrogenFromRawDigestateAvailable;

                if (this.SubtractAmountsFromLandApplications)
                {
                    tank.CarbonFromRawDigestate -= totalCarbonUsed;
                    tank.NitrogenFromRawDigestate -= totalNitrogenUsed;
                }
            }
        }

        public void CalculateLiquidAmountsAvailable(
            DigestorDailyOutput outputOnCurrentDay,
            DateTime outputDate,
            Farm farm,
            int outputNumber,
            List<DigestateTank> result,
            DigestateTank tank,
            AnaerobicDigestionComponent component)
        {
            var totalLiquidFractionOnThisDay = outputOnCurrentDay.FlowRateLiquidFraction;
            var totalLiquidDigestateUsedForFieldApplications = this.GetTotalAmountOfDigestateAppliedOnDay(outputDate, farm, DigestateState.LiquidPhase, ManureLocationSourceType.Livestock);
            var totalLiquidDigestateFromPreviousDay = outputNumber == 0 ? 0 : result.ElementAt(outputNumber - 1).TotalLiquidDigestateAvailable;
            var totalLiquidProduced = totalLiquidFractionOnThisDay + totalLiquidDigestateFromPreviousDay;
            var totalLiquidDigestateAvailableAfterFieldApplications = totalLiquidProduced - totalLiquidDigestateUsedForFieldApplications;

            // Nitrogen from liquid digestate
            var totalNitrogenFromLiquidDigestateOnThisDay = outputOnCurrentDay.TotalAmountOfNitrogenInRawDigestateAvailableForLandApplicationFromLiquidFraction;
            var totalNitrogenFromLiquidDigestateFromPreviousDay = outputNumber == 0 ? 0 : result.ElementAt(outputNumber - 1).NitrogenFromLiquidDigestate;
            var totalNitrogenFromLiquidDigestateAvailable = totalNitrogenFromLiquidDigestateOnThisDay + totalNitrogenFromLiquidDigestateFromPreviousDay;

            // Carbon from liquid digestate
            var totalCarbonFromLiquidDigestateOnThisDay = outputOnCurrentDay.TotalAmountOfCarbonInRawDigestateAvailableForLandApplicationFromLiquidFraction;
            var totalCarbonFromLiquidDigestateFromPreviousDay = outputNumber == 0 ? 0 : result.ElementAt(outputNumber - 1).CarbonFromLiquidDigestate;
            var totalCarbonFromLiquidDigestateAvailable = totalCarbonFromLiquidDigestateOnThisDay + totalCarbonFromLiquidDigestateFromPreviousDay;

            if (component.IsLiquidSolidSeparated)
            {
                tank.TotalLiquidDigestateAvailable = totalLiquidDigestateAvailableAfterFieldApplications;
                tank.TotalLiquidDigestateProduced = totalLiquidProduced;
                tank.NitrogenFromLiquidDigestate = totalNitrogenFromLiquidDigestateAvailable;
                tank.CarbonFromLiquidDigestate = totalCarbonFromLiquidDigestateAvailable;

                var totalFractionOfLiquidDigestateUsedFromLandApplications = totalLiquidDigestateUsedForFieldApplications / totalLiquidProduced;
                var totalCarbonUsed = totalFractionOfLiquidDigestateUsedFromLandApplications * totalCarbonFromLiquidDigestateAvailable;
                var totalNitrogenUsed = totalFractionOfLiquidDigestateUsedFromLandApplications * totalNitrogenFromLiquidDigestateAvailable;

                if (this.SubtractAmountsFromLandApplications)
                {
                    tank.CarbonFromLiquidDigestate -= totalCarbonUsed;
                    tank.NitrogenFromLiquidDigestate -= totalNitrogenUsed;
                }
            }
        }

        public void CalculateSolidAmountsAvailable(
            DigestorDailyOutput outputOnCurrentDay,
            DateTime outputDate,
            Farm farm,
            int outputNumber,
            List<DigestateTank> result,
            DigestateTank tank,
            AnaerobicDigestionComponent component)
        {
            /*
             * Calculate solid amounts available
             */

            var totalSolidFractionOnThisDay = outputOnCurrentDay.FlowRateSolidFraction;
            var totalSolidDigestateUsedForFieldApplications = this.GetTotalAmountOfDigestateAppliedOnDay(outputDate, farm, DigestateState.SolidPhase, ManureLocationSourceType.Livestock);
            var totalSolidDigestateFromPreviousDay = outputNumber == 0 ? 0 : result.ElementAt(outputNumber - 1).TotalSolidDigestateAvailable;
            var totalSolidProduced = totalSolidFractionOnThisDay + totalSolidDigestateFromPreviousDay;
            var totalSolidDigestateAvailableAfterFieldApplications = totalSolidProduced - totalSolidDigestateUsedForFieldApplications;

            //// Nitrogen from solid digestate
            var totalNitrogenFromSolidDigestateOnThisDay = outputOnCurrentDay.TotalAmountOfNitrogenInRawDigestateAvailableForLandApplicationFromSolidFraction;
            var totalNitrogenFromSolidDigestateFromPreviousDay = outputNumber == 0 ? 0 : result.ElementAt(outputNumber - 1).NitrogenFromSolidDigestate;
            var totalNitrogenFromSolidDigestateAvailable = totalNitrogenFromSolidDigestateOnThisDay + totalNitrogenFromSolidDigestateFromPreviousDay;

            //// Carbon from solid digestate
            var totalCarbonFromSolidDigestateOnThisDay = outputOnCurrentDay.TotalAmountOfCarbonInRawDigestateAvailableForLandApplicationFromSolidFraction;
            var totalCarbonFromSolidDigestateFromPreviousDay = outputNumber == 0 ? 0 : result.ElementAt(outputNumber - 1).CarbonFromSolidDigestate;
            var totalCarbonFromSolidDigestateAvailable = totalCarbonFromSolidDigestateOnThisDay + totalCarbonFromSolidDigestateFromPreviousDay;

            if (component.IsLiquidSolidSeparated)
            {
                tank.TotalSolidDigestateAvailable = totalSolidDigestateAvailableAfterFieldApplications;
                tank.TotalSolidDigestateProduced = totalSolidProduced;
                tank.NitrogenFromSolidDigestate = totalNitrogenFromSolidDigestateAvailable;
                tank.CarbonFromSolidDigestate = totalCarbonFromSolidDigestateAvailable;

                var totalFractionOfSolidDigestateUsedFromLandApplications = totalSolidDigestateUsedForFieldApplications / totalSolidProduced;
                var totalCarbonUsed = totalFractionOfSolidDigestateUsedFromLandApplications * totalCarbonFromSolidDigestateAvailable;
                var totalNitrogenUsed = totalFractionOfSolidDigestateUsedFromLandApplications * totalNitrogenFromSolidDigestateAvailable;

                if (this.SubtractAmountsFromLandApplications)
                {
                    tank.CarbonFromSolidDigestate -= totalCarbonUsed;
                    tank.NitrogenFromSolidDigestate -= totalNitrogenUsed;
                }
            }
        }

        #endregion

        #region Private Methods

        #endregion
    }
}