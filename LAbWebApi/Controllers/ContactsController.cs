using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Http;

namespace LabWebApi.Controllers
{
    public class ContactsController : ApiController
    {
        public ContactsController()
        {
            var acct = CloudStorageAccount.Parse(
                ConfigurationManager.ConnectionStrings["tableStorageConnString"].ConnectionString
                );

            tableClient = acct.CreateCloudTableClient();
        }

        private const string _tableName = "Contacts";

        private readonly CloudTableClient tableClient;

        private CloudTable GetTableClientReference()
        {
            // Get reference to the Storage Table
            var table = this.tableClient.GetTableReference(_tableName);

            // Create the Table if it doesn't already exist
            table.CreateIfNotExists();

            return table;
        }

        // GET api/contacts
        public IEnumerable<Contact> Get()
        {
            var table = this.GetTableClientReference();

            var operation = new TableQuery<Contact>();

            return table.ExecuteQuery(operation);
        }

        /*
        // GET api/contacts/{lastname}-{firstname}
        public Contact Get(string id)
        {
            var parts = id.Split('-');

            var table = this.GetTableClientReference();

            // Get existing entity
            var retrieveOperation = TableOperation.Retrieve<Contact>(parts[0], parts[1]);
            var result = table.Execute(retrieveOperation).Result;

            var contactEntity = result as Contact;

            return contactEntity;
        }
        */

        // POST api/contacts
        public void Post([FromBody] Contact value)
        {
            var table = this.GetTableClientReference();

            // Save existing Contact
            var operation = TableOperation.Retrieve<Contact>(value.PartitionKey, value.RowKey);
            var result = table.Execute(operation);

            var contactEntity = result.Result as Contact;
            if (contactEntity == null)
            {
                throw new KeyNotFoundException("Contact not found");
            }

            contactEntity.Address1 = value.Address1;
            contactEntity.Address2 = value.Address2;
            contactEntity.Age = value.Age;
            contactEntity.City = value.City;
            contactEntity.PostalCode = value.PostalCode;
            contactEntity.PrimaryPhone = value.PrimaryPhone;
            contactEntity.StateProv = value.StateProv;

            // Update existing Contact
            var updateOperation = TableOperation.Replace(contactEntity);
            table.Execute(updateOperation);
        }

        // PUT api/contacts
        public void Put([FromBody] Contact value)
        {
            var table = this.GetTableClientReference();

            // Insert new Contact
            var operation = TableOperation.Insert(value);
            table.Execute(operation);
        }

        // DELETE api/contacts/{lastname}-{firstname}
        public void Delete(string id)
        {
            var parts = id.Split('-');

            var table = this.GetTableClientReference();

            // Get existing entity
            var retrieveOperation = TableOperation.Retrieve<Contact>(parts[0], parts[1]);
            var contactEntity = table.Execute(retrieveOperation).Result as TableEntity;

            if (contactEntity != null)
            {
                // Delete entity
                var deleteOperation = TableOperation.Delete(contactEntity);
                table.Execute(deleteOperation);
            }
        }
    }

    public class Contact : TableEntity
    {
        public Contact() { }

        public Contact(string firstName, string lastName)
            : this()
        {
            this.PartitionKey = lastName;
            this.RowKey = firstName;
        }

        public int Age { get; set; }

        public string PrimaryPhone { get; set; }

        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string StateProv { get; set; }
        public string PostalCode { get; set; }
    }
}

