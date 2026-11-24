using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using SeventWebApplication.Models.Maintenance;
using SeventWebApplication.Models.VehicleNS;
using SeventWebApplication.Models.Maintenance.Repsitories;
using SeventWebApplication.Models.Booking;



namespace SeventWebApplication.Models.Maintenance.Services
{

    public class VehicleMaintenanceService
    {
    private readonly MaintenanceRepository _maintenanceRepository;
    private readonly FleetManager _fleetManager;
    private readonly BookingRepository _bookingRepository;
    // Repot (singleton) och inte VehicleDamageService (scoped), annars hamnar ett scoped beroende i en singleton
    private readonly VehicleDamageRepository _damageRepository;


    public VehicleMaintenanceService (MaintenanceRepository maintenanceRepository, FleetManager fleetManager, BookingRepository bookingRepository, VehicleDamageRepository damageRepository)
    {
        _maintenanceRepository = maintenanceRepository;
        _fleetManager = fleetManager;
        _bookingRepository = bookingRepository;
        _damageRepository = damageRepository;
    } // lägg till nullexepctions


    public MaintenanceRecord RegisterMaintenance( Guid vehicleId, MaintenanceType type, string description, DateTime startDate, DateTime endDate, int? mileage = null, decimal? cost = null, string? notes = null)
    {
            EnsureVehicleExists(vehicleId);
            if (endDate < startDate)
            {
                throw new ArgumentException("End date cannot be earlier than start date.");
            }

            var maintenanceRecord = new MaintenanceRecord
        {
            VehicleId = vehicleId,
            Type = type,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            Mileage = mileage,
            Cost = cost,
            Notes = notes
        };
        _maintenanceRepository.AddMaintenanceRecord(maintenanceRecord); // Fixad: hette Add, repot har AddMaintenanceRecord
        return maintenanceRecord;
        }


        public MaintenanceRecord ScheduleMaintenance(Guid vehicleId, MaintenanceType type, string description, DateTime startDate, DateTime endDate, int? mileage = null, decimal? cost = null, string? notes = null)
        {
            var vehicle = EnsureVehicleExists (vehicleId);
            if (endDate < startDate)
            {
                throw new ArgumentException("End date cannot be earlier than start date.");
            }
            EnsureNoBookingsDuring(vehicleId, startDate, endDate);

            var maintenanceRecord = new MaintenanceRecord
            {
                VehicleId = vehicleId,
                Type = type,
                Description = description,
                StartDate = startDate,
                EndDate = endDate,
                Mileage = mileage,
                Cost = cost,
                Notes = notes
            };
            _maintenanceRepository.AddMaintenanceRecord(maintenanceRecord); // Fixad: hette Add, repot har AddMaintenanceRecord
            _fleetManager.AddMaintenancePeriod(new MaintenancePeriod             
            {
                MaintenanceId = maintenanceRecord.MaintenanceId,
                VehicleId = vehicleId,
                StartDate = startDate,
                EndDate = endDate,
                Reason = description
            });

            return maintenanceRecord;
        }

        public MaintenanceRecord CompleteMaintenance(Guid maintenanceId, DateTime completedAt, int? mileage = null, decimal? cost = null, string? notes = null )
        {

            var maintenanceRecord = _maintenanceRepository.GetById(maintenanceId);
            if (maintenanceRecord == null)
            {
                throw new ArgumentException("Maintenance record not found.");
            }

            if (maintenanceRecord.IsCompleted)
            {
                throw new InvalidOperationException("Maintenance is already completed.");
            }
            if (completedAt < maintenanceRecord.StartDate)
            {
                throw new ArgumentException("Completion date cannot be earlier than start date.");
            }

            var updatedRecord = maintenanceRecord with
            {
                EndDate = completedAt,
                Mileage = mileage ?? maintenanceRecord.Mileage,
                Cost = cost ?? maintenanceRecord.Cost,
                Notes = notes ?? maintenanceRecord.Notes
            };
            _maintenanceRepository.Update(updatedRecord);
            _fleetManager.EndMaintenancePeriod(maintenanceId, completedAt);
            ReleaseIfNoOpenRecord(EnsureVehicleExists(updatedRecord.VehicleId));
            return updatedRecord;
        }

