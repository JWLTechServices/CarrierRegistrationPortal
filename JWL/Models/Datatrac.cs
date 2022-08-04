using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    class Datatrac
    {
    }
    public class DatatracSettings
    {
        public string DatatracUserName { get; set; }
        public string DatatracPassword { get; set; }
        public string DatatracUrl { get; set; }
    }


    public class amazon_equipment_owner_Request
    {
        public amazon_equipment_owner amazon_equipment_owner { get; set; }
    }

    public class amazon_equipment_owner
    {
        public string owner_id { get; set; }
        public string owner_name { get; set; }
        public int key_id { get; set; }
        public string dx_vendor_id { get; set; }
        public string ascend_vendor_id { get; set; }
        public string check_name { get; set; }
        public string carrier_type { get; set; }
        public string carrier_status { get; set; }
        public string scac_code { get; set; }
    }
    
    public enum CarrierStatusEnum
    {
        APPD, //APPD = Approved,
        HOLD, // HOLD = On Hold,
        PEND, // PEND = Pending,
        TERM  // TERM = Terminated
    }
    public enum CarrierTypeEnum
    {

        CARR,// CARR = Carrier, 
        IC //IC = Independent Contractor
    }
    public class amazon_equipment_owner_Response
    {
        public bool success { get; set; }
        public double score { get; set; }
        public string action { get; set; }
        public DateTime challenge_ts { get; set; }
        public string hostname { get; set; }
    }
}
