using System;
using System.Threading.Tasks;
using Example.Models;

namespace Example.Service
{
    public interface IPersonService
    {
        Person InsertPerson(Person person);
        Person FindPerson(string name);

        Person UpdatePerson(Person person);

        bool DeletePerson(Person person);

        Task<Person> FindPersonAsync(string name);

        Task<Person> UpdatePersonAsync(Person person);

        Task<bool> DeletePersonAsync(Person person);

        DateTime? GetDateTime();
    }
}