using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Example.Models;
using SummerBoot.Cache;
using SummerBoot.Core;
using SummerBoot.Repository;

namespace Example.Service
{
    public class PersonService : IPersonService
    {
        [Autowired]
        private IPersonRepository IPersonRepository { set; get; }

        public Person InsertPerson(Person person)
        {
            return new Person();
            //return IPersonRepository.Insert(person);
        }

        [Cacheable("db1", "Person", cacheManager: "redis")]
        public Person FindPerson(string name)
        {
            return new Person();
            //return IPersonRepository.GetPersonsByName(name);
        }

        [CachePut("db1", key: "Person", cacheManager: "redis")]
        public Person UpdatePerson(Person person)
        {
            return new Person() { Name = "春光灿烂喜洋洋", Id = 3, Age = 5 };
        }

        [CacheEvict("db1", "Person", cacheManager: "redis")]
        public bool DeletePerson(Person person)
        {
            return true;
        }

















        [CacheEvict("db1", "Person", cacheManager: "redis")]
        public Task<bool> DeletePersonAsync(Person person)
        {
            return Task.FromResult(true);
        }

       

        [Cacheable("db1", "Person", cacheManager: "redis")]
        public Task<Person> FindPersonAsync(string name)
        {
            return Task.FromResult(new Person() { Name = name, Id = 1, Age = 3 });
        }

        [Cacheable("db1", "Datetime", cacheManager: "redis")]
        public DateTime? GetDateTime()
        {
            return null;
        }

     

        [CachePut("db1", key: "Person", cacheManager: "redis")]
        public Task<Person> UpdatePersonAsync(Person person)
        {
            return Task.FromResult(new Person() { Name = "春光灿烂喜洋洋", Id = 3, Age = 5 });
        }
    }
}