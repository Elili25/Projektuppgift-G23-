using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using SeventWebApplication.Models.Maintenance;


namespace SeventWebApplication.Models.Maintenance.Services
{

    public class VehicleMaintenanceService
    {
    private readonly MaintenanceRepository _maintenanceRepository;
    private readonly FleetManager _fleetManager;


    public VehicleMaintenanceService (MaintenanceRepository maintenanceRepository, FleetManager fleetManager)
    {
        _maintenanceRepository = maintenanceRepository;
        _fleetManager = fleetManager;
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
        _maintenanceRepository.Add(maintenanceRecord);
        return maintenanceRecord;
        }


        public MaintenanceRecord ScheduleMaintenance(Guid vehicleId, MaintenanceType type, string description, DateTime startDate, DateTime endDate, int? mileage = null, decimal? cost = null, string? notes = null)
        {
            var vehicle = EnsureVehicleExists (vehicleId);
            if (plannedEndDate < startDate)
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
            _maintenanceRepository.Add(maintenanceRecord);
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
            return updatedRecord;
        }
        
        pubic MarkVehicleOutOfService(Guid vehicleId, string reason, MaintenanceType type = MaintenanceType.Repair)
        {
            EnsureVehicleExists(vehicleId);
            var now = DateTime.UtcNow;


            var maintenanceRecord = new MaintenanceRecord
            {
                VehicleId = vehicleId,
                Type = type,
                Description = reason,
                StartDate = DateTime.UtcNow,
                EndDate = null // Indicating that the maintenance is accuring and the vehicle is out of service
            };


            _maintenanceRepository.Add(maintenanceRecord);
            _fleetManager.AddMaintenancePeriod(new MaintenancePeriod
            {
                MaintenanceId = maintenanceRecord.MaintenanceId,
                VehicleId = vehicleId,
                StartDate = now,
                EndDate = null, //  maintenance
                Reason = reason
            });
        }
        
        public void MarkVehicleActive(Guid vehicleId)
        {

            EnsureVehicleExists(vehicleId);
            var now = DateTime.UtcNow;

            var ongoing maintenance = _maintenanceRepository.GetActiveMaintenanceByVehicleId(vehicleId);

            .Where(m => !m.IsCompleted && m.StartDate <= now)
            .ToList();


            for each (var maintenance in ongoingMaintenance)
            {
                CompleteMaintenance(maintenance.MaintenanceId, now);
            }

            /// Exeption handling if no active maintenance records found? 

            //var maintenanceRecord = _maintenanceRepository.GetActiveMaintenanceByVehicleId(vehicleId);
            //if (maintenanceRecord == null)
            //{
            //    throw new ArgumentException("No active maintenance record found for this vehicle.");
            //}
            //CompleteMaintenance(maintenanceRecord.MaintenanceId, completedAt, mileage, cost, notes);

            public IEnumerable<MaintenanceRecord> GetMaintenanceHistory(Guid vehicleId)
            => _maintenanceRepository.GetByVehicleId(vehicleId)

            public IEnumerable<MaintenanceRecord> GetUpcomingMaintenance(DateTime ? = null)
            => _maintenanceRepository.GetUpcomnigServices(DateTime ?? DateTime.Now);
        /// Unsure of this one.
        /// 

        public Vehicle EnsureVehicleExists(Guid vehicleId)
        {
            var vehicle = _fleetManager.GetVehicleById(vehicleId);
            => _fleetManager.GetVehicleById(vehicleId) ?? throw new ArgumentException($"Vehicle {vehicleId} not found.");

            // oklart om vår fleetmanager 

            //if (vehicle == null)
            //{
            //    throw new ArgumentException("Vehicle not found.");
            //}
          
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