        // Tar bilen ur drift: skapar en öppen post + period och sätter bilen InService.
        public void MarkVehicleOutOfService(Guid vehicleId, string reason, MaintenanceType type = MaintenanceType.Repair)
        {
            var vehicle = EnsureVehicleExists(vehicleId);

            if (vehicle.VehicleStatus == VehicleStatus.InService || HasOpenRecord(vehicleId))
                throw new InvalidOperationException("The vehicle is already in service, no change needed.");

            if (!vehicle.IsAvailable)
                throw new InvalidOperationException("The vehicle is rented out and cannot be put in service.");

            var now = DateTime.UtcNow;
            EnsureNoBookingsDuring(vehicleId, now, null);


            var maintenanceRecord = new MaintenanceRecord
            {
                VehicleId = vehicleId,
                Type = type,
                Description = reason,
                StartDate = DateTime.UtcNow,
                EndDate = null // Indicating that the maintenance is accuring and the vehicle is out of service
            };


            _maintenanceRepository.AddMaintenanceRecord(maintenanceRecord); // Fixad: hette Add, repot har AddMaintenanceRecord
            _fleetManager.AddMaintenancePeriod(new MaintenancePeriod
            {
                MaintenanceId = maintenanceRecord.MaintenanceId,
                VehicleId = vehicleId,
                StartDate = now,
                EndDate = null, //  maintenance
                Reason = reason
            });

            vehicle.ChangeStatus(VehicleStatus.InService);
        }

        // Tar bilen tillbaka i drift: avslutar pågående poster och släpper bilen (Active, eller InRepair vid olösta skador).
        public void MarkVehicleActive(Guid vehicleId)
        {

            var vehicle = EnsureVehicleExists(vehicleId);

            if (vehicle.VehicleStatus != VehicleStatus.InService && !HasOpenRecord(vehicleId))
                throw new InvalidOperationException("The vehicle is not out of service.");

            var now = DateTime.UtcNow;

            var ongoingMaintenance = _maintenanceRepository.GetActiveMaintenanceByVehicleId(vehicleId)
            .Where(m => !m.IsCompleted && m.StartDate <= now)
            .ToList();


            foreach (var maintenance in ongoingMaintenance)
            {
                CompleteMaintenance(maintenance.MaintenanceId, now);
            }

            // Säkerhetsnät: bil som står som InService utan någon post att avsluta
            ReleaseIfNoOpenRecord(vehicle);
        }

        // Regeln: InService gäller om och bara om bilen har en öppen post (EndDate == null)
        private bool HasOpenRecord(Guid vehicleId)
            => _maintenanceRepository.GetByVehicle(vehicleId).Any(r => r.EndDate == null);

        private void ReleaseIfNoOpenRecord(Vehicle vehicle)
        {
            if (vehicle.VehicleStatus != VehicleStatus.InService || HasOpenRecord(vehicle.VehicleId))
                return;

            bool hasOpenDamages = _damageRepository.GetByVehicleId(vehicle.VehicleId).Any(d => !d.IsResolved);
            vehicle.ChangeStatus(hasOpenDamages ? VehicleStatus.InRepair : VehicleStatus.Active);
        }

        public IEnumerable<MaintenanceRecord> GetMaintenanceHistory(Guid vehicleId)
            => _maintenanceRepository.GetByVehicle(vehicleId);

            public IEnumerable<MaintenanceRecord> GetUpcomingMaintenance(DateTime? from = null)
            => _maintenanceRepository.GetUpcomingServices(from ?? DateTime.Now);
        /// Unsure of this one.
        /// 

        // Bilen kan inte tas ur drift eller bokas för underhåll om en Pending-, Confirmed- eller Active-bokning
        // överlappar perioden. end = null betyder tillsvidare. En försenad Active-bokning räknas som pågående tills bilen lämnas tillbaka.
        // Felet anger antal krockande bokningar så att admin kan flytta eller avboka dem först.
        private void EnsureNoBookingsDuring(Guid vehicleId, DateTime start, DateTime? end)
        {
            var now = DateTime.Now;
            int conflicts = _bookingRepository.GetAll().Count(b =>
                b.Status is BookingStatus.Pending or BookingStatus.Confirmed or BookingStatus.Active
                && b.Vehicle != null
                && b.Vehicle.VehicleId == vehicleId
                && (end == null || b.StartDate < end.Value.ToLocalTime())
                && start.ToLocalTime() < (b.Status == BookingStatus.Active && b.EndDate < now ? now : b.EndDate));

            if (conflicts > 0)
                throw new InvalidOperationException(
                    $"The vehicle has {conflicts} pending, confirmed or active booking(s) overlapping the maintenance period. " +
                    "Move or cancel them first, or choose a period after them.");
        }

        public Vehicle EnsureVehicleExists(Guid vehicleId)
        {
            return _fleetManager.GetVehicle(vehicleId) ?? throw new ArgumentException($"Vehicle {vehicleId} not found.");

            // oklart om vår fleetmanager 

            //if (vehicle == null)
            //{
            //    throw new ArgumentException("Vehicle not found.");
            //}
        }
    }
}


//VehicleMaintenanceService	
//-MaintenanceRepository _maintenanceRepository
//-FleetManager _fleetManager 
//+RegisterMaintenance : void 
////+ScheduleMaintenance : void
//+CompleteMaintenance : void
//+MarkVehicleOutOfService : void
//+MarkVehicleActive : void
//+GetMaintenanceHistory : IEnumerable
//+GetUpcomingMaintenance : IEnumerable