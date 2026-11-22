using System;
using System.Collections.Generic;
using System.Linq;


namespace SeventWebApplication.Models.Maintenance.Repsitories
{

    public class MaintenanceRepository

    {
        private readonly List<MaintenanceRecord> _records = new;
        
        public void AddMaintenanceRecord(MaintenanceRecord record)
        {

            // exeption handling for null record

            _records.Add(record);
        }

        public MaintenanceRecord GetById(Guid maintenanceId) => _records.Find(r => r.MaintenanceId == maintenanceId);

        public IEnumerable<MaintenanceRecord> GetByVehicle(Guid vehicleId) => _records.Where(r => r.VehicleId == vehicleId)
            .OrderByDescending(r => r.StartDate);
            .ToList();

        public IEnumerable<MaintenanceRecord> GetUpcomingServices(DateTime from) => _records.Where(r => !r.IsCompleted && r.StartDate >= from)
            .OrderBy(r=> r.StartDate)
            .ToList();


        public void Update(MaintenanceRecord record)
        {
            ArgumentNullException.ThrowIfNull(record);

            int index = _records.FindIndex(r => r.MaintenanceId == record.MaintenanceId);

            if (index <0)
                throw new KeyNotFoundException($"Maintenance record with ID {record.MaintenanceId} not found.");

            _records[index] = record;
        } 

        public IEnumerable<MaintenanceRecord> GetAll() => _records.ToList();
    }


//MaintenanceRepository	
//-List<MaintenanceRecord> _records  
//+Add(record: MaintenanceRecord) : void
//+GetById : MaintenanceRecord
//+GetByVehicle(VehicleId)
//+GetUpcomingServices(datetime) IEnumerable

//+Update(record) : void

//+GetAll : IEnumerable<MaintenanceRecord